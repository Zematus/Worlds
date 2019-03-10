using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MenuPanelScript : ModalPanelScript
{
    public UnityEvent DialogEscapedEvent;

    private static List<ModalPanelScript> _hiddenInteractionPanels = new List<ModalPanelScript>();

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
        if (state && IsMenuPanelActive())
            return; // Can't have more than one menu panel active at a time

        base.SetVisible(state);

        if (!state)
        {
            ShowHiddenInteractionPanels();
        }
    }

    public static bool IsMenuPanelActive()
    {
        GameObject[] panels = GameObject.FindGameObjectsWithTag("MenuPanel");

        foreach (GameObject panel in panels)
        {
            if (panel.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    public static ModalPanelScript GetActiveModalPanel()
    {
        GameObject[] panels = GameObject.FindGameObjectsWithTag("MenuPanel");

        foreach (GameObject panel in panels)
        {
            if (panel.activeInHierarchy)
            {
                ModalPanelScript modalPanel = panel.GetComponent<ModalPanelScript>();

                return modalPanel;
            }
        }

        return null;
    }

    public static void HideInteractionPanel(ModalPanelScript panel)
    {
        _hiddenInteractionPanels.Add(panel);
    }

    public static void ShowHiddenInteractionPanels()
    {
        foreach (ModalPanelScript panel in _hiddenInteractionPanels)
        {
            panel.SetVisible(true);
        }

        _hiddenInteractionPanels.Clear();
    }
}
