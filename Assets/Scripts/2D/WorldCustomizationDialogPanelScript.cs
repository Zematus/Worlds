using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class WorldCustomizationDialogPanelScript : MonoBehaviour {

	public CanvasGroup ModalPanelCanvasGroup;

	public Text DialogText;
	public InputField SeedInputField;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetDialogText (string text) {

		DialogText.text = text;
	}
	
	public void SetSeedStr (string seedStr) {
		
		SeedInputField.text = seedStr;
	}
	
	public string GetSeedString () {
		
		return SeedInputField.text;
	}

	public void SetVisible (bool value) {
		
		ModalPanelCanvasGroup.gameObject.SetActive (value);
		ModalPanelCanvasGroup.blocksRaycasts = value;
	
		gameObject.SetActive (value);
	}
}
