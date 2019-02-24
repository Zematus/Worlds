using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MenuPanelScript : ModalPanelScript
{
    private static List<ModalPanelScript> _hiddenInteractionPanels = new List<ModalPanelScript>();

    // Update is called once per frame
    void Update()
    {
        ReadKeyboardInput();
    }

    public void ReadKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetVisible(false);
        }
    }

    public override void SetVisible(bool value)
    {
        if (value && IsMenuPanelActive())
            return; // Can't have more than one menu panel active at a time

        base.SetVisible(value);

        if (!value)
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

    public static ModalPanelScript GetActiveMenuPanel()
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
