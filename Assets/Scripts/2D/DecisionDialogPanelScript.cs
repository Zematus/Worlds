using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class DecisionDialogPanelScript : ModalPanelScript {

	public Text DecisionText;

	public Transform OptionsPanelTransform;

	public Button OptionButtonPrefab;

	public Text SpeedButtonText;

	public int ResumeSpeedLevelIndex;

	public UnityEvent OptionChosenEvent;

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
		string descriptionText = option.DescriptionText;

		if (index < _optionButtons.Count) {
			button = _optionButtons[index];

		} else {
			button = AddOptionButton ();
		}

		ButtonWithTooltipScript buttonScript = button.GetComponent<ButtonWithTooltipScript> ();
		buttonScript.ButtonText.text = text;
		buttonScript.TooltipText.text = descriptionText;
		buttonScript.TooltipPanel.gameObject.SetActive (false);
		
		button.onClick.RemoveAllListeners ();

		button.onClick.AddListener (() => {
			
			option.Execute ();
			OptionChosenEvent.Invoke ();
		});
	}

	private Button AddOptionButton () {
	
		Button newButton = Instantiate (OptionButtonPrefab) as Button;

		newButton.transform.SetParent (OptionsPanelTransform, false);

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

	public override void SetVisible (bool value) {
		
		base.SetVisible (value);

		if (value) {
			SetOptions ();
		} else {
			RemoveOptionButtons ();
		}
	}

	public void Set (Decision decision, int currentSpeedIndex) {

		ResumeSpeedLevelIndex = currentSpeedIndex;

		SpeedButtonText.text = Speed.Levels[currentSpeedIndex];

		DecisionText.text = decision.Description;

		_decision = decision;
	}

	public void SelectNextSpeedLevel () {

		ResumeSpeedLevelIndex++;

		if (ResumeSpeedLevelIndex == Speed.Levels.Length) {
		
			ResumeSpeedLevelIndex = -1;

			SpeedButtonText.text = Speed.Zero;

		} else {

			SpeedButtonText.text = Speed.Levels[ResumeSpeedLevelIndex];
		}
	}
}
