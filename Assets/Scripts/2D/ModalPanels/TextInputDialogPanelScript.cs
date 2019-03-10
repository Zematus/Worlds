using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class TextInputDialogPanelScript : MenuPanelScript
{
    public InputField TextInputField;

    public void SetText(string text)
    {
        TextInputField.text = text;
    }

    public string GetText()
    {
        return TextInputField.text;
    }
}
