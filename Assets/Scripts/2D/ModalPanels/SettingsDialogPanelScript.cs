using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System;

public class SettingsDialogPanelScript : MenuPanelScript
{
    public Toggle FullscreenToggle;
    public Toggle UIScalingToggle;
    public Toggle AnimationShadersToggle;

    public Text DevModeButtonText;

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

    public override void SetVisible(bool state)
    {
        if (state)
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
        else
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

        base.SetVisible(state);
    }

    public void ValidateKeyboardSettings(InputField inputField)
    {
        if (!IsKeyboardAxisSensitivityValid(inputField.text))
        {
            float parsedValue;
            Debug.Log(String.Format("Invalid axis sensitivity entered for field {0}, value: {1}", inputField.name, inputField.text));
            bool foundUnknownField = false;
            if (inputField == KeyboardXAxisSensitivity)
            {
                parsedValue = Manager.KeyboardXAxisSensitivity;
            }
            else if (inputField == KeyboardYAxisSensitivity)
            {
                parsedValue = Manager.KeyboardYAxisSensitivity;
            }
            else
            {
                int defaultValue = 50;
                Debug.Log(String.Format("Handling input for an unknown InputField {0}, defaulting to {1}", inputField.ToString(), defaultValue));
                parsedValue = defaultValue;
                foundUnknownField = true;
            }
            if (!foundUnknownField)
            {
                // TODO: show a dialog with an error message: "enter a valid number between 1 and 100"
                inputField.Select();
            }
        }
    }

    private bool IsKeyboardAxisSensitivityValid(string text, float min = 1f, float max = 100f)
    {
        float parsedValue = min;
        try
        {
            parsedValue = float.Parse(text);
        }
        catch (FormatException e)
        {
            Debug.Log(String.Format("Invalid axis sensitivity entered {0}", text));
            return false;
        }
        if (parsedValue < min || parsedValue > max)
        {
            Debug.Log(String.Format("Axis sensitivity out-of-range entered {0}", text));
            return false;
        }
        return true;
    }
}
