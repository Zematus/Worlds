using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelScript : MonoBehaviour {

	public Text InfoText;
	public Button FocusButton;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ShowFocusButton (bool state) {

		if (FocusButton.gameObject.activeSelf == state)
			return;

		FocusButton.gameObject.SetActive (state);
	}
}
