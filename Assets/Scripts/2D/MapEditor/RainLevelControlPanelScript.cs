using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class RainLevelControlPanelScript : RegenControlPanelScript
{
    public SliderControlsScript SliderControlsScript;

    public override void ResetSliderControls()
    {
        SliderControlsScript.MinValue = -800f + World.AvgPossibleRainfall;
        SliderControlsScript.MaxValue = 800f + World.AvgPossibleRainfall;
        SliderControlsScript.DefaultValue = World.AvgPossibleRainfall;

        SliderControlsScript.CurrentValue = Manager.RainfallOffset;
        SliderControlsScript.Reinitialize();
    }

    public override void AllowEventInvoke(bool state)
    {
        SliderControlsScript.AllowEventInvoke(state);
    }
}
