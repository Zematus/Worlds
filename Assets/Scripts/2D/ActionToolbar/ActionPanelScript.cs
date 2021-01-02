using UnityEngine;
using System.Collections.Generic;

public class ActionPanelScript : MonoBehaviour
{
    public ActionButtonScript ActionButtonPrefab;

    private HashSet<ActionButtonScript> _buttons = new HashSet<ActionButtonScript>();

    public void AddActionButton()
    {
        ActionButtonScript button =
            Instantiate(ActionButtonPrefab) as ActionButtonScript;

        button.transform.SetParent(transform);
        button.transform.localScale = Vector3.one;

        _buttons.Add(button);
    }

    public void ClearActionButtons()
    {
        foreach (ActionButtonScript button in _buttons)
        {
            button.Remove();
        }

        _buttons.Clear();
    }
}
