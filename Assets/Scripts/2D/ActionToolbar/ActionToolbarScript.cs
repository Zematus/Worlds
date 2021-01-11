using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ActionToolbarScript : MonoBehaviour
{
    public Transform ToggleSet;

    public ActionCategoryScript ActionCategoryPrefab;

    private Dictionary<string, ActionCategoryScript> _actionCategoryToggles =
        new Dictionary<string, ActionCategoryScript>();

    private bool _categoriesSet = false;

    public void SetVisible(bool state)
    {
        if (state)
        {
            UpdateActionCategories();
        }

        gameObject.SetActive(state);
    }

    private void SetupActionCategories()
    {
        if (!_categoriesSet)
        {
            // We want to make sure the categories are added to the toolbar in the same
            // order they were loaded
            foreach (string categoryId in ActionCategory.CategoryKeys)
            {
                AddActionCategoryToggle(ActionCategory.Categories[categoryId]);
            }

            _categoriesSet = true;
        }
        else
        {
            foreach (ActionCategoryScript toggle in _actionCategoryToggles.Values)
            {
                toggle.gameObject.SetActive(false);
                toggle.ActionPanel.ClearActionButtons();
            }
        }
    }

    public void UpdateActionCategories()
    {
        Faction guidedFaction = Manager.CurrentWorld.GuidedFaction;

        if (guidedFaction == null)
        {
            throw new System.Exception("No faction is being guided by the player");
        }

        SetupActionCategories();

        foreach (Action action in Action.Actions.Values)
        {
            action.SetTarget(guidedFaction);

            if (!action.CanAccess())
                continue;

            ActionCategoryScript toggle =
                _actionCategoryToggles[action.Category];

            toggle.gameObject.SetActive(true);
            toggle.ActionPanel.AddActionButton(action);
        }
    }

    private void AddActionCategoryToggle(ActionCategory category)
    {
        ActionCategoryScript toggle = Instantiate(ActionCategoryPrefab);

        toggle.ToogleImage.sprite = category.Image;
        toggle.TooltipText = category.Name;
        toggle.ActionPanel.Title.text = category.Name;

        toggle.transform.SetParent(ToggleSet);
        toggle.transform.localScale = Vector3.one;

        _actionCategoryToggles.Add(category.Id, toggle);
    }

    public void ClearActionCategories()
    {
        foreach (ActionCategoryScript category in _actionCategoryToggles.Values)
        {
            category.Remove();
        }

        _actionCategoryToggles.Clear();
        _categoriesSet = false;
    }
}
