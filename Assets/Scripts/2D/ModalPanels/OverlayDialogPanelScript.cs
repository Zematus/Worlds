using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class OverlayDialogPanelScript : MenuPanelScript
{
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
    public Toggle PolityClustersToggle;

    public Toggle TemperatureToggle;
    public Toggle RainfallToggle;
    public Toggle RiverBasinsToggle;
    public Toggle ArabilityToggle;
    public Toggle AccessibilityToggle;
    public Toggle HillinessToggle;
    public Toggle WoodCoverageToggle;
    public Toggle LayerToggle;
    public Toggle RegionToggle;
    public Toggle LanguageToggle;

    public Toggle PopChangeToggle;
    public Toggle UpdateSpanToggle;

    public Toggle DisplayRoutesToggle;
    public Toggle DisplayGroupActivityToggle;

    public GameObject Separator;

    public Button CloseActionButton;

    public bool DontUpdateDialog = false;

    // Use this for initialization
    void Start()
    {
        UpdateDebugOverlays();
    }

    public void UpdateDebugOverlays()
    {
        DebugDataToggle.gameObject.SetActive(Manager.DebugModeEnabled);
        DistancesToCoresToggle.gameObject.SetActive(Manager.DebugModeEnabled);
        PolityClustersToggle.gameObject.SetActive(Manager.DebugModeEnabled);
    }

    public void SetLayerOverlay(bool state)
    {
        LayerToggle.gameObject.SetActive(state);
    }

    public void SetCloseAction(UnityAction closeAction)
    {
        CloseActionButton.onClick.RemoveAllListeners();
        CloseActionButton.onClick.AddListener(closeAction);
    }

    public void ResetToggles()
    {
        if (!PopDataToggle.isOn)
        {
            PopDensityToggle.isOn = false;
            FarmlandToggle.isOn = false;
            PopCulturalPreferenceToggle.isOn = false;
            PopCulturalActivityToggle.isOn = false;
            PopCulturalSkillToggle.isOn = false;
            PopCulturalKnowledgeToggle.isOn = false;
            PopCulturalDiscoveryToggle.isOn = false;
        }

        if (!PolityDataToggle.isOn)
        {
            TerritoriesToggle.isOn = false;
            DistancesToCoresToggle.isOn = false;
            ProminenceToggle.isOn = false;
            ContactsToggle.isOn = false;
            PolityCulturalPreferenceToggle.isOn = false;
            PolityCulturalActivityToggle.isOn = false;
            PolityCulturalSkillToggle.isOn = false;
            PolityCulturalKnowledgeToggle.isOn = false;
            PolityCulturalDiscoveryToggle.isOn = false;
            PolityClustersToggle.isOn = false;
        }

        if (!MiscDataToggle.isOn)
        {
            TemperatureToggle.isOn = false;
            RainfallToggle.isOn = false;
            RiverBasinsToggle.isOn = false;
            ArabilityToggle.isOn = false;
            AccessibilityToggle.isOn = false;
            HillinessToggle.isOn = false;
            WoodCoverageToggle.isOn = false;
            RegionToggle.isOn = false;
            LanguageToggle.isOn = false;
        }

        if (!DebugDataToggle.isOn)
        {
            PopChangeToggle.isOn = false;
            UpdateSpanToggle.isOn = false;
        }
    }

    public void SetVisibleSimulationOverlays(bool state)
    {
        GeneralDataToggle.gameObject.SetActive(state);
        PopDataToggle.gameObject.SetActive(state);
        PolityDataToggle.gameObject.SetActive(state);
        RegionToggle.gameObject.SetActive(state);
        LanguageToggle.gameObject.SetActive(state);

        Separator.SetActive(state);

        DisplayRoutesToggle.gameObject.SetActive(state);
        DisplayGroupActivityToggle.gameObject.SetActive(state);
    }

    public void UpdateOptions()
    {
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
            (Manager.PlanetOverlay == PlanetOverlay.PolityCulturalDiscovery) ||
            (Manager.PlanetOverlay == PlanetOverlay.PolityCluster)
        );
        MiscDataToggle.isOn = (
            (Manager.PlanetOverlay == PlanetOverlay.Temperature) ||
            (Manager.PlanetOverlay == PlanetOverlay.Rainfall) ||
            (Manager.PlanetOverlay == PlanetOverlay.RiverBasins) ||
            (Manager.PlanetOverlay == PlanetOverlay.Arability) ||
            (Manager.PlanetOverlay == PlanetOverlay.Accessibility) ||
            (Manager.PlanetOverlay == PlanetOverlay.Hilliness) ||
            (Manager.PlanetOverlay == PlanetOverlay.WoodCoverage) ||
            (Manager.PlanetOverlay == PlanetOverlay.Layer) ||
            (Manager.PlanetOverlay == PlanetOverlay.Region) ||
            (Manager.PlanetOverlay == PlanetOverlay.Language)
        );
        DebugDataToggle.isOn = (
            (Manager.PlanetOverlay == PlanetOverlay.PopChange) ||
            (Manager.PlanetOverlay == PlanetOverlay.UpdateSpan)
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
        PolityClustersToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PolityCluster);

        TemperatureToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.Temperature);
        RainfallToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.Rainfall);
        RiverBasinsToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.RiverBasins);
        ArabilityToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.Arability);
        AccessibilityToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.Accessibility);
        HillinessToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.Hilliness);
        WoodCoverageToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.WoodCoverage);
        LayerToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.Layer);
        RegionToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.Region);
        LanguageToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.Language);

        PopChangeToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.PopChange);
        UpdateSpanToggle.isOn = (Manager.PlanetOverlay == PlanetOverlay.UpdateSpan);

        DisplayRoutesToggle.isOn = Manager.DisplayRoutes;
        DisplayGroupActivityToggle.isOn = Manager.DisplayGroupActivity;

        DontUpdateDialog = false;
    }
}
