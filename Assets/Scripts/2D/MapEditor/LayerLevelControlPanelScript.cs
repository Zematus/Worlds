using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class LayerLevelControlPanelScript : RegenControlPanelScript
{
    public Text Name;

    public SliderControlsScript FrequencySliderControlsScript;
    public SliderControlsScript NoiseInfluenceSliderControlsScript;

    public ToggleEvent TriggerOverlayChangeEvent; // This event will fire when this panel is activated

    public override void ResetSliderControls()
    {
        string layerId = Manager.PlanetOverlaySubtype;

        if ((Manager.PlanetOverlay != PlanetOverlay.Layer) ||
            (layerId == Manager.NoOverlaySubtype))
            return;

        Layer layer = Layer.Layers[layerId];

        FrequencySliderControlsScript.MinValue = 0;
        FrequencySliderControlsScript.MaxValue = 1;
        FrequencySliderControlsScript.DefaultValue = layer.Frequency;

        FrequencySliderControlsScript.CurrentValue = layer.Frequency;
        FrequencySliderControlsScript.Reinitialize();

        NoiseInfluenceSliderControlsScript.MinValue = 0;
        NoiseInfluenceSliderControlsScript.MaxValue = 1;
        NoiseInfluenceSliderControlsScript.DefaultValue = layer.SecondaryNoiseInfluence;

        NoiseInfluenceSliderControlsScript.CurrentValue = layer.SecondaryNoiseInfluence;
        NoiseInfluenceSliderControlsScript.Reinitialize();
    }

    public override void AllowEventInvoke(bool state)
    {
        FrequencySliderControlsScript.AllowEventInvoke(state);
        NoiseInfluenceSliderControlsScript.AllowEventInvoke(state);
    }

    public void ActivateControls(bool state)
    {
        FrequencySliderControlsScript.SetInteractable(state);
        NoiseInfluenceSliderControlsScript.SetInteractable(state);
    }

    public override void Activate(bool state)
    {
        base.Activate(state);

        TriggerOverlayChangeEvent.Invoke(state);
    }
}
