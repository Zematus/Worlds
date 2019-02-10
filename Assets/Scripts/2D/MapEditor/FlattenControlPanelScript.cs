using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class FlattenControlPanelScript : RegenControlPanelScript
{
    public FlattenControlPanelScript()
    {
        _minValue = 0.05f;
        _maxValue = 1;
        _defaultValue = 1;
    }

    protected override float GetCurrentValueFromManager()
    {
        return Manager.AltitudeScale;
    }
}
