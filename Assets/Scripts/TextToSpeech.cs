using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine;
using IBM.Watson.TextToSpeech.V1;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.Authentication.Iam;



public class TextToSpeech : MonoBehaviour
{
    #region PLEASE SET THESE VARIABLES IN THE INSPECTOR
    [Header("Text to Speech")]
    [SerializeField]
    [Tooltip("The service URL (optional). This defaults to \"https://stream.watsonplatform.net/text-to-speech/api\"")]
    private string serviceUrl;
    [Header("IAM Authentication")]
    [Tooltip("The IAM apikey.")]
    [SerializeField]
    private string iamApikey;
    [Tooltip("The IAM url used to authenticate the apikey (optional). This defaults to \"https://iam.bluemix.net/identity/token\".")]
    [SerializeField]
    private string iamUrl;
    #endregion

    private TextToSpeechService service;
    public static TextToSpeech Instance;
    [SerializeField] Actor actor;

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

        service = new TextToSpeechService(authenticator);
        if (!string.IsNullOrEmpty(serviceUrl))
        {
            service.SetServiceUrl(serviceUrl);
        }

        //Runnable.Run(CallTextToSpeech("Hello there, this is Watson!"));
    }

    public void Run(string outputText)
    {
        Runnable.Run(CallTextToSpeech(outputText));
    }

    private IEnumerator CallTextToSpeech(string outputText)
    {
        byte[] synthesizeResponse = null;
        AudioClip clip = null;
        service.Synthesize(
            callback: (DetailedResponse<byte[]> response, IBMError error) =>
            {
                synthesizeResponse = response.Result;
                clip = WaveFile.ParseWAV("myClip", synthesizeResponse);
                actor.PlayClip(clip);

                SpeechToText.Instance.RecordAgain(clip.length);
            },
            text: outputText,
            voice: "en-US_MichaelVoice",
            accept: "audio/wav"
        );

        while (synthesizeResponse == null)
            yield return null;

        yield return new WaitForSeconds(clip.length);
    }
}
