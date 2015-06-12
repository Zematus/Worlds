using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class OverlayDialogPanelScript : MonoBehaviour {

	public CanvasGroup ModalPanelCanvasGroup;

	public Text DialogText;

	public Toggle RainfallToggle;
	public Toggle TemperatureToggle;

	public Button CloseActionButton;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetDialogText (string text) {

		DialogText.text = text;
	}

	public void SetVisible (bool value) {
		
		ModalPanelCanvasGroup.gameObject.SetActive (value);
		ModalPanelCanvasGroup.blocksRaycasts = value;
	
		gameObject.SetActive (value);
	}
	
	public void SetCloseAction (UnityAction closeAction) {
		
		CloseActionButton.onClick.RemoveAllListeners ();
		CloseActionButton.onClick.AddListener (closeAction);
	}
}
