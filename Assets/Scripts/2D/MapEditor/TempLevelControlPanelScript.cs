using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class TempLevelControlPanelScript : RegenControlPanelScript
{
    public SliderControlsScript SliderControlsScript;

    public override void ResetSliderControls()
    {
        SliderControlsScript.MinValue = -50 - World.AvgPossibleTemperature;
        SliderControlsScript.MaxValue = 50 + World.AvgPossibleTemperature;
        SliderControlsScript.DefaultValue = World.AvgPossibleTemperature;

        SliderControlsScript.CurrentValue = Manager.TemperatureOffset;
        SliderControlsScript.Reinitialize();
    }

    public override void AllowEventInvoke(bool state)
    {
        SliderControlsScript.AllowEventInvoke(state);
    }
}
