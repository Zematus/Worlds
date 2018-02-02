using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FocusPanelScript : MonoBehaviour {

	public Text FocusText;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetVisible (bool value) {

		gameObject.SetActive (value);
	}
}
