using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class OverlayDialogPanelScript : DialogPanelScript {

	public Toggle GeneralDataToggle;
	public Toggle PopDataToggle;
	public Toggle PolityDataToggle;
	public Toggle MiscDataToggle;
	public Toggle DebugDataToggle;

	public Toggle PopDensityToggle;
	public Toggle FarmlandToggle;
	public Toggle PopCulturalPreferenceToggle;
	public Toggle PopCulturalActivityToggle;
	public Toggle PopCulturalSkillToggle;
	public Toggle PopCulturalKnowledgeToggle;
	public Toggle PopCulturalDiscoveryToggle;

	public Toggle TerritoriesToggle;
    public Toggle ProminenceToggle;
	public Toggle ContactsToggle;
	public Toggle PolityCulturalPreferenceToggle;
	public Toggle PolityCulturalActivityToggle;
	public Toggle PolityCulturalSkillToggle;
	public Toggle PolityCulturalKnowledgeToggle;
	public Toggle PolityCulturalDiscoveryToggle;
	public Toggle DistancesToCoresToggle;

	public Toggle TemperatureToggle;
	public Toggle RainfallToggle;
	public Toggle ArabilityToggle;
	public Toggle RegionToggle;
	public Toggle LanguageToggle;

	public Toggle PopChangeToggle;
	public Toggle UpdateSpanToggle;
    public Toggle PolityClustersToggle;

    public Toggle DisplayRoutesToggle;
	public Toggle DisplayGroupActivityToggle;

	public Button CloseActionButton;

	public bool DontUpdateDialog = false;

	// Use this for initialization
	void Start () {

		bool debugState = false;

		#if DEBUG
		debugState = true;
		#endif
	
		DebugDataToggle.gameObject.SetActive (debugState);
		DistancesToCoresToggle.gameObject.SetActive (debugState);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void SetCloseAction (UnityAction closeAction) {
		
		CloseActionButton.onClick.RemoveAllListeners ();
		CloseActionButton.onClick.AddListener (closeAction);
	}

	public void ResetToggles () {
	
		if (!PopDataToggle.isOn) {
			PopDensityToggle.isOn = false;
			FarmlandToggle.isOn = false;
			PopCulturalPreferenceToggle.isOn = false;
			PopCulturalActivityToggle.isOn = false;
			PopCulturalSkillToggle.isOn = false;
			PopCulturalKnowledgeToggle.isOn = false;
			PopCulturalDiscoveryToggle.isOn = false;
		}

		if (!PolityDataToggle.isOn) {
			TerritoriesToggle.isOn = false;
			DistancesToCoresToggle.isOn = false;
			ProminenceToggle.isOn = false;
			ContactsToggle.isOn = false;
			PolityCulturalPreferenceToggle.isOn = false;
			PolityCulturalActivityToggle.isOn = false;
			PolityCulturalSkillToggle.isOn = false;
			PolityCulturalKnowledgeToggle.isOn = false;
			PolityCulturalDiscoveryToggle.isOn = false;
		}

		if (!MiscDataToggle.isOn) {
			TemperatureToggle.isOn = false;
			RainfallToggle.isOn = false;
			ArabilityToggle.isOn = false;
			RegionToggle.isOn = false;
			LanguageToggle.isOn = false;
		}

		if (!DebugDataToggle.isOn) {
			PopChangeToggle.isOn = false;
			UpdateSpanToggle.isOn = false;
            PolityClustersToggle.isOn = false;
		}
	}

	public void UpdateOptions () {

		DontUpdateDialog = true;

		GeneralDataToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.General);		
		PopDataToggle.isOn = (
			(Manager.PlanetOverlay == PlanetOverlay.PopDensity) ||
			(Manager.PlanetOverlay == PlanetOverlay.FarmlandDistribution) ||
			(Manager.PlanetOverlay == PlanetOverlay.PopCulturalPreference) ||
			(Manager.PlanetOverlay == PlanetOverlay.PopCulturalActivity) ||
			(Manager.PlanetOverlay == PlanetOverlay.PopCulturalSkill) ||
			(Manager.PlanetOverlay == PlanetOverlay.PopCulturalKnowledge) ||
			(Manager.PlanetOverlay == PlanetOverlay.PopCulturalDiscovery)
		);
		PolityDataToggle.isOn = (
			(Manager.PlanetOverlay == PlanetOverlay.PolityTerritory) ||
			(Manager.PlanetOverlay == PlanetOverlay.FactionCoreDistance) ||
			(Manager.PlanetOverlay == PlanetOverlay.PolityProminence) ||
			(Manager.PlanetOverlay == PlanetOverlay.PolityContacts) ||
			(Manager.PlanetOverlay == PlanetOverlay.PolityCulturalPreference) ||
			(Manager.PlanetOverlay == PlanetOverlay.PolityCulturalActivity) ||
			(Manager.PlanetOverlay == PlanetOverlay.PolityCulturalSkill) ||
			(Manager.PlanetOverlay == PlanetOverlay.PolityCulturalKnowledge) ||
			(Manager.PlanetOverlay == PlanetOverlay.PolityCulturalDiscovery)
		);
		MiscDataToggle.isOn = (
			(Manager.PlanetOverlay == PlanetOverlay.Temperature) ||
			(Manager.PlanetOverlay == PlanetOverlay.Rainfall) ||
			(Manager.PlanetOverlay == PlanetOverlay.Arability) ||
			(Manager.PlanetOverlay == PlanetOverlay.Region) ||
			(Manager.PlanetOverlay == PlanetOverlay.Language)
		);
		DebugDataToggle.isOn = (
			(Manager.PlanetOverlay == PlanetOverlay.PopChange) ||
			(Manager.PlanetOverlay == PlanetOverlay.UpdateSpan) ||
            (Manager.PlanetOverlay == PlanetOverlay.PolityClusters)
        );

		PopDensityToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PopDensity);
		FarmlandToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.FarmlandDistribution);
		PopCulturalPreferenceToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PopCulturalPreference);
		PopCulturalActivityToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PopCulturalActivity);
		PopCulturalSkillToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PopCulturalSkill);
		PopCulturalKnowledgeToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PopCulturalKnowledge);
		PopCulturalDiscoveryToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PopCulturalDiscovery);

		TerritoriesToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PolityTerritory);
		ProminenceToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PolityProminence);
		ContactsToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PolityContacts);
		PolityCulturalPreferenceToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PolityCulturalPreference);
		PolityCulturalActivityToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PolityCulturalActivity);
		PolityCulturalSkillToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PolityCulturalSkill);
		PolityCulturalKnowledgeToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PolityCulturalKnowledge);
		PolityCulturalDiscoveryToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PolityCulturalDiscovery);
		DistancesToCoresToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.FactionCoreDistance);

		TemperatureToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.Temperature);
		RainfallToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.Rainfall);
		ArabilityToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.Arability);
		RegionToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.Region);
		LanguageToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.Language);

		PopChangeToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PopChange);
		UpdateSpanToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.UpdateSpan);
        PolityClustersToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PolityClusters);

        DisplayRoutesToggle.isOn = Manager.DisplayRoutes;
		DisplayGroupActivityToggle.isOn = Manager.DisplayGroupActivity;

		DontUpdateDialog = false;
	}
}
