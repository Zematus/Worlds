using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SelfEnablingButtonScript : MonoBehaviour {

	public Button Button;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Disable (bool value) {

		Button.interactable = !value;
	}
}
