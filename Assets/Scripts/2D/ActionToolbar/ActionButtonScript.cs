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

        bool canExecute = _action.CanExecute();
        Button.interactable = canExecute;

        ButtonWithTooltipScript buttonScript = Button.GetComponent<ButtonWithTooltipScript>();
        if (canExecute)
        {
            buttonScript.UpdateTooltip(null);
        }
        else
        {
            buttonScript.UpdateTooltip(_action.BuildExecuteInfoText());
        }
    }

    private void AddActionToExecute()
    {
        Manager.CurrentWorld.SetActionToExecute(_action);
    }

    public void SetAction(ModAction action)
    {
        ButtonWithTooltipScript buttonScript = Button.GetComponent<ButtonWithTooltipScript>();
        buttonScript.Init(action.Name);

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
