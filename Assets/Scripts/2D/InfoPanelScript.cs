using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelScript : MonoBehaviour {

	public Text InfoText;
	public GameObject ButtonPanel;
	public Text FocusButtonText;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ShowFocusButton (bool state) {

		if (ButtonPanel.activeSelf == state)
			return;

		ButtonPanel.SetActive (state);
	}
}
