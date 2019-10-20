using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class TextInputDialogPanelScript : MenuPanelScript
{
    public Text RecText;

    public InputField TextInputField;

    public Text CancelButtonText;

    public void SetText(string text)
    {
        TextInputField.text = text;
    }

    public string GetText()
    {
        return TextInputField.text;
    }

    public void SetRecommendationTextVisible(bool state)
    {
        RecText.gameObject.SetActive(state);
    }

    public void SetCancelButtonText(string text)
    {
        CancelButtonText.text = text;
    }
}
