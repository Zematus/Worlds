using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonWithTooltipScript : MonoBehaviour
{
    public Text ButtonText;
    public Text TooltipText;

    public GameObject TooltipPanel;

    bool canShow = true;

    public void Init(string text, string tooltipText = "")
    {
        ButtonText.text = text;
        TooltipText.text = tooltipText;
        TooltipPanel.gameObject.SetActive(false);

        canShow = !string.IsNullOrWhiteSpace(tooltipText);
    }

    public void UpdateTooltip(string tooltipText)
    {
        TooltipText.text = tooltipText;
        canShow = !string.IsNullOrWhiteSpace(tooltipText);
    }

    public void PointerEnterHandler()
    {
        TooltipPanel.gameObject.SetActive(canShow);
    }

    public void PointerExitHandler()
    {
        TooltipPanel.gameObject.SetActive(false);
    }
}
