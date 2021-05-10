using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MenuPanelScript : ModalPanelScript
{
    public UnityEvent MenuEscapedEvent;
    public UnityEvent MenuHiddenEvent;

    // Update is called once per frame
    void Update()
    {
        ReadKeyboardInput();
    }

    protected virtual void ReadKeyboardInput()
    {
        Manager.HandleKeyUp(KeyCode.Escape, false, false, EscapeDialog, false);
    }

    public void EscapeDialog()
    {
        SetVisible(false);
        
        MenuEscapedEvent.Invoke();
    }

    public override void SetVisible(bool state)
    {
        if (state && GuiManagerScript.IsModalPanelActive())
            return; // Can't have more than one menu panel active at a time

        base.SetVisible(state);

        if (!state)
        {
            MenuHiddenEvent.Invoke();
        }
    }
}
