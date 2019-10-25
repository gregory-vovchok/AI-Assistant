#pragma warning disable 0649


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IBM.Watson.SpeechToText.V1;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Authentication;
using IBM.Cloud.SDK.Authentication.Iam;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.DataTypes;
using TMPro;

public class SpeechToText : MonoBehaviour
{
    #region PLEASE SET THESE VARIABLES IN THE INSPECTOR
    [Tooltip("Text field to display the results of streaming.")]
    public TextMeshProUGUI ResultsField;
    [Tooltip("The service URL (optional). This defaults to \"https://stream.watsonplatform.net/speech-to-text/api\"")]
    [SerializeField]
    private string serviceUrl;
    [Header("IAM Authentication")]
    [Tooltip("The IAM apikey.")]
    [SerializeField]
    private string iamApikey;

    [Header("Parameters")]
    // https://www.ibm.com/watson/developercloud/speech-to-text/api/v1/curl.html?curl#get-model
    [Tooltip("The Model to use. This defaults to en-US_BroadbandModel")]
    [SerializeField]
    private string recognizeModel;
    #endregion

    public static SpeechToText Instance;
    private int recordingRoutine = 0;
    private string microphoneID = null;
    private AudioClip recording = null;
    private int recordingBufferSize = 1;
    private int recordingHZ = 22050;

    private SpeechToTextService service;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        LogSystem.InstallDefaultReactors();
        Runnable.Run(CreateService());
    }

    private IEnumerator CreateService()
    {
        if (string.IsNullOrEmpty(iamApikey))
        {
            throw new IBMException("Plesae provide IAM ApiKey for the service.");
        }

        IamAuthenticator authenticator = new IamAuthenticator(apikey: iamApikey);

        //  Wait for tokendata
        while (!authenticator.CanAuthenticate())
            yield return null;

        service = new SpeechToTextService(authenticator);
        if (!string.IsNullOrEmpty(serviceUrl))
        {
            service.SetServiceUrl(serviceUrl);
        }
        service.StreamMultipart = true;

        Active = true;
        StartRecording();
    }

    public bool Active
    {
        get { return service.IsListening; }
        set
        {
            if (value && !service.IsListening)
            {
                service.RecognizeModel = (string.IsNullOrEmpty(recognizeModel) ? "en-US_BroadbandModel" : recognizeModel);
                service.DetectSilence = true;
                service.EnableWordConfidence = true;
                service.EnableTimestamps = true;
                service.SilenceThreshold = 0.01f;
                service.MaxAlternatives = 1;
                service.EnableInterimResults = true;
                service.OnError = OnError;
                service.InactivityTimeout = -1;
                service.ProfanityFilter = false;
                service.SmartFormatting = true;
                service.SpeakerLabels = false;
                service.WordAlternativesThreshold = null;
                service.StartListening(OnRecognize, OnRecognizeSpeaker);
            }
            else if (!value && service.IsListening)
            {
                service.StopListening();
            }
        }
    }

    private void StartRecording()
    {
        if (recordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
            recordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    private void StopRecording()
    {
        if (recordingRoutine != 0)
        {
            Microphone.End(microphoneID);
            Runnable.Stop(recordingRoutine);
            recordingRoutine = 0;
        }
    }

    private void OnError(string error)
    {
        Active = false;

        Log.Debug("ExampleStreaming.OnError()", "Error! {0}", error);
    }

    private IEnumerator RecordingHandler()
    {
        Log.Debug("ExampleStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
        recording = Microphone.Start(microphoneID, true, recordingBufferSize, recordingHZ);
        yield return null; // let recordingRoutine get set..

        if (recording == null)
        {
            StopRecording();
            yield break;
        }

        bool bFirstBlock = true;
        int midPoint = recording.samples / 2;
        float[] samples = null;

        while (recordingRoutine != 0 && recording != null)
        {
            int writePos = Microphone.GetPosition(microphoneID);
            if (writePos > recording.samples || !Microphone.IsRecording(microphoneID))
            {
                Log.Error("ExampleStreaming.RecordingHandler()", "Microphone disconnected.");

                StopRecording();
                yield break;
            }

            if ((bFirstBlock && writePos >= midPoint)
              || (!bFirstBlock && writePos < midPoint))
            {
                // front block is recorded, make a RecordClip and pass it onto our callback.
                samples = new float[midPoint];
                recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
                record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
                record.Clip = AudioClip.Create("Recording", midPoint, recording.channels, recordingHZ, false);
                record.Clip.SetData(samples, 0);

                service.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                // calculate the number of samples remaining until we ready for a block of audio, 
                // and wait that amount of time it will take to record.
                int remaining = bFirstBlock ? (midPoint - writePos) : (recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)recordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }
        }
        yield break;
    }

    private void OnRecognize(SpeechRecognitionEvent result)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    //string text = string.Format("{0} ({1}, {2:0.00})\n", alt.transcript, res.final ? "Final" : "Interim", alt.confidence);
                    //Log.Debug("ExampleStreaming.OnRecognize()", text);
                    //ResultsField.text = text;

                    if (res.final && alt.confidence > 0)
                    {
                        StopRecording();
                        string inputText = alt.transcript;
                        //Debug.Log("Watson hears : " + text + " Confidence: " + alt.confidence);
                        ResultsField.text = inputText;
                        ResultsField.color = new Color(0, 0, 1);
                        Assistant.Instance.BuildSpokenRequest(inputText);
                    }
                }

                if (res.keywords_result != null && res.keywords_result.keyword != null)
                {
                    foreach (var keyword in res.keywords_result.keyword)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "keyword: {0}, confidence: {1}, start time: {2}, end time: {3}", keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
                    }
                }
                 
                if (res.word_alternatives != null)
                {
                    foreach (var wordAlternative in res.word_alternatives)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "Word alternatives found. Start time: {0} | EndTime: {1}", wordAlternative.start_time, wordAlternative.end_time);
                        foreach (var alternative in wordAlternative.alternatives)
                        Log.Debug("ExampleStreaming.OnRecognize()", "\t word: {0} | confidence: {1}", alternative.word, alternative.confidence);
                    }
                }
            }
        }
    }

    private void OnRecognizeSpeaker(SpeakerRecognitionEvent result)
    {
        if (result != null)
        {
            foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
            {
                Log.Debug("ExampleStreaming.OnRecognizeSpeaker()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
            }
        }
    }

    public void RecordAgain(float delay)
    {
        //Debug.Log("Played Audio received from Watson Text To Speech");
        StartCoroutine(OnListen(delay));
    }

    private IEnumerator OnListen(float delay)
    {
        yield return new WaitForSeconds(delay);

        Log.Debug("AvatarPattern.OnListen", "Start();");

        Active = true;

        StartRecording();
    }
}

