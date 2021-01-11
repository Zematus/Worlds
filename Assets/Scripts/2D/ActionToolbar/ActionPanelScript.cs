using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ActionPanelScript : MonoBehaviour
{
    public ActionButtonScript ActionButtonPrefab;

    public Text Title;

    private HashSet<ActionButtonScript> _buttons = new HashSet<ActionButtonScript>();

    public void AddActionButton(Action action)
    {
        ActionButtonScript button = Instantiate(ActionButtonPrefab);

        button.transform.SetParent(transform);
        button.transform.localScale = Vector3.one;

        button.SetAction(action);

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
