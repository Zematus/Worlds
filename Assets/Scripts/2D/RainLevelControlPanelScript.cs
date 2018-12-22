using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class RainLevelControlPanelScript : MonoBehaviour
{
    public SliderControlsScript RainLevelSliderControlsScript;
    
    private const float _minRainfallOffset = -800f + World.AvgPossibleRainfall;
    private const float _maxRainfallOffset = 800f + World.AvgPossibleRainfall;
    private const float _defaultRainfallOffset = World.AvgPossibleRainfall;

    // Use this for initialization
    void Start()
    {
        RainLevelSliderControlsScript.MinValue = _minRainfallOffset;
        RainLevelSliderControlsScript.MaxValue = _maxRainfallOffset;
        RainLevelSliderControlsScript.DefaultValue = _defaultRainfallOffset;
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
            RainLevelSliderControlsScript.CurrentValue = Manager.RainfallOffset;
            RainLevelSliderControlsScript.Initialize();
        }
    }
}
