using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class SeaLevelControlPanelScript : RegenControlPanelScript
{
    public SeaLevelControlPanelScript()
    {
        _minValue = -10000;
        _maxValue = 10000;
        _defaultValue = 0;
    }

    protected override float GetCurrentValueFromManager()
    {
        return Manager.SeaLevelOffset;
    }
}
