using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BrushControlPanelScript : MonoBehaviour
{
    public Toggle FlattenToggle;

    public SliderControlsScript RadiusSliderControlsScript;
    public SliderControlsScript StrengthSliderControlsScript;
    public SliderControlsScript NoiseSliderControlsScript;

    public EditorBrushType BrushType;

    public List<Toggle> BrushToggles = new List<Toggle>();

    public ToggleEvent BrushUntoggleEvent; // This event will fire when the brush is activated / deactivated regardless of type

    private const float _minRadiusValue = 1;
    private const float _maxRadiusValue = 20;
    private const float _defaultRadiusValue = 10;

    private const float _minStrengthValue = -1f;
    private const float _maxStrengthValue = 1;
    private const float _defaultStrengthValue = 0.25f;

    private const float _minNoiseValue = 0;
    private const float _maxNoiseValue = 1;
    private const float _defaultNoiseValue = 0.25f;

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

    public void ActivateFlattenMode(bool state)
    {
        NoiseSliderControlsScript.SetInteractable(!state);

        _flattenModeIsActive = state;
        Manager.EditorBrushIsFlattenModeIsActive = state;
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

    public void Activate(bool state)
    {
        gameObject.SetActive(state);

        if (state)
        {
            RadiusSliderControlsScript.MinValue = _minRadiusValue;
            RadiusSliderControlsScript.MaxValue = _maxRadiusValue;
            RadiusSliderControlsScript.DefaultValue = _defaultRadiusValue;

            RadiusSliderControlsScript.CurrentValue = _lastRadiusValue;
            RadiusSliderControlsScript.Initialize();

            StrengthSliderControlsScript.MinValue = _minStrengthValue;
            StrengthSliderControlsScript.MaxValue = _maxStrengthValue;
            StrengthSliderControlsScript.DefaultValue = _defaultStrengthValue;

            StrengthSliderControlsScript.CurrentValue = _lastStrengthValue;
            StrengthSliderControlsScript.Initialize();

            NoiseSliderControlsScript.MinValue = _minNoiseValue;
            NoiseSliderControlsScript.MaxValue = _maxNoiseValue;
            NoiseSliderControlsScript.DefaultValue = _defaultNoiseValue;

            NoiseSliderControlsScript.CurrentValue = _lastNoiseValue;
            NoiseSliderControlsScript.Initialize();

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
        }
        else
        {
            _lastRadiusValue = RadiusSliderControlsScript.CurrentValue;
            _lastStrengthValue = StrengthSliderControlsScript.CurrentValue;
            _lastNoiseValue = NoiseSliderControlsScript.CurrentValue;

            // Verify if any of the other brush toggles has been activated
            bool brushStillActive = false;
            foreach (Toggle toggle in BrushToggles)
            {
                brushStillActive |= toggle.isOn;
            }

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
