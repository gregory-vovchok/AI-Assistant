using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IBM.Watson.Assistant.V2;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.Authentication.Iam;
using IBM.Watson.Assistant.V2.Model;
using TMPro;


public class Assistant : MonoBehaviour
{
    #region PLEASE SET THESE VARIABLES IN THE INSPECTOR
    [Header("Watson Assistant")]
    [Tooltip("Text field to display the results of streaming.")]
    public TextMeshProUGUI ResultsField;
    [Tooltip("The service URL (optional). This defaults to \"https://gateway.watsonplatform.net/assistant/api\"")]
    [SerializeField]
    private string serviceUrl;
    [SerializeField]
    private string assistantId;
    [Header("IAM Authentication")]
    [Tooltip("The IAM apikey.")]
    [SerializeField]
    private string iamApikey;

    #endregion
    private AssistantService service;
    private bool sessionCreated = false;
    private string sessionId;
    private bool firstMessage;
    public static Assistant Instance;

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

        service = new AssistantService("2019-02-08", authenticator);

        service.CreateSession(OnCreateSession, assistantId);

        while (!sessionCreated)
            yield return null;
        
        
        // Send first message, create inputObj w/ no context
        //Message0();
    }
    private void OnCreateSession(DetailedResponse<SessionResponse> response, IBMError error)
    {
        Log.Debug("AvatarPatternError.OnCreateSession()", "Session: {0}", response.Result.SessionId);
        sessionId = response.Result.SessionId;
        sessionCreated = true;
    }

    // Initiate a conversation
    private void Message0()
    {
        firstMessage = true;
        var input = new MessageInput()
        {
            Text = "Hello"
        };

        service.Message(OnMessage, assistantId, sessionId, input);
    }

    private void OnMessage(DetailedResponse<MessageResponse> response, IBMError error)
    {
        if (!firstMessage)
        {
            //getIntent
            string intent = response.Result.Output.Intents[0].Intent;

            Debug.Log("Intent:" + intent);

            //Trigger the animation
            //MakeAMove(intent);

            //get Watson Output
            string outputText = response.Result.Output.Generic[0].Text;

            TextToSpeech.Instance.Run(outputText);

            //Debug.Log("Bot.Output text:" + outputText);

            ResultsField.text = outputText;
            ResultsField.color = new Color(0, 1, 0);
        }

        firstMessage = false;
    }

    /*
    private void MakeAMove(string intent)
    {
        if (intent.ToLower() == "forward")
        {
            //animator.SetBool("isIdle", false);
            //animator.SetBool("isWalkingBackward", false);
            //animator.SetBool("isWalkingForward", true);
        }
        else if (intent.ToLower() == "backward")
        {
            //animator.SetBool("isIdle", false);
            //animator.SetBool("isWalkingForward", false);
            //animator.SetBool("isWalkingBackward", true);
        }
        else if (intent.ToLower() == "idle")
        {
            //animator.SetBool("isIdle", true);
            //animator.SetBool("isWalkingBackward", false);
            //animator.SetBool("isWalkingForward", false);
        }
        else
        {
            //animator.SetBool("isIdle", true);
            //animator.SetBool("isWalkingBackward", false);
            //animator.SetBool("isWalkingForward", false);
        }
    }
    */

    public void BuildSpokenRequest(string spokenText)
    {
        var input = new MessageInput()
        {
            Text = spokenText
        };

        service.Message(OnMessage, assistantId, sessionId, input);
    }
}
