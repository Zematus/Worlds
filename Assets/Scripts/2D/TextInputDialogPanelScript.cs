using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class TextInputDialogPanelScript : DialogPanelScript {

	public InputField NameInputField;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void SetName (string name) {
		
		NameInputField.text = name;
	}
	
	public string GetName () {
		
		return NameInputField.text;
	}
}
