using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MapEditorToolbarScript : MonoBehaviour
{
    public GuiManagerScript GuiManager;

    public Toggle Toggle1;
    public Toggle Toggle2;
    public Toggle Toggle3;
    public Toggle Toggle4;
    public Toggle Toggle5;
    public Toggle Toggle6;
    public Toggle Toggle7;

    public Button UndoActionButton;
    public Button RedoActionButton;

    public ValueSetEvent RegenerateWorldAltitudeScaleChangeEvent;
    public ValueSetEvent RegenerateWorldSeaLevelOffsetChangeEvent;
    public ValueSetEvent RegenerateWorldTemperatureOffsetChangeEvent;
    public ValueSetEvent RegenerateWorldRainfallOffsetChangeEvent;

    // Use this for initialization
    void Start()
    {
        GuiManager.RegisterGenerateWorldPostProgressOp(Manager.ResetActionStacks);
        GuiManager.RegisterLoadWorldPostProgressOp(Manager.ResetActionStacks);

        Manager.RegisterUndoStackUpdateOp(OnUndoStackUpdate);
        Manager.RegisterRedoStackUpdateOp(OnRedoStackUpdate);
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameObject.activeInHierarchy)
            return;

        ReadKeyboardInput();
    }

    public void ReadKeyboardInput()
    {
        bool controlPressed = false;
        bool shiftPressed = false;

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            controlPressed = true;
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            shiftPressed = true;
        }

        if (controlPressed)
        {
            if (shiftPressed)
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    RedoEditorAction();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                UndoEditorAction();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Toggle1.isOn = !Toggle1.isOn;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Toggle2.isOn = !Toggle2.isOn;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Toggle3.isOn = !Toggle3.isOn;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Toggle4.isOn = !Toggle4.isOn;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Toggle5.isOn = !Toggle5.isOn;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                Toggle6.isOn = !Toggle6.isOn;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                Toggle7.isOn = !Toggle7.isOn;
            }
        }
    }

    public void UndoEditorAction()
    {
        Manager.UndoEditorAction();
    }

    public void RedoEditorAction()
    {
        Manager.RedoEditorAction();
    }

    private void OnRegenCompletion()
    {
        GuiManager.DeregisterRegenerateWorldPostProgressOp(OnRegenCompletion);

        Manager.BlockUndoAndRedo(false);
    }

    private void PerformRegenerateWorldAction(System.Action<float> action, float previousValue, float newValue)
    {
        EditorAction editorAction = new RegenerateWorldAction
        {
            Action = action,
            PreviousValue = previousValue,
            NewValue = newValue
        };

        Manager.PerformEditorAction(editorAction);
    }

    private void RegenerateWorldAltitudeScaleChange_Internal(float value)
    {
        Manager.BlockUndoAndRedo(true);

        GuiManager.RegisterRegenerateWorldPostProgressOp(OnRegenCompletion);

        RegenerateWorldAltitudeScaleChangeEvent.Invoke(value);
    }

    public void RegenerateWorldAltitudeScaleChange(float value)
    {
        PerformRegenerateWorldAction(
            RegenerateWorldAltitudeScaleChange_Internal,
            Manager.AltitudeScale,
            value);
    }

    private void RegenerateWorldSeaLevelOffsetChange_Internal(float value)
    {
        Manager.BlockUndoAndRedo(true);

        GuiManager.RegisterRegenerateWorldPostProgressOp(OnRegenCompletion);

        RegenerateWorldSeaLevelOffsetChangeEvent.Invoke(value);
    }

    public void RegenerateWorldSeaLevelOffsetChange(float value)
    {
        PerformRegenerateWorldAction(
            RegenerateWorldSeaLevelOffsetChange_Internal,
            Manager.SeaLevelOffset,
            value);
    }

    private void RegenerateWorldTemperatureOffsetChange_Internal(float value)
    {
        Manager.BlockUndoAndRedo(true);

        GuiManager.RegisterRegenerateWorldPostProgressOp(OnRegenCompletion);

        RegenerateWorldTemperatureOffsetChangeEvent.Invoke(value);
    }

    public void RegenerateWorldTemperatureOffsetChange(float value)
    {
        PerformRegenerateWorldAction(
            RegenerateWorldTemperatureOffsetChange_Internal,
            Manager.TemperatureOffset,
            value);
    }

    private void RegenerateWorldRainfallOffsetChange_Internal(float value)
    {
        Manager.BlockUndoAndRedo(true);

        GuiManager.RegisterRegenerateWorldPostProgressOp(OnRegenCompletion);

        RegenerateWorldRainfallOffsetChangeEvent.Invoke(value);
    }

    public void RegenerateWorldRainfallOffsetChange(float value)
    {
        PerformRegenerateWorldAction(
            RegenerateWorldRainfallOffsetChange_Internal,
            Manager.RainfallOffset,
            value);
    }

    private void OnUndoStackUpdate()
    {
        UndoActionButton.interactable = Manager.UndoableEditorActionsCount > 0;
    }

    private void OnRedoStackUpdate()
    {
        RedoActionButton.interactable = Manager.RedoableEditorActionsCount > 0;
    }

    public void OnSwitchingToGlobeViewing(bool state)
    {
        if (Manager.GameMode != GameMode.Editor)
            return;

        gameObject.SetActive(!state);
    }
}
