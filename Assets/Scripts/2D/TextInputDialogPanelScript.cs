using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class TextInputDialogPanelScript : DialogPanelScript
{
    public InputField NameInputField;

    public void SetName(string name)
    {
        NameInputField.text = name;
    }

    public string GetName()
    {
        return NameInputField.text;
    }
}
