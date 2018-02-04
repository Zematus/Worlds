using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SelectFactionDialogPanelScript : MonoBehaviour {

	public CanvasGroup ModalPanelCanvasGroup;

	public Button FactionButtonPrefab;

	public Button CancelActionButton;

	public Transform ActionButtonPanelTransform;

	private List<Button> _factionButtons = new List<Button>();

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void SetFactionButtons () {

		Territory selectedTerritory = Manager.CurrentWorld.SelectedTerritory;

		Polity polity = null;

		if ((Manager.CurrentWorld.FocusedPolity != null) && 
			(Manager.CurrentWorld.FocusedPolity.StillPresent)) {

			polity = Manager.CurrentWorld.FocusedPolity;
		}

		if ((polity == null) && 
			(Manager.CurrentWorld.SelectedTerritory != null) && 
			(Manager.CurrentWorld.SelectedTerritory.Polity.StillPresent))

			polity = selectedTerritory.Polity;

		if (polity == null) {

			throw new System.Exception ("SetFactionButtons: Both focused polity and selected polity are null...");
		}

		///////

		_factionButtons.Add (FactionButtonPrefab);

		int i = 0;

		foreach (Faction faction in polity.GetFactions ()) {

			SetFactionButton (faction, i);

			i++;
		}
	}

	private void SetFactionButton (Faction faction, int index) {
	
		Button button;

		if (index < _factionButtons.Count) {
			button = _factionButtons[index];
			button.GetComponentInChildren<Text> ().text = faction.Name.Text;

		} else {
			button = AddFactionButton (faction);
		}
		
		button.onClick.RemoveAllListeners ();

		button.onClick.AddListener (() => {

			faction.SetControlled (true);
		});
	}

	private Button AddFactionButton (Faction faction) {
	
		Button newButton = Instantiate (FactionButtonPrefab) as Button;

		newButton.transform.SetParent (transform, false);
		newButton.GetComponentInChildren<Text> ().text = faction.Name.Text;

		_factionButtons.Add (newButton);

		ActionButtonPanelTransform.SetAsLastSibling ();

		return newButton;
	}

	private void RemoveFactionButtons () {

		bool first = true;

		foreach (Button button in _factionButtons) {
		
			if (first) {
				first = false;
				continue;
			}

			GameObject.Destroy(button.gameObject);
		}

		_factionButtons.Clear ();
	}

	public void SetVisible (bool value) {
		
		ModalPanelCanvasGroup.gameObject.SetActive (value);
		ModalPanelCanvasGroup.blocksRaycasts = value;
	
		gameObject.SetActive (value);

		if (value) {
			SetFactionButtons ();
		} else {
			RemoveFactionButtons ();
		}
	}
	
	public void SetCancelAction (UnityAction cancelAction) {
		
		CancelActionButton.onClick.RemoveAllListeners ();
		CancelActionButton.onClick.AddListener (cancelAction);
	}
}
