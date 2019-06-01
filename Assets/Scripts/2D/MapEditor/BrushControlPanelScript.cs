using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BrushControlPanelScript : MonoBehaviour
{
    public MapEditorToolbarScript ToolbarScript;

    public Text Name;

    public SliderControlsScript RadiusSliderControlsScript;
    public SliderControlsScript StrengthSliderControlsScript;
    public SliderControlsScript NoiseSliderControlsScript;

    public Toggle FlattenModeToggle;

    public EditorBrushType BrushType;

    public ToggleEvent BrushUntoggleEvent; // This event will fire when the brush is activated / deactivated regardless of type

    public ToggleEvent TriggerOverlayChangeEvent; // This event will fire when this panel is activated

    private const float _minRadiusValue = Manager.MinEditorBrushRadius;
    private const float _maxRadiusValue = Manager.MaxEditorBrushRadius;
    private const float _defaultRadiusValue = 10;

    private const float _minStrengthValue = -1f;
    private const float _minFlattenStrengthValue = 0f;
    private const float _maxStrengthValue = 1;
    private const float _defaultStrengthValue = 0.25f;

    private const float _minNoiseValue = 0;
    private const float _maxNoiseValue = 1;
    private const float _defaultNoiseValue = 0.25f;

    private bool _enableOverlayWhenActive = true;

    private float _lastRadiusValue = _defaultRadiusValue;
    private float _lastStrengthValue = _defaultStrengthValue;
    private float _lastNoiseValue = _defaultNoiseValue;

    private bool _flattenModeIsActive = false;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ActivateControls(bool state)
    {
        RadiusSliderControlsScript.SetInteractable(state);
        StrengthSliderControlsScript.SetInteractable(state);
        NoiseSliderControlsScript.SetInteractable(!_flattenModeIsActive && state);

        FlattenModeToggle.interactable = state;
    }

    public void ActivateFlattenMode(bool state)
    {
        NoiseSliderControlsScript.SetInteractable(!state);

        _flattenModeIsActive = state;
        Manager.EditorBrushIsFlattenModeIsActive = state;

        if (_flattenModeIsActive)
        {
            StrengthSliderControlsScript.MinValue = _minFlattenStrengthValue;
            StrengthSliderControlsScript.Slider.minValue = _minFlattenStrengthValue;

            if (StrengthSliderControlsScript.CurrentValue < _minFlattenStrengthValue)
                StrengthSliderControlsScript.SetValue(_minFlattenStrengthValue);
        }
        else
        {
            StrengthSliderControlsScript.MinValue = _minStrengthValue;
            StrengthSliderControlsScript.Slider.minValue = _minStrengthValue;
        }
    }

    public void SetRadiusValue(float value)
    {
        Manager.EditorBrushRadius = (int)value;
    }

    public void SetStrengthValue(float value)
    {
        Manager.EditorBrushStrength = value;
    }

    public void SetNoiseValue(float value)
    {
        Manager.EditorBrushNoise = value;
    }

    public void SetToEnableOverlay(bool state)
    {
        _enableOverlayWhenActive = state;

        TriggerOverlayChangeEvent.Invoke(state);
    }

    public void Activate(bool state)
    {
        gameObject.SetActive(state);

        if (state)
        {
            RadiusSliderControlsScript.MinValue = _minRadiusValue;
            RadiusSliderControlsScript.MaxValue = _maxRadiusValue;
            RadiusSliderControlsScript.DefaultValue = _defaultRadiusValue;

            RadiusSliderControlsScript.CurrentValue = _lastRadiusValue;
            RadiusSliderControlsScript.Reinitialize();

            if (_flattenModeIsActive)
                StrengthSliderControlsScript.MinValue = _minFlattenStrengthValue;
            else
                StrengthSliderControlsScript.MinValue = _minStrengthValue;
            StrengthSliderControlsScript.MaxValue = _maxStrengthValue;
            StrengthSliderControlsScript.DefaultValue = _defaultStrengthValue;

            StrengthSliderControlsScript.CurrentValue = _lastStrengthValue;
            StrengthSliderControlsScript.Reinitialize();

            NoiseSliderControlsScript.MinValue = _minNoiseValue;
            NoiseSliderControlsScript.MaxValue = _maxNoiseValue;
            NoiseSliderControlsScript.DefaultValue = _defaultNoiseValue;

            NoiseSliderControlsScript.CurrentValue = _lastNoiseValue;
            NoiseSliderControlsScript.Reinitialize();

            Manager.EditorBrushRadius = (int)_lastRadiusValue;
            Manager.EditorBrushStrength = _lastStrengthValue;
            Manager.EditorBrushNoise = _lastNoiseValue;
            Manager.EditorBrushType = BrushType;

            if (!Manager.EditorBrushIsVisible)
            {
                BrushUntoggleEvent.Invoke(false);
                Manager.SetSelectedCell(null);
            }

            Manager.EditorBrushIsVisible = true;
            Manager.EditorBrushIsFlattenModeIsActive = _flattenModeIsActive;

            TriggerOverlayChangeEvent.Invoke(_enableOverlayWhenActive);
        }
        else
        {
            _lastRadiusValue = RadiusSliderControlsScript.CurrentValue;
            _lastStrengthValue = StrengthSliderControlsScript.CurrentValue;
            _lastNoiseValue = NoiseSliderControlsScript.CurrentValue;

            // Verify if any of the other brush toggles has been activated
            bool brushStillActive = ToolbarScript.IsBrushToggleActive();

            Manager.EditorBrushIsVisible = brushStillActive;

            if (!brushStillActive)
            {
                Manager.EditorBrushType = EditorBrushType.None;

                BrushUntoggleEvent.Invoke(true);
            }
        }

        RadiusSliderControlsScript.AllowEventInvoke(state);
        StrengthSliderControlsScript.AllowEventInvoke(state);
        NoiseSliderControlsScript.AllowEventInvoke(state);
    }
}
