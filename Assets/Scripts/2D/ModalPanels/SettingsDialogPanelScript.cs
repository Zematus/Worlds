using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System;
using System.ComponentModel;

public class SettingsDialogPanelScript : MenuPanelScript
{
    public Toggle FullscreenToggle;
    public Toggle UIScalingToggle;
    public Toggle AnimationShadersToggle;

    public Text DevModeButtonText;

    public DialogPanelScript InvalidSettingsDialogPanelScript;
    public InputField KeyboardXAxisSensitivity;
    public InputField KeyboardYAxisSensitivity;
    public Toggle KeyboardInvertXAxis;
    public Toggle KeyboardInvertYAxis;

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

    public void OpenSettingsDialog()
    {
        LoadSettings();
        SetVisible(true);
    }

    public void CloseIfValid(ModalPanelScript caller)
    {
        if (!IsKeyboardAxisSensitivityValid(KeyboardXAxisSensitivity.text) ||
            !IsKeyboardAxisSensitivityValid(KeyboardYAxisSensitivity.text))
        {
            ShowInvalidSettingsModalDialog("Invalid axis sensitivity entered.\nPlease enter a value in the range [1, 100].");
        }
        else
        {
            SaveSettings();
            SetVisible(false);
            if (caller != null)
            {
                caller.SetVisible(true);
            }
            else
            {
                Debug.LogError("null reference passed in parameter 'parent', this COULD be a bug, perhaps you need to set the parameter to CloseIfValid()");
            }
        }
    }

    public void CloseInvalidSettingsDialog()
    {
        InvalidSettingsDialogPanelScript.SetVisible(false);
        SetVisible(true);
    }

    public void ValidateKeyboardSettings(UnityEngine.Object source)
    {
        if (source == null || (source != KeyboardXAxisSensitivity && source != KeyboardYAxisSensitivity))
        {
            Debug.Log(string.Format("Handling input for an unknown InputField or Object {0}, exiting method", source));
            return;
        }

        var inputField = source as InputField;
        if (!IsKeyboardAxisSensitivityValid(inputField.text))
        {
            Debug.Log(String.Format("Invalid axis sensitivity entered for field {0}, value: {1}", inputField.name, inputField.text));
            ShowInvalidSettingsModalDialog();
        }

    }

    private void LoadSettings()
    {
        FullscreenToggle.isOn = Manager.FullScreenEnabled;
        UIScalingToggle.isOn = Manager.UIScalingEnabled;
        AnimationShadersToggle.isOn = Manager.AnimationShadersEnabled;

        KeyboardXAxisSensitivity.text = String.Format("{0:N2}", Manager.KeyboardXAxisSensitivity);
        KeyboardYAxisSensitivity.text = String.Format("{0:N2}", Manager.KeyboardYAxisSensitivity);
        KeyboardInvertXAxis.isOn = Manager.KeyboardInvertXAxis;
        KeyboardInvertYAxis.isOn = Manager.KeyboardInvertYAxis;

        RefreshDevButtonText();
    }

    private void SaveSettings()
    {
        if (IsKeyboardAxisSensitivityValid(KeyboardXAxisSensitivity.text))
        {
            Manager.KeyboardXAxisSensitivity = float.Parse(KeyboardXAxisSensitivity.text);
        }
        else
        {
            Debug.LogError(String.Format("An invalid keyboard X-axis setting was entered and we didn't catch it. Value: {0}", KeyboardXAxisSensitivity.text));
        }
        if (IsKeyboardAxisSensitivityValid(KeyboardYAxisSensitivity.text))
        {
            Manager.KeyboardYAxisSensitivity = float.Parse(KeyboardYAxisSensitivity.text);
        }
        else
        {
            Debug.LogError(String.Format("An invalid keyboard Y-axis setting was entered and we didn't catch it. Value: {0}", KeyboardYAxisSensitivity.text));
        }
        Manager.KeyboardInvertXAxis = KeyboardInvertXAxis.isOn;
        Manager.KeyboardInvertYAxis = KeyboardInvertYAxis.isOn;
    }

    private bool IsKeyboardAxisSensitivityValid(string text, float min = 1f, float max = 100f)
    {
        float parsedValue;
        try
        {
            parsedValue = float.Parse(text);
        }
        catch (FormatException e)
        {
            Debug.Log(String.Format("Invalid axis sensitivity entered {0}", text));
            Debug.Log(e);
            return false;
        }
        if (parsedValue < min || parsedValue > max)
        {
            Debug.Log(String.Format("Axis sensitivity out-of-range entered {0}", text));
            return false;
        }
        return true;
    }

    private void ShowInvalidSettingsModalDialog(string text = "Please enter a value in the range [1, 100].")
    {
        InvalidSettingsDialogPanelScript.SetDialogText(text);
        SetVisible(false);
        InvalidSettingsDialogPanelScript.SetVisible(true);
    }
}
