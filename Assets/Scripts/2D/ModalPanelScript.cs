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
}
