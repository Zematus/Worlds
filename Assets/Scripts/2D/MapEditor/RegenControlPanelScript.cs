using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public abstract class RegenControlPanelScript : MonoBehaviour
{
    public GuiManagerScript GuiManager;

    public abstract void ResetSliderControls();
    public abstract void AllowEventInvoke(bool state);

    public virtual void Activate(bool state)
    {
        gameObject.SetActive(state);

        if (state)
        {
            ResetSliderControls();

            GuiManager.RegisterLoadWorldPostProgressOp(ResetSliderControls);
        }
        else
        {
            GuiManager.DeregisterLoadWorldPostProgressOp(ResetSliderControls);
        }

        AllowEventInvoke(state);
    }
}
