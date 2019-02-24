using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class ProgressDialogPanelScript : ImageDialogPanelScript
{
    public void SetProgress(float value)
    {
        Image.fillAmount = value;
    }
}
