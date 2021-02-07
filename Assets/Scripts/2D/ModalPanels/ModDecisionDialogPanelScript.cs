using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class ModDecisionDialogPanelScript : ModalPanelScript
{
    public Text DecisionText;

    public Transform OptionsPanelTransform;

    public Button OptionButtonPrefab;

    public Text SpeedButtonText;

    public int ResumeSpeedLevelIndex;

    public UnityEvent OptionChosenEvent;

    private readonly List<Button> _optionButtons = new List<Button>();

    private ModDecision _decision;

    private void SetOptions()
    {
        _optionButtons.Add(OptionButtonPrefab);

        DecisionOption[] options = _decision.Options;

        int i = 0;

        foreach (DecisionOption option in options)
        {
            // Skip options that can only be used by the simulation AI
            if (option.AllowedGuide == GuideType.Simulation)
                continue;

            if (!option.CanShow())
                continue;

            SetOptionButton(option, i);

            i++;
        }
    }

    private void SetOptionButton(DecisionOption option, int index)
    {
        Button button;

        string text = option.Text.GetFormattedString();

        string descriptionText = "Effects:";

        if (option.Effects != null)
        {
            foreach (DecisionOptionEffect effect in option.Effects)
            {
                descriptionText += "\n\t• " + effect.Text.GetFormattedString();
            }
        }
        else
        {
            descriptionText += "\n\t• None";
        }

        if (index < _optionButtons.Count)
        {
            button = _optionButtons[index];
        }
        else
        {
            button = AddOptionButton();
        }

        ButtonWithTooltipScript buttonScript = button.GetComponent<ButtonWithTooltipScript>();
        buttonScript.ButtonText.text = text;
        buttonScript.TooltipText.text = descriptionText;
        buttonScript.TooltipPanel.gameObject.SetActive(false);

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            if (option.Effects != null)
            {
                foreach (DecisionOptionEffect effect in option.Effects)
                {
                    effect.Result.Apply();
                }
            }

            OptionChosenEvent.Invoke();
        });
    }

    private Button AddOptionButton()
    {
        Button newButton = Instantiate(OptionButtonPrefab) as Button;

        newButton.transform.SetParent(OptionsPanelTransform, false);

        _optionButtons.Add(newButton);

        return newButton;
    }

    private void RemoveOptionButtons()
    {
        bool first = true;

        foreach (Button button in _optionButtons)
        {
            if (first)
            {
                first = false;
                continue;
            }

            Destroy(button.gameObject);
        }

        _optionButtons.Clear();
    }

    public override void SetVisible(bool state)
    {
        base.SetVisible(state);

        if (state)
        {
            SetOptions();
        }
        else
        {
            RemoveOptionButtons();
        }
    }

    public void Set(ModDecision decision, int currentSpeedIndex)
    {
        ResumeSpeedLevelIndex = currentSpeedIndex;

        SpeedButtonText.text = Speed.Levels[currentSpeedIndex];

        string dialogText = "";

        bool notFirst = false;
        foreach (OptionalDescription description in decision.DescriptionSegments)
        {
            if (description.CanShow())
            {
                // every new description segment should start on a new line
                if (notFirst)
                {
                    dialogText += "\n";
                }

                dialogText += description.Text.GetFormattedString();
                notFirst = true;
            }
        }

        DecisionText.text = dialogText;

        _decision = decision;
    }

    public void SelectNextSpeedLevel()
    {
        ResumeSpeedLevelIndex++;

        if (ResumeSpeedLevelIndex == Speed.Levels.Length)
        {
            ResumeSpeedLevelIndex = -1;

            SpeedButtonText.text = Speed.Zero;
        }
        else
        {
            SpeedButtonText.text = Speed.Levels[ResumeSpeedLevelIndex];
        }
    }
}
