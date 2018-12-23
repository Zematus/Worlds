using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class SeaLevelControlPanelScript : MonoBehaviour
{
    public SliderControlsScript SeaLevelSliderControlsScript;

    private const float _minSeaLevelOffset = -10000;
    private const float _maxSeaLevelOffset = 10000;
    private const float _defaultSeaLevelOffset = 0;

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
            SeaLevelSliderControlsScript.MinValue = _minSeaLevelOffset;
            SeaLevelSliderControlsScript.MaxValue = _maxSeaLevelOffset;
            SeaLevelSliderControlsScript.DefaultValue = _defaultSeaLevelOffset;

            SeaLevelSliderControlsScript.CurrentValue = Manager.SeaLevelOffset;
            SeaLevelSliderControlsScript.Initialize();
        }
    }
}
