using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ActionButtonScript : MonoBehaviour
{
    public Text Text;

    public Button Button;

    private ModAction _action;

    public void Update()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (Manager.ResolvingPlayerInvolvedDecisionChain)
            return;

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

        Button.onClick.AddListener(AddActionToExecute);
    }

    public void Remove()
    {
        gameObject.SetActive(false);

        Destroy(gameObject);
    }
}
