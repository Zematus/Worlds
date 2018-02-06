using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FocusedPolityPanelScript : MonoBehaviour {

	public Text PolityText;

	public Button UnsetFocusButton;

	public Polity Polity = null;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	public void SetVisible (bool value) {

		gameObject.SetActive (value);
	}

	public void Set (Polity polity) {

		Polity = polity;

		PolityText.text = polity.Name.Text + " " + polity.Type;

		UnsetFocusButton.onClick.RemoveAllListeners ();

		UnsetFocusButton.onClick.AddListener (() => {

			Manager.UnsetFocusOnPolity (polity);
		});
	}
}
