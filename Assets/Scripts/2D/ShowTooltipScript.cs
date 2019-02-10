using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShowTooltipScript : MonoBehaviour
{
    GameObject TooltipGameObject;

    public void SetText(string text)
    {
        Text tooltipText = TooltipGameObject.GetComponent<Text>();
        tooltipText.text = text;
    }

    public void SetVisible(bool value)
    {
        TooltipGameObject.SetActive(value);
    }

    public void SetPosition(Vector3 position)
    {
        TooltipGameObject.transform.position = position;
    }

    public void DisplayTip(string text, Vector3 position)
    {
        SetText(text);
        SetPosition(position);
        SetVisible(true);
    }
}
