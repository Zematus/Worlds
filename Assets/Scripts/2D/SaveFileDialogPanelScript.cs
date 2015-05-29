﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class SaveFileDialogPanelScript : MonoBehaviour {

	public CanvasGroup ModalPanelCanvasGroup;

	public Text DialogText;
	public InputField NameInputField;

	public Button ActionButton;
	public Button CancelActionButton;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetDialogText (string text) {

		DialogText.text = text;
	}
	
	public void SetWorldName (string name) {
		
		NameInputField.text = name;
	}
	
	public string GetWorldName () {
		
		return NameInputField.text;
	}

	public void SetVisible (bool value) {
		
		ModalPanelCanvasGroup.gameObject.SetActive (value);
		ModalPanelCanvasGroup.blocksRaycasts = value;
	
		gameObject.SetActive (value);
	}

	public void SetAction (UnityAction action) {

		ActionButton.onClick.RemoveAllListeners ();
		ActionButton.onClick.AddListener (action);
	}
	
	public void SetCancelAction (UnityAction cancelAction) {
		
		CancelActionButton.onClick.RemoveAllListeners ();
		CancelActionButton.onClick.AddListener (cancelAction);
	}
}
