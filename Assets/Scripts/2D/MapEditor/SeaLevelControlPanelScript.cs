using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class SeaLevelControlPanelScript : RegenControlPanelScript
{
    public SliderControlsScript SliderControlsScript;

    public override void ResetSliderControls()
    {
        SliderControlsScript.MinValue = -10000;
        SliderControlsScript.MaxValue = 10000;
        SliderControlsScript.DefaultValue = 0;

        SliderControlsScript.CurrentValue = Manager.SeaLevelOffset;
        SliderControlsScript.Reinitialize();
    }

    public override void AllowEventInvoke(bool state)
    {
        SliderControlsScript.AllowEventInvoke(state);
    }
}
