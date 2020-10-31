using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class SettingsDialogPanelScript : DialogPanelScript
{
    public Toggle FullscreenToggle;
    public Toggle UIScalingToggle;
    public Toggle AnimationShadersToggle;

    public Text DevModeButtonText;

    public void RefreshDevButtonText()
    {
        switch (Manager.CurrentDevMode)
        {
            case DevMode.None:
                DevModeButtonText.text = "None";
                break;
            case DevMode.Basic:
                DevModeButtonText.text = "Basic";
                break;
            case DevMode.Advanced:
                DevModeButtonText.text = "Advanced";
                break;
            default:
                throw new System.Exception("Unhandled Dev Mode: " + Manager.CurrentDevMode);
        }
    }
}
