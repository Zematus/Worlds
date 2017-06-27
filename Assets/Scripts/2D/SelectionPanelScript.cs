using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class SelectionPanelScript : MonoBehaviour {

	public Text Title;

	public Toggle PrototypeToggle;

	public string SelectedOption = "None";

	public Dictionary<string, Toggle> Toggles = new Dictionary<string, Toggle>();

	// Use this for initialization
	void Start () {
	
		PrototypeToggle.gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void SetVisible (bool value) {
		
		gameObject.SetActive (value);
	}
	
	public bool IsVisible () {
		
		return gameObject.activeInHierarchy;
	}

	public void AddOption (string id, string text, UnityAction<bool> call) {

		Toggle toggle = null;

		if (Toggles.TryGetValue (id, out toggle)) {

			return;
		}

		toggle = GameObject.Instantiate (PrototypeToggle) as Toggle;

//		foreach (Toggle existingToggle in Toggles) {
//			
//			SelectionToggleScript existingToggleScript = existingToggle.gameObject.GetComponent<SelectionToggleScript> ();
//
//			if (existingToggleScript.Label.text == text) return;
//		}
//
//		Toggle toggle = GameObject.Instantiate (PrototypeToggle) as Toggle;

		toggle.onValueChanged.AddListener (call);
		toggle.transform.SetParent (gameObject.transform);

		SelectionToggleScript toggleScript = toggle.gameObject.GetComponent<SelectionToggleScript> ();
		toggleScript.Label.text = text;
		
		toggle.gameObject.SetActive (true);

		Toggles.Add (id, toggle);
	}

	public void SetStateOption (string id, bool state) {

		Toggle toggle = null;

		if (Toggles.TryGetValue (id, out toggle)) {

			toggle.isOn = state;
		}
	}

//	public void SetStateOption (string text, bool state) {
//
//		foreach (Toggle toggle in Toggles) {
//		
//			SelectionToggleScript toggleScript = toggle.gameObject.GetComponent<SelectionToggleScript> ();
//
//			if (toggleScript.Label.text == text) {
//
//				toggle.isOn = state;
//			}
//		}
//	}

	public void RemoveAllOptions () {
		
		foreach (Toggle toggle in Toggles.Values) {
			
			toggle.gameObject.SetActive (false);
			toggle.transform.SetParent (null);
			
			GameObject.Destroy (toggle);
		}

		Toggles.Clear ();
	}
}
