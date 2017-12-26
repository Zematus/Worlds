using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class DecisionDialogPanelScript : MonoBehaviour {

	public CanvasGroup ModalPanelCanvasGroup;

	public Text DecisionText;

	public Button OptionButtonPrefab;

	private List<Button> _optionButtons = new List<Button>();

	private Decision _decision;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void SetOptions () {
		
		_optionButtons.Add (OptionButtonPrefab);

		string dirPath = Manager.SavePath;
	
		Decision.Option[] options = _decision.GetOptions ();

		int i = 0;

		foreach (Decision.Option option in options) {

			SetOptionButton (option, i);

			i++;
		}
	}

	private void SetOptionButton (Decision.Option option, int index) {
	
		Button button;

		string text = option.Text;

		if (index < _optionButtons.Count) {
			button = _optionButtons[index];
			button.GetComponentInChildren<Text> ().text = text;

		} else {
			button = AddOptionButton (text);
		}
		
		button.onClick.RemoveAllListeners ();

		button.onClick.AddListener (option.Execute);

		button.onClick.AddListener (() => {
			
			option.Execute ();

		});
	}

	private Button AddOptionButton (string text) {
	
		Button newButton = Instantiate (OptionButtonPrefab) as Button;

		newButton.transform.SetParent (transform, false);
		newButton.GetComponentInChildren<Text> ().text = text;

		_optionButtons.Add (newButton);

		return newButton;
	}

	private void RemoveOptionButtons () {

		bool first = true;

		foreach (Button button in _optionButtons) {
		
			if (first) {
				first = false;
				continue;
			}

			GameObject.Destroy (button.gameObject);
		}

		_optionButtons.Clear ();
	}

	public void SetVisible (bool value) {
		
		ModalPanelCanvasGroup.gameObject.SetActive (value);
		ModalPanelCanvasGroup.blocksRaycasts = value;
	
		gameObject.SetActive (value);

		if (value) {
			SetOptions ();
		} else {
			RemoveOptionButtons ();
		}
	}

	public void SetDecision (Decision decision) {

		DecisionText.text = decision.Description;

		_decision = decision;
	}
}
