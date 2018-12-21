using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class MapEditorToolbarScript : MonoBehaviour
{
    public SliderControlsScript AltitudeScaleSliderControlsScript;
    public SliderControlsScript SeaLevelSliderControlsScript;
    public SliderControlsScript TempLevelSliderControlsScript;
    public SliderControlsScript RainLevelSliderControlsScript;

    private const float _minAltitudeScaleOffset = 0;
    private const float _maxAltitudeScaleOffset = 100;
    private const float _defaultAltitudeScaleOffset = 100;

    private const float _minSeaLevelOffset = -10000;
    private const float _maxSeaLevelOffset = 10000;
    private const float _defaultSeaLevelOffset = 0;

    private const float _minTemperatureOffset = -50 + World.AvgPossibleTemperature;
    private const float _maxTemperatureOffset = 50 + World.AvgPossibleTemperature;
    private const float _defaultTemperatureOffset = World.AvgPossibleTemperature;

    private const float _minRainfallOffset = -800f + World.AvgPossibleRainfall;
    private const float _maxRainfallOffset = 800f + World.AvgPossibleRainfall;
    private const float _defaultRainfallOffset = World.AvgPossibleRainfall;

    // Use this for initialization
    void Start()
    {
        AltitudeScaleSliderControlsScript.MinValue = _minAltitudeScaleOffset;
        AltitudeScaleSliderControlsScript.MaxValue = _maxAltitudeScaleOffset;
        AltitudeScaleSliderControlsScript.DefaultValue = _defaultAltitudeScaleOffset;
        AltitudeScaleSliderControlsScript.Initialize();

        SeaLevelSliderControlsScript.MinValue = _minSeaLevelOffset;
        SeaLevelSliderControlsScript.MaxValue = _maxSeaLevelOffset;
        SeaLevelSliderControlsScript.DefaultValue = _defaultSeaLevelOffset;
        SeaLevelSliderControlsScript.Initialize();

        TempLevelSliderControlsScript.MinValue = _minTemperatureOffset;
        TempLevelSliderControlsScript.MaxValue = _maxTemperatureOffset;
        TempLevelSliderControlsScript.DefaultValue = _defaultTemperatureOffset;
        TempLevelSliderControlsScript.Initialize();

        RainLevelSliderControlsScript.MinValue = _minRainfallOffset;
        RainLevelSliderControlsScript.MaxValue = _maxRainfallOffset;
        RainLevelSliderControlsScript.DefaultValue = _defaultRainfallOffset;
        RainLevelSliderControlsScript.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
