using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public abstract class RegenControlPanelScript : MonoBehaviour
{
    public GuiManagerScript GuiManager;

    public SliderControlsScript SliderControlsScript;

    protected float _minValue;
    protected float _maxValue;
    protected float _defaultValue;

    protected abstract float GetCurrentValueFromManager();

    public void ResetSliderControl()
    {
        SliderControlsScript.MinValue = _minValue;
        SliderControlsScript.MaxValue = _maxValue;
        SliderControlsScript.DefaultValue = _defaultValue;

        SliderControlsScript.CurrentValue = GetCurrentValueFromManager();
        SliderControlsScript.Reinitialize();
    }

    public void Activate(bool state)
    {
        gameObject.SetActive(state);

        if (state)
        {
            ResetSliderControl();

            GuiManager.RegisterLoadWorldPostProgressOp(ResetSliderControl);
        }
        else
        {
            GuiManager.DeregisterLoadWorldPostProgressOp(ResetSliderControl);
        }

        SliderControlsScript.AllowEventInvoke(state);
    }
}
