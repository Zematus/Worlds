using UnityEngine;
using System.Collections.Generic;

public class ActionPanelScript : MonoBehaviour
{
    public ActionButtonScript ActionButtonPrefab;

    private HashSet<ActionButtonScript> _buttons = new HashSet<ActionButtonScript>();

    public void AddActionButton(Action action)
    {
        ActionButtonScript button = Instantiate(ActionButtonPrefab);

        button.transform.SetParent(transform);
        button.transform.localScale = Vector3.one;

        button.SetText(action.Name);

        button.gameObject.SetActive(true);

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
