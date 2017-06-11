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

	public void ResetToggles () {
	
		if (!PopDataToggle.isOn) {
			PopDensityToggle.isOn = false;
			FarmlandToggle.isOn = false;
			PopCulturalActivityToggle.isOn = false;
			PopCulturalSkillToggle.isOn = false;
			PopCulturalKnowledgeToggle.isOn = false;
			PopCulturalDiscoveryToggle.isOn = false;
		}

		if (!PolityDataToggle.isOn) {
			TerritoriesToggle.isOn = false;
			DistancesToCoresToggle.isOn = false;
			InfluenceToggle.isOn = false;
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
		}
	}
}
