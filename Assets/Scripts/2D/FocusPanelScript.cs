using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FocusPanelState
{
    SetFocus,
    UnsetFocus
}

public class FocusPanelScript : MonoBehaviour
{
    public Text PolityText;

    public Button SetFocusButton;
    public Button UnsetFocusButton;
    public Button GuideFactionButton;

    public void SetVisible(bool value)
    {
        gameObject.SetActive(value);
    }

    public void SetState(FocusPanelState state, Polity polity)
    {
        PolityText.text = polity.Name.Text + " " + polity.Type;

        switch (state)
        {
            case FocusPanelState.SetFocus:
                SetFocusButton.gameObject.SetActive(true);
                UnsetFocusButton.gameObject.SetActive(false);
                GuideFactionButton.gameObject.SetActive(true);
                break;

            case FocusPanelState.UnsetFocus:
                SetFocusButton.gameObject.SetActive(false);
                UnsetFocusButton.gameObject.SetActive(true);
                GuideFactionButton.gameObject.SetActive(true);
                break;
        }
    }
}
