using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class SliderControlsScript : MonoBehaviour
{
    public Button ResetButton;
    public Slider Slider;
    public InputField InputField;
    public Text SymbolText;

    public string InputFormat = "0.000";
    public string UnitSymbol = "%";

    public bool IsPercentFormat = true;

    public float MinValue = 0;
    public float MaxValue = 100;
    public float DefaultValue = 100;

    public float CurrentValue = 100;

    public ValueSetEvent ValueSetEvent;

    private const float _maxTimeToInvokeEvent = 1;

    private bool _hasToInvokeEvent = false;
    private float _timeToInvokeEvent = -1;

    private bool _allowInvokeEvent = false;

    private float _lastCurrentValueSet = 0;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (_hasToInvokeEvent)
        {
            _timeToInvokeEvent -= Time.deltaTime;

            if (_timeToInvokeEvent < 0)
            {
                InvokeEvent();

                _hasToInvokeEvent = false;
            }
        }
    }

    public void Initialize()
    {
        Slider.minValue = MinValue;
        Slider.maxValue = MaxValue;

        SymbolText.text = UnitSymbol;

        SetValue(CurrentValue);
    }

    public void SetValueFromSlider(System.Single value)
    {
        SetValue(value);

        if (_allowInvokeEvent)
        {
            _timeToInvokeEvent = _maxTimeToInvokeEvent;
            _hasToInvokeEvent = true;
        }
    }

    public void SetValueFromInputField(string valueStr)
    {
        float value = 0;

        float.TryParse(valueStr, out value);

        if (IsPercentFormat)
        {
            value /= 100f;
        }

        SetValue(value);

        InvokeEvent();
    }

    public void SetValue(float value)
    {
        if (value < MinValue)
            value = MinValue;

        if (value > MaxValue)
            value = MaxValue;

        CurrentValue = value;

        Slider.value = value;

        InputField.text = value.ToString(InputFormat);
    }

    public void Reset()
    {
        SetValue(DefaultValue);

        InvokeEvent();
    }

    private void InvokeEvent()
    {
        if (_lastCurrentValueSet != CurrentValue)
        {
            ValueSetEvent.Invoke(CurrentValue);

            _lastCurrentValueSet = CurrentValue;
        }
    }

    public void AllowEventInvoke(bool value)
    {
        _allowInvokeEvent = value;

        if (value)
        {
            _lastCurrentValueSet = CurrentValue;
        }
    }
}
