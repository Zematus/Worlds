using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class FlattenControlPanelScript : RegenControlPanelScript
{
    public SliderControlsScript SliderControlsScript;

    public override void ResetSliderControls()
    {
        SliderControlsScript.MinValue = 0.05f;
        SliderControlsScript.MaxValue = 1;
        SliderControlsScript.DefaultValue = World.DefaultAltitudeScale;

        SliderControlsScript.CurrentValue = Manager.AltitudeScale;
        SliderControlsScript.Reinitialize();
    }

    public override void AllowEventInvoke(bool state)
    {
        SliderControlsScript.AllowEventInvoke(state);
    }
}
