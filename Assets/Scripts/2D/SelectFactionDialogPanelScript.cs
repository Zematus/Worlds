using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SelectFactionDialogPanelScript : ModalPanelScript {

	public Button FactionButtonPrefab;

	public Button CancelActionButton;

	public Transform ActionButtonPanelTransform;

	public UnityEvent FactionButtonClickEvent;

	public Faction ChosenFaction = null;

	private List<Button> _factionButtons = new List<Button>();

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void SetFactionButtons () {

		ChosenFaction = null;

		Territory selectedTerritory = Manager.CurrentWorld.SelectedTerritory;

		Polity polity = null;

		if ((polity == null) && 
			(Manager.CurrentWorld.SelectedTerritory != null) && 
			(Manager.CurrentWorld.SelectedTerritory.Polity.StillPresent))

			polity = selectedTerritory.Polity;

		if (polity == null) {

			throw new System.Exception ("SetFactionButtons: Both focused polity and selected polity are null...");
		}

		///////

		_factionButtons.Add (FactionButtonPrefab);

		List<Faction> factions = new List<Faction> (polity.GetFactions ());

		factions.Sort ((a, b) => {
			if (a.Influence > b.Influence)
				return -1;
			if (a.Influence < b.Influence)
				return 1;

			return 0;
		});

		int i = 0;

		foreach (Faction faction in factions) {

			SetFactionButton (faction, i);

			i++;
		}
	}

	private string GenerateFactionButtonStr (Faction faction) {

		return faction.Name.Text + " " + "(Influence: " + faction.Influence.ToString("P") + ")";
	}

	private void SetFactionButton (Faction faction, int index) {
	
		Button button;

		if (index < _factionButtons.Count) {
			button = _factionButtons[index];
			button.GetComponentInChildren<Text> ().text = GenerateFactionButtonStr (faction);

		} else {
			button = AddFactionButton (faction);
		}
		
		button.onClick.RemoveAllListeners ();

		button.onClick.AddListener (() => {

			ChosenFaction = faction;
			FactionButtonClickEvent.Invoke ();
		});
	}

	private Button AddFactionButton (Faction faction) {
	
		Button newButton = Instantiate (FactionButtonPrefab) as Button;

		newButton.transform.SetParent (transform, false);
		newButton.GetComponentInChildren<Text> ().text = GenerateFactionButtonStr (faction);

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

	public override void SetVisible (bool value) {
		
		base.SetVisible (value);

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
