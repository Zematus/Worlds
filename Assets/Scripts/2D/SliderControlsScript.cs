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

    public float MinValue = 0;
    public float MaxValue = 100;
    public float DefaultValue = 100;

    public float CurrentValue = 100;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Initialize()
    {
        Slider.minValue = MinValue;
        Slider.maxValue = MaxValue;

        SymbolText.text = UnitSymbol;

        Reset();
    }

    public void SetValueFromSlider(System.Single value)
    {
        SetValue(value);
    }

    public void SetValueFromInputField(string valueStr)
    {
        float value = 0;

        float.TryParse(valueStr, out value);

        SetValue(value);
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
    }
}
