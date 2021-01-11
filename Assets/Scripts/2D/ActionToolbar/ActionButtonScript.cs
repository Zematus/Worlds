using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ActionButtonScript : MonoBehaviour
{
    public Text Text;

    public Button Button;

    private Action _action;

    private long _lastDate = -1;

    public void Update()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (_lastDate == Manager.CurrentWorld.CurrentDate)
            return;

        _lastDate = Manager.CurrentWorld.CurrentDate;

        _action.SetTarget(Manager.CurrentWorld.GuidedFaction);

        Button.interactable = _action.CanUse();
    }

    public void SetAction(Action action)
    {
        Text.text = action.Name;

        _action = action;

        _lastDate = -1;

        Button.onClick.AddListener(_action.Use);
    }

    public void Remove()
    {
        gameObject.SetActive(false);

        Destroy(gameObject);
    }
}
