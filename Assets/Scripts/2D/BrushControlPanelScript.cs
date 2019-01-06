using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class BrushControlPanelScript : MonoBehaviour
{
    public SliderControlsScript RadiusSliderControlsScript;
    public SliderControlsScript StrengthSliderControlsScript;
    public SliderControlsScript NoiseSliderControlsScript;

    private const float _minRadiusValue = 1;
    private const float _maxRadiusValue = 20;
    private const float _defaultRadiusValue = 4;

    private const float _minStrengthValue = 0.05f;
    private const float _maxStrengthValue = 1;
    private const float _defaultStrengthValue = 0.5f;

    private const float _minNoiseValue = 0;
    private const float _maxNoiseValue = 1;
    private const float _defaultNoiseValue = 0;

    private float _lastRadiusValue = _defaultRadiusValue;
    private float _lastStrengthValue = _defaultStrengthValue;
    private float _lastNoiseValue = _defaultNoiseValue;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
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
        }
        else
        {
            _lastRadiusValue = RadiusSliderControlsScript.CurrentValue;
            _lastStrengthValue = StrengthSliderControlsScript.CurrentValue;
            _lastNoiseValue = NoiseSliderControlsScript.CurrentValue;
        }

        RadiusSliderControlsScript.AllowEventInvoke(state);
        StrengthSliderControlsScript.AllowEventInvoke(state);
        NoiseSliderControlsScript.AllowEventInvoke(state);
    }
}
