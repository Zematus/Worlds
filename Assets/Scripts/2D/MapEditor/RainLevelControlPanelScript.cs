using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class RainLevelControlPanelScript : RegenControlPanelScript
{
    public RainLevelControlPanelScript()
    {
        _minValue = -800f + World.AvgPossibleRainfall;
        _maxValue = 800f + World.AvgPossibleRainfall;
        _defaultValue = World.AvgPossibleRainfall;
    }

    protected override float GetCurrentValueFromManager()
    {
        return Manager.RainfallOffset;
    }
}
