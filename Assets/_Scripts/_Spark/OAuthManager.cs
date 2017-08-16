using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityEngine.Networking;


/// <summary>
/// Handles Authentication
/// It currently relies on a local HTTPListener, meaning the box its running on has to be accessible from the internet
/// I'm using spark.mki.io to point to my home IP address.. 
/// </summary>


public class AccessToken
{
    public string access_token;
    public int expires_in;
    public string refresh_token;
    public int refresh_token_expires_in;
}

public class OAuthManager : MonoBehaviour {

    private string clientID = "";
    private string clientSecret = "";
    private string redirectUri = "http://spark.mki.io:8080";
  
    private string stateCheck = "authmkiio"; //Not actively using this right now -- but wanted to have it anyway. 

    private void Start()
    {
        if (!SparkManager.instance.IsAuthenticated())
        {
            StartAuthentication();
        }
    }

    //this kicks off the whole cycle -- it calls a browser window, then calls the HTTP Listener cycle. 
    public void StartAuthentication()
    {
        //Request URL -- grab the settings and authenticate. 
        string requestUrl = "https://api.ciscospark.com/v1/authorize?response_type=code&" + 
         "scope=" + WWW.EscapeURL("spark:all") + "&" + 
         "state=" + stateCheck + "&" +   
         "client_id=" + WWW.EscapeURL(clientID) + "&" +
         "redirect_uri=" +  WWW.EscapeURL(redirectUri); 

        Application.OpenURL(requestUrl);
        StartCoroutine(StartListener());
    }

    //most of this is straight out of MSDN C# Docs, opens a listener for the return call from the Spark OAuth integration
    private IEnumerator StartListener() 
    {
        //This isn't really necessary -- I was just giving the app a second before the listern blocks execution 
        yield return new WaitForSeconds(0.5f); 

        HttpListener listener = new HttpListener();

        listener.Prefixes.Add(redirectUri + "/");
        listener.Prefixes.Add("http://localhost:8080/"); //Dev back-up
        listener.Start();

        Debug.Log("Listening...");

        // Note: The GetContext method blocks while waiting for a request. 
        HttpListenerContext context = listener.GetContext();
        HttpListenerRequest request = context.Request;

        // Obtain a response object.
        HttpListenerResponse response = context.Response;

        // Construct a response.
        string responseString = "<html><title>Spark VR</title><body>Spark VR Authenticated! You may now close this window.</body></html>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        // Get a response stream and write the response to it.
        response.ContentLength64 = buffer.Length;
        System.IO.Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
      
        output.Close();
        listener.Stop();

        Debug.Log(request.RawUrl);
        RequestToken(request.RawUrl);
    }

    //Once the above cycle is done we should have our authcode -- no error handling here yet.. 
    //This just cleans the URL authcode returned back to the app, then it posts back to Spark API for the token.
    private void RequestToken(string authCode)
    {
        authCode = authCode.Substring(7);
        authCode = authCode.Substring(0, authCode.IndexOf("&"));
        StartCoroutine(PostAuthorization(authCode, ParseToken));
    }

    //Post request -- call ParseToken on success
    private IEnumerator PostAuthorization(string authCode, System.Action<string> sendTo)
    {
        WWWForm newForm = new WWWForm();
        newForm.AddField("grant_type", "authorization_code");
        newForm.AddField("client_id", clientID);
        newForm.AddField("client_secret", clientSecret);
        newForm.AddField("code", authCode);
        newForm.AddField("redirect_uri", redirectUri);

        Debug.Log("Redirect URI");

        UnityWebRequest www = UnityWebRequest.Post("https://api.ciscospark.com/v1/access_token", newForm);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return www.Send();

        Debug.Log("Status Code: " + www.responseCode);

        if (www.isError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if(sendTo != null)
            {
                sendTo.Invoke(www.downloadHandler.text);
            }
        }
    }

    //Parses the token and passes it over to SparkManager
    //Not currently keeping any of the refresh or expirey information for this prototype. 
    private void ParseToken(string json)
    {
        Debug.Log("Parsing access token...");
        AccessToken accessToken = JsonUtility.FromJson<AccessToken>(json);
        SparkManager.instance.UpdateToken(accessToken.access_token);
    }
}
