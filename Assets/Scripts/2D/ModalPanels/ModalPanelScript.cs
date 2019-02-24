using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class ModalPanelScript : MonoBehaviour
{
    public CanvasGroup ModalPanelCanvasGroup;

    public virtual void SetVisible(bool value)
    {
        ModalPanelCanvasGroup.gameObject.SetActive(value);
        ModalPanelCanvasGroup.blocksRaycasts = value;

        gameObject.SetActive(value);
    }

    public static bool IsBackgroundActivityPanelActive()
    {
        GameObject[] panels = GameObject.FindGameObjectsWithTag("BackgroundActivityPanel");

        foreach (GameObject panel in panels)
        {
            if (panel.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsInteractionPanelActive()
    {
        GameObject[] panels = GameObject.FindGameObjectsWithTag("InteractionPanel");

        foreach (GameObject panel in panels)
        {
            if (panel.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }
}
