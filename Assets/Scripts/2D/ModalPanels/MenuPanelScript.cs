using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MenuPanelScript : ModalPanelScript
{
    public UnityEvent DialogEscapedEvent;

    // Update is called once per frame
    void Update()
    {
        ReadKeyboardInput();
    }

    private void ReadKeyboardInput()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            EscapeDialog();
        }
    }

    public void EscapeDialog()
    {
        SetVisible(false);

        DialogEscapedEvent.Invoke();
    }

    public override void SetVisible(bool state)
    {
        if (state && GuiManagerScript.IsModalPanelActive())
            return; // Can't have more than one menu panel active at a time

        base.SetVisible(state);

        if (!state)
        {
            GuiManagerScript.ManagerScript.ShowHiddenInteractionPanels();
        }
    }
}
