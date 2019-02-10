using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class TempLevelControlPanelScript : RegenControlPanelScript
{
    public TempLevelControlPanelScript()
    {
        _minValue = -40 - World.AvgPossibleTemperature;
        _maxValue = 50 + World.AvgPossibleTemperature;
        _defaultValue = World.AvgPossibleTemperature;
    }

    protected override float GetCurrentValueFromManager()
    {
        return Manager.TemperatureOffset;
    }
}
