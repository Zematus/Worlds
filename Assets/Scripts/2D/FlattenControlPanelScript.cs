using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class FlattenControlPanelScript : MonoBehaviour
{
    public SliderControlsScript AltitudeScaleSliderControlsScript;

    private const float _minAltitudeScaleOffset = 0.05f;
    private const float _maxAltitudeScaleOffset = 1;
    private const float _defaultAltitudeScaleOffset = 1;

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
            AltitudeScaleSliderControlsScript.MinValue = _minAltitudeScaleOffset;
            AltitudeScaleSliderControlsScript.MaxValue = _maxAltitudeScaleOffset;
            AltitudeScaleSliderControlsScript.DefaultValue = _defaultAltitudeScaleOffset;

            AltitudeScaleSliderControlsScript.CurrentValue = Manager.AltitudeScale;
            AltitudeScaleSliderControlsScript.Initialize();
        }
    }
}
