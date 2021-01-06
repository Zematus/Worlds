using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ActionToolbarScript : MonoBehaviour
{
    public ActionPanelScript TerritoryActionPanel;

    public void SetVisible(bool state)
    {
        if (state)
        {
            SetActions();
        }

        gameObject.SetActive(state);
    }

    private void SetActions()
    {
        foreach (Action action in Action.Actions.Values)
        {
            switch (action.Category)
            {
                case Action.TerritoryCategoryId:
                    TerritoryActionPanel.AddActionButton(action);
                    break;

                default:
                    throw new System.Exception("Unsupported action category type: " +
                        action.Category + ", action id: " + action.Id);
            }
        }
    }
}
