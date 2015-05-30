using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LoadFileDialogPanelScript : MonoBehaviour {

	public CanvasGroup ModalPanelCanvasGroup;

	public Text DialogText;

	public Button WorldNameButtonPrefab;

	public Button ActionButton;
	public Button CancelActionButton;

	public Canvas ActionButtonCanvas;

	private List<Button> WorldNameButtons = new List<Button>();

	// Use this for initialization
	void Start () {
	
		WorldNameButtons.Add (WorldNameButtonPrefab);

		AddWorldNameButton ("test");
		AddWorldNameButton ("test 2");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void AddWorldNameButton (string name) {
	
		Button newButton = Instantiate (WorldNameButtonPrefab) as Button;

		newButton.transform.SetParent (transform, false);
		newButton.GetComponentInChildren<Text> ().text = name;

		WorldNameButtons.Add (newButton);

		ActionButtonCanvas.transform.SetAsLastSibling ();
	}

	public void SetDialogText (string text) {

		DialogText.text = text;
	}

	public void SetVisible (bool value) {
		
		ModalPanelCanvasGroup.gameObject.SetActive (value);
		ModalPanelCanvasGroup.blocksRaycasts = value;
	
		gameObject.SetActive (value);
	}

	public void SetAction (UnityAction action) {

		ActionButton.onClick.RemoveAllListeners ();
		ActionButton.onClick.AddListener (action);
	}
	
	public void SetCancelAction (UnityAction cancelAction) {
		
		CancelActionButton.onClick.RemoveAllListeners ();
		CancelActionButton.onClick.AddListener (cancelAction);
	}
}
