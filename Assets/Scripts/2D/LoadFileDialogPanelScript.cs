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

	public Button CancelActionButton;

	public Transform ActionButtonPanelTransform;

	private List<Button> WorldNameButtons = new List<Button>();

	private UnityAction _loadAction;
	private string _pathToLoad;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public string GetPathToLoad () {

		return _pathToLoad;
	}

	private void LoadFileNames () {
		
		WorldNameButtons.Add (WorldNameButtonPrefab);

		string dirPath = Manager.SavePath;
	
		string[] files = Directory.GetFiles (dirPath, "*.PLNT");

		int i = 0;

		foreach (string file in files) {

			string name = Path.GetFileNameWithoutExtension(file);

			SetWorldNameButton (name, i);

			i++;
		}
	}

	private void SetWorldNameButton (string name, int index) {
	
		Button button;

		if (index < WorldNameButtons.Count) {
			button = WorldNameButtons[index];
			button.GetComponentInChildren<Text> ().text = name;

		} else {
			button = AddWorldNameButton (name);
		}
		
		button.onClick.RemoveAllListeners ();

		string path = Manager.SavePath + name + ".PLNT";

		button.onClick.AddListener (() => {

			_pathToLoad = path;
			_loadAction ();
		});
	}

	private Button AddWorldNameButton (string name) {
	
		Button newButton = Instantiate (WorldNameButtonPrefab) as Button;

		newButton.transform.SetParent (transform, false);
		newButton.GetComponentInChildren<Text> ().text = name;

		WorldNameButtons.Add (newButton);

		ActionButtonPanelTransform.SetAsLastSibling ();

		return newButton;
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

	public void SetLoadAction (UnityAction action) {

		_loadAction = action;
	}
	
	public void SetCancelAction (UnityAction cancelAction) {
		
		CancelActionButton.onClick.RemoveAllListeners ();
		CancelActionButton.onClick.AddListener (cancelAction);
	}
}
