using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FocusPanelState {
	SetFocusOrControl,
	UnfocusOrControl,
	StopControl
}

public class FocusPanelScript : MonoBehaviour {

	public Text FocusText;

	public Button FocusButton;
	public Button UnfocusButton;
	public Button ControlButton;
	public Button StopControlButton;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetVisible (bool value) {

		gameObject.SetActive (value);
	}

	public void SetState (FocusPanelState state, string name) {
	
		switch (state) {

		case FocusPanelState.SetFocusOrControl:
			FocusText.text = name;
			FocusButton.gameObject.SetActive (true);
			UnfocusButton.gameObject.SetActive (false);
			ControlButton.gameObject.SetActive (true);
			StopControlButton.gameObject.SetActive (false);
			break;

		case FocusPanelState.UnfocusOrControl:
			FocusText.text = "Focused on " + name;
			FocusButton.gameObject.SetActive (false);
			UnfocusButton.gameObject.SetActive (true);
			ControlButton.gameObject.SetActive (true);
			StopControlButton.gameObject.SetActive (false);
			break;

		case FocusPanelState.StopControl:
			FocusText.text = "Controlling " + name;
			FocusButton.gameObject.SetActive (false);
			UnfocusButton.gameObject.SetActive (false);
			ControlButton.gameObject.SetActive (false);
			StopControlButton.gameObject.SetActive (true);
			break;
		}
	}
}
