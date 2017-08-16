using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkUser : MonoBehaviour {

    public Material headMaterial;

    private Person me;


    public void Start()
    {
        if (SparkManager.instance.IsAuthenticated())
        {
            SparkManager.instance.GetMe(ParseMe);
        }
    }

    public void ParseMe(string json)
    {
        me = JsonUtility.FromJson<Person>(json);

        if (me == null)
        {
            Debug.LogError("SparkUser did not recieve any data about current local user. Local user not setup.");
            return;
        }

        UpdateMe();
    }

    private void UpdateMe()
    {
        if (me == null) return;

        StartCoroutine(UpdateImage());
    }

    public IEnumerator UpdateImage()
    {
        Debug.Log("Downloading avatar picture for local user");
        WWW www = new WWW(me.avatar); //downloading the avatar picture
        yield return www;

        Debug.Log("Download complete. Updating Avatar Image for local user ");
        headMaterial.mainTexture = www.texture;
    }
}
