using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapScript : MonoBehaviour {

	public RawImage Image;
	public GameObject InfoPanel;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

	}

	public void SetVisible (bool value) {
	
		Image.enabled = value;
		InfoPanel.SetActive(value);
	}
	
	public bool IsVisible () {
		
		return Image.enabled;
	}
	
	public void RefreshTexture () {
		
		Texture2D texture = Manager.CurrentMapTexture;
		
		Image.texture = texture;
	}
}
