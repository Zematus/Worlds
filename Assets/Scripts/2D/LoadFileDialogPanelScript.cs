using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void LoadFileNames () {
	
		string[] files = Directory.GetFiles(@"Saves\", "*.XML");

		int i = 0;

		foreach (string file in files) {

			string name = file.Split(new char[] {'\\', '.'})[1];

			SetWorldNameButton(name, i);

			i++;
		}
	}

	private void SetWorldNameButton (string name, int index) {
	
		if (index < WorldNameButtons.Count) {
			WorldNameButtons[index].GetComponentInChildren<Text> ().text = name;
		} else {
			AddWorldNameButton (name);
		}
	}

	private void AddWorldNameButton (string name) {
	
		Button newButton = Instantiate (WorldNameButtonPrefab) as Button;

		newButton.transform.SetParent (transform, false);
		newButton.GetComponentInChildren<Text> ().text = name;

		WorldNameButtons.Add (newButton);

		ActionButtonCanvas.transform.SetAsLastSibling ();
	}

	private void RemoveWorldNameButtons () {

		bool first = true;

		foreach (Button button in WorldNameButtons) {
		
			if (first) {
				first = false;
				continue;
			}

			GameObject.Destroy(button.gameObject);
		}

		WorldNameButtons.Clear ();

		WorldNameButtons.Add (WorldNameButtonPrefab);
	}

	public void SetDialogText (string text) {

		DialogText.text = text;
	}

	public void SetVisible (bool value) {
		
		ModalPanelCanvasGroup.gameObject.SetActive (value);
		ModalPanelCanvasGroup.blocksRaycasts = value;
	
		gameObject.SetActive (value);

		if (value) {
			LoadFileNames ();
		} else {
			RemoveWorldNameButtons ();
		}
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
