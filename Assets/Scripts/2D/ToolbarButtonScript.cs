using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class ToolbarButtonScript : MonoBehaviour
{
    public bool IsOn = false;

    public GameObject EnabledShadow;

    public ToggleEvent ToggleEvent;

    // Use this for initialization
    void Start()
    {
        SetState(false);
    }

    public void SetState(bool value)
    {
        IsOn = value;

        EnabledShadow.SetActive(value);

        ToggleEvent.Invoke(value);
    }

    public void Toggle()
    {
        SetState(!IsOn);
    }
}
