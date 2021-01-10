using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ActionCategoryScript : MonoBehaviour
{
    public ActionPanelScript ActionPanel;

    public Image ToogleImage;

    public string TooltipText;

    public TooltipHandlerScript TooltipHandler;

    public void Remove()
    {
        gameObject.SetActive(false);

        Destroy(gameObject);
    }

    public void UpdateTooltip()
    {
        TooltipHandler.SetText(TooltipText);
    }
}
