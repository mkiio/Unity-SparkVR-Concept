using UnityEngine;
using System.Collections;

//simple script to change the colour of a hovered object.
public class HandHoverColour : MonoBehaviour {

    public Color hitColor;

    public Renderer _renderer;
    private Color startColor;

	void Start ()
    {
        if(_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
        }

        if (_renderer == null) return;

        startColor = _renderer.material.color;
	}

    public void OnHandHoverBegin ()
    {
        if (_renderer == null) return;

        _renderer.material.color = hitColor;
    }

    public void OnHandHoverEnd()
    {
        if (_renderer == null) return;

        _renderer.material.color = startColor;
    }
}
