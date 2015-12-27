using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpeedPanelScript : MonoBehaviour {

	public Text Message;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetSpeedMessage (string message) {

		Message.text = message;
	}
}
