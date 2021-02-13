using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ActionButtonScript : MonoBehaviour
{
    public Text Text;

    public Button Button;

    private ModAction _action;

    private long _lastDate = -1;

    public void Update()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (_lastDate == Manager.CurrentWorld.CurrentDate)
            return;

        _lastDate = Manager.CurrentWorld.CurrentDate;

        _action.SetTarget(Manager.CurrentWorld.GuidedFaction);

        Button.interactable = _action.CanExecute();
    }

    private void AddActionToExecute()
    {
        Manager.CurrentWorld.SetActionToExecute(_action);

        if (Manager.SimulationCanRun && !Manager.SimulationRunning)
        {
            Manager.SetToPerformSimulationStep(true);
        }
    }

    public void SetAction(ModAction action)
    {
        Text.text = action.Name;

        _action = action;

        _lastDate = -1;

        Button.onClick.AddListener(AddActionToExecute);
    }

    public void Remove()
    {
        gameObject.SetActive(false);

        Destroy(gameObject);
    }
}
