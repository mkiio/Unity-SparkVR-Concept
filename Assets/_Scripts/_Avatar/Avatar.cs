using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Avatar : MonoBehaviour
{
    public string personId;
    public string email;
    public Text textName;
    public Text textMessageBox;
    public Renderer headRenderer;

    private void Update()
    {
        transform.LookAt(Camera.main.transform);
    }

    public void SetPersonId(string _personId)
    {
        personId = _personId;
    }

    public void SetEmail(string _email)
    {
        email = _email;
    }

    public void UpdateName(string _name)
    {
        gameObject.name = _name;
        textName.text = _name;
    }

    public void UpdateImage(Texture2D _personImage)
    {
        headRenderer.material.mainTexture = _personImage;
    }

    public void UpdateMessages(string _messages)
    {
        textMessageBox.text += "\n" + _messages;
    }
}
