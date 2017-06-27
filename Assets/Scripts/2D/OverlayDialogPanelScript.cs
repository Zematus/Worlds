using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class OverlayDialogPanelScript : MonoBehaviour {

	public CanvasGroup ModalPanelCanvasGroup;

	public Text DialogText;

	public Toggle GeneralDataToggle;
	public Toggle PopDataToggle;
	public Toggle PolityDataToggle;
	public Toggle MiscDataToggle;
	public Toggle DebugDataToggle;

	public Toggle PopDensityToggle;
	public Toggle FarmlandToggle;
	public Toggle PopCulturalActivityToggle;
	public Toggle PopCulturalSkillToggle;
	public Toggle PopCulturalKnowledgeToggle;
	public Toggle PopCulturalDiscoveryToggle;

	public Toggle TerritoriesToggle;
	public Toggle DistancesToCoresToggle;
	public Toggle InfluenceToggle;
	public Toggle PolityCulturalActivityToggle;
	public Toggle PolityCulturalSkillToggle;
	public Toggle PolityCulturalKnowledgeToggle;
	public Toggle PolityCulturalDiscoveryToggle;

	public Toggle TemperatureToggle;
	public Toggle RainfallToggle;
	public Toggle ArabilityToggle;
	public Toggle RegionToggle;
	public Toggle LanguageToggle;

	public Toggle PopChangeToggle;
	public Toggle UpdateSpanToggle;

	public Toggle DisplayRoutesToggle;
	public Toggle DisplayGroupActivityToggle;

	public Button CloseActionButton;

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

	public void SetDialogText (string text) {

		DialogText.text = text;
	}

	public void SetVisible (bool value) {
		
		ModalPanelCanvasGroup.gameObject.SetActive (value);
		ModalPanelCanvasGroup.blocksRaycasts = value;
	
		gameObject.SetActive (value);
	}
	
	public void SetCloseAction (UnityAction closeAction) {
		
		CloseActionButton.onClick.RemoveAllListeners ();
		CloseActionButton.onClick.AddListener (closeAction);
	}

//	public void ResetToggles () {
//	
//		if (!PopDataToggle.isOn) {
//			PopDensityToggle.isOn = false;
//			FarmlandToggle.isOn = false;
//			PopCulturalActivityToggle.isOn = false;
//			PopCulturalSkillToggle.isOn = false;
//			PopCulturalKnowledgeToggle.isOn = false;
//			PopCulturalDiscoveryToggle.isOn = false;
//		}
//
//		if (!PolityDataToggle.isOn) {
//			TerritoriesToggle.isOn = false;
//			DistancesToCoresToggle.isOn = false;
//			InfluenceToggle.isOn = false;
//			PolityCulturalActivityToggle.isOn = false;
//			PolityCulturalSkillToggle.isOn = false;
//			PolityCulturalKnowledgeToggle.isOn = false;
//			PolityCulturalDiscoveryToggle.isOn = false;
//		}
//
//		if (!MiscDataToggle.isOn) {
//			TemperatureToggle.isOn = false;
//			RainfallToggle.isOn = false;
//			ArabilityToggle.isOn = false;
//			RegionToggle.isOn = false;
//			LanguageToggle.isOn = false;
//		}
//
//		if (!DebugDataToggle.isOn) {
//			PopChangeToggle.isOn = false;
//			UpdateSpanToggle.isOn = false;
//		}
//	}

	public void UpdateOptions () {

		GeneralDataToggle.isOn = false;		
		PopDataToggle.isOn = false;
		PolityDataToggle.isOn = false;
		MiscDataToggle.isOn = false;
		DebugDataToggle.isOn = false;

		PopDensityToggle.isOn = false;
		FarmlandToggle.isOn = false;
		PopCulturalActivityToggle.isOn = false;
		PopCulturalSkillToggle.isOn = false;
		PopCulturalKnowledgeToggle.isOn = false;
		PopCulturalDiscoveryToggle.isOn = false;

		TerritoriesToggle.isOn = false;
		DistancesToCoresToggle.isOn = false;
		InfluenceToggle.isOn = false;
		PolityCulturalActivityToggle.isOn = false;
		PolityCulturalSkillToggle.isOn = false;
		PolityCulturalKnowledgeToggle.isOn = false;
		PolityCulturalDiscoveryToggle.isOn = false;

		TemperatureToggle.isOn = false;
		RainfallToggle.isOn = false;
		ArabilityToggle.isOn = false;
		RegionToggle.isOn = false;
		LanguageToggle.isOn = false;

		PopChangeToggle.isOn = false;
		UpdateSpanToggle.isOn = false;

		DisplayRoutesToggle.isOn = false;
		DisplayGroupActivityToggle.isOn = false;

		switch (Manager.PlanetOverlay) {

		case PlanetOverlay.General:
			GeneralDataToggle.isOn = true;
			break;

		case PlanetOverlay.PopDensity:
			PopDensityToggle.isOn = true;
			PopDataToggle.isOn = true;
			break;

		case PlanetOverlay.FarmlandDistribution:
			FarmlandToggle.isOn = true;
			PopDataToggle.isOn = true;
			break;

		case PlanetOverlay.PopCulturalActivity:
			PopCulturalActivityToggle.isOn = true;
			PopDataToggle.isOn = true;
			break;

		case PlanetOverlay.PopCulturalSkill:
			PopCulturalSkillToggle.isOn = true;
			PopDataToggle.isOn = true;
			break;

		case PlanetOverlay.PopCulturalKnowledge:
			PopCulturalKnowledgeToggle.isOn = true;
			PopDataToggle.isOn = true;
			break;

		case PlanetOverlay.PopCulturalDiscovery:
			PopCulturalDiscoveryToggle.isOn = true;
			PopDataToggle.isOn = true;
			break;

		case PlanetOverlay.PolityTerritory:
			TerritoriesToggle.isOn = true;
			PolityDataToggle.isOn = true;
			break;

		case PlanetOverlay.PolityCoreDistance:
			DistancesToCoresToggle.isOn = true;
			PolityDataToggle.isOn = true;
			break;

		case PlanetOverlay.PolityInfluence:
			InfluenceToggle.isOn = true;
			PolityDataToggle.isOn = true;
			break;

		case PlanetOverlay.PolityCulturalActivity:
			PolityCulturalActivityToggle.isOn = true;
			PolityDataToggle.isOn = true;
			break;

		case PlanetOverlay.PolityCulturalSkill:
			PolityCulturalSkillToggle.isOn = true;
			PolityDataToggle.isOn = true;
			break;

		case PlanetOverlay.PolityCulturalKnowledge:
			PolityCulturalKnowledgeToggle.isOn = true;
			PolityDataToggle.isOn = true;
			break;

		case PlanetOverlay.PolityCulturalDiscovery:
			PolityCulturalDiscoveryToggle.isOn = true;
			PolityDataToggle.isOn = true;
			break;

		case PlanetOverlay.Temperature:
			TemperatureToggle.isOn = true;
			MiscDataToggle.isOn = true;
			break;

		case PlanetOverlay.Rainfall:
			RainfallToggle.isOn = true;
			MiscDataToggle.isOn = true;
			break;

		case PlanetOverlay.Arability:
			ArabilityToggle.isOn = true;
			MiscDataToggle.isOn = true;
			break;

		case PlanetOverlay.Region:
			RegionToggle.isOn = true;
			MiscDataToggle.isOn = true;
			break;

		case PlanetOverlay.Language:
			LanguageToggle.isOn = true;
			MiscDataToggle.isOn = true;
			break;

		case PlanetOverlay.PopChange:
			PopChangeToggle.isOn = true;
			DebugDataToggle.isOn = true;
			break;

		case PlanetOverlay.UpdateSpan:
			UpdateSpanToggle.isOn = true;
			DebugDataToggle.isOn = true;
			break;

		case PlanetOverlay.None:
			break;

		default:
			throw new System.Exception ("Unhandled Planet Overlay type: " + Manager.PlanetOverlay);
		}

		DisplayRoutesToggle.isOn = Manager.DisplayRoutes;
		DisplayGroupActivityToggle.isOn = Manager.DisplayGroupActivity;
	}
}
