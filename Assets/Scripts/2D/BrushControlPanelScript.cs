using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BrushControlPanelScript : MonoBehaviour
{
    public SliderControlsScript RadiusSliderControlsScript;
    public SliderControlsScript StrengthSliderControlsScript;
    public SliderControlsScript NoiseSliderControlsScript;

    public ValueSetEvent RadiusValueSetEvent;
    public ValueSetEvent StrengthValueSetEvent;
    public ValueSetEvent NoiseValueSetEvent;

    public List<Toggle> BrushToggles = new List<Toggle>();

    private const float _minRadiusValue = 1;
    private const float _maxRadiusValue = 20;
    private const float _defaultRadiusValue = 4;

    private const float _minStrengthValue = -1f;
    private const float _maxStrengthValue = 1;
    private const float _defaultStrengthValue = 0f;

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

    public void SetRadiusValue(float value)
    {
        //RadiusValueSetEvent.Invoke(value);

        Manager.BrushRadius = (int)value;
    }

    public void SetStrengthValue(float value)
    {
        //StrengthValueSetEvent.Invoke(value);
    }

    public void SetNoiseValue(float value)
    {
        //NoiseValueSetEvent.Invoke(value);
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
            
            //RadiusValueSetEvent.Invoke(_lastRadiusValue);
            //StrengthValueSetEvent.Invoke(_lastStrengthValue);
            //NoiseValueSetEvent.Invoke(_lastNoiseValue);

            Manager.BrushRadius = (int)_lastRadiusValue;
            Manager.IsBrushActive = true;
        }
        else
        {
            _lastRadiusValue = RadiusSliderControlsScript.CurrentValue;
            _lastStrengthValue = StrengthSliderControlsScript.CurrentValue;
            _lastNoiseValue = NoiseSliderControlsScript.CurrentValue;

            bool mantainActive = false;
            foreach (Toggle toggle in BrushToggles)
            {
                mantainActive |= toggle.isOn;
            }

            Manager.IsBrushActive = mantainActive;
        }

        RadiusSliderControlsScript.AllowEventInvoke(state);
        StrengthSliderControlsScript.AllowEventInvoke(state);
        NoiseSliderControlsScript.AllowEventInvoke(state);
    }
}
