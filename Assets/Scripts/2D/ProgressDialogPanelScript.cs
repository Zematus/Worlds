using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class ProgressDialogPanelScript : ImageDialogPanelScript {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetProgress (float value) {

		Image.fillAmount = value;
	}
}
