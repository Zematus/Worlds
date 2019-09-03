using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class RiverLevelControlPanelScript : RegenControlPanelScript
{
    public SliderControlsScript SliderControlsScript;

    public override void ResetSliderControls()
    {
        SliderControlsScript.MinValue = 0;
        SliderControlsScript.MaxValue = 1;
        SliderControlsScript.DefaultValue = World.DefaultRiverStrength;

        SliderControlsScript.CurrentValue = Manager.RiverStrength;
        SliderControlsScript.Reinitialize();
    }

    public override void AllowEventInvoke(bool state)
    {
        SliderControlsScript.AllowEventInvoke(state);
    }
}
