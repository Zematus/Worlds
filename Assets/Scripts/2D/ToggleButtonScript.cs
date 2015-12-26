using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;

[Serializable]
public class ToggleEvent : UnityEvent <bool> {}

public class ToggleButtonScript : MonoBehaviour {

	public bool IsOn;

	public Toggle Toggle;

	public Image CheckImage;
	public Image UncheckImage;

	public ToggleEvent OnToggle;

	// Use this for initialization
	void Start () {
	
		SetState (false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnValueChanged () {

		SetState (Toggle.isOn);
	}

	public void SetState (bool value) {

		Toggle.isOn = value;
		IsOn = value;

		UncheckImage.enabled = !value;
		CheckImage.enabled = value;

		OnToggle.Invoke (value);
	}
}
