using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class TempLevelControlPanelScript : MonoBehaviour
{
    public SliderControlsScript TempLevelSliderControlsScript;

    private const float _minTemperatureOffset = -40 - World.AvgPossibleTemperature;
    private const float _maxTemperatureOffset = 50 + World.AvgPossibleTemperature;
    private const float _defaultTemperatureOffset = World.AvgPossibleTemperature;

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
            TempLevelSliderControlsScript.MinValue = _minTemperatureOffset;
            TempLevelSliderControlsScript.MaxValue = _maxTemperatureOffset;
            TempLevelSliderControlsScript.DefaultValue = _defaultTemperatureOffset;

            TempLevelSliderControlsScript.CurrentValue = Manager.TemperatureOffset;
            TempLevelSliderControlsScript.Initialize();
        }
    }
}
