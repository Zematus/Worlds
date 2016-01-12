using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class ToggleButtonScript : MonoBehaviour {

	public bool IsOn;

	public Toggle Toggle;

	public Image CheckImage;
	public Image UncheckImage;
	public Image PartialCheckImage;

	public ToggleEvent OnToggle;

	private bool _partialCheck = false;

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

		UncheckImage.enabled = !value && !_partialCheck;
		PartialCheckImage.enabled = !value && _partialCheck;
		CheckImage.enabled = value;

		OnToggle.Invoke (value);
	}

	public void SetPartiallyToggled (bool state) {

		_partialCheck = state;

		UncheckImage.enabled = !IsOn && !state;
		PartialCheckImage.enabled = !IsOn && state;

		Toggle.interactable = !state;
	}
}
