using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class MapScript : MonoBehaviour {

	public RawImage Image;
	public GameObject InfoPanel;

	public UnityEvent MouseOverEvent;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
		if (EventSystem.current.IsPointerOverGameObject()) {
			
			MouseOverEvent.Invoke ();
		}
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
