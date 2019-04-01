using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class ToggleImageScript : MonoBehaviour
{
    public bool Enabled;

    public Image ImageOn;
    public Image ImageOff;

    public void SetEnabled(bool state)
    {
        Enabled = state;

        ImageOn.gameObject.SetActive(state);
        ImageOff.gameObject.SetActive(!state);
    }

    public void Toggle()
    {
        SetEnabled(!Enabled);
    }
}
