using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class ProgressDialogPanelScript : MonoBehaviour {

	public CanvasGroup ModalPanelCanvasGroup;

	public Text DialogText;

	public Image Image;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetDialogText (string text) {

		DialogText.text = text;
	}

	public void SetVisible (bool value) {
		
		ModalPanelCanvasGroup.gameObject.SetActive (value);
		ModalPanelCanvasGroup.blocksRaycasts = value;
	
		gameObject.SetActive (value);
	}

	public void SetProgress (float value) {

		Image.fillAmount = value;
	}
}
