using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class ModalPanelScript : MonoBehaviour
{
    public CanvasGroup ModalPanelCanvasGroup;

    public virtual void SetVisible(bool state)
    {
        if (!state && !gameObject.activeInHierarchy)
            return; // There's no need to make this dialog invisible if it already is. Also, we want to avoid disabling the canvas group if it's being enabled by another dialog

        ModalPanelCanvasGroup.GetComponent<ModalActivationScript>().Activate(state);
        ModalPanelCanvasGroup.blocksRaycasts = state;

        gameObject.SetActive(state);
    }
}
