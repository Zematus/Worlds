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

    private Stack<EditorAction> _undoableEditorActions = new Stack<EditorAction>();
    private Stack<EditorAction> _redoableEditorActions = new Stack<EditorAction>();

    private bool _blockUndoAndRedo = false;

    // Use this for initialization
    void Start()
    {
        GuiManager.RegisterGenerateWorldPostProgressOp(ResetActionStacks);
        GuiManager.RegisterLoadWorldPostProgressOp(ResetActionStacks);
    }

    // Update is called once per frame
    void Update()
    {
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
        else if (shiftPressed)
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
        if (_blockUndoAndRedo)
            return;

        if (_undoableEditorActions.Count <= 0)
            return;

        EditorAction action = PopUndoableAction();

        action.Undo();

        PushRedoableAction(action);
    }

    public void RedoEditorAction()
    {
        if (_blockUndoAndRedo)
            return;

        if (_redoableEditorActions.Count <= 0)
            return;

        EditorAction action = PopRedoableAction();

        action.Do();

        PushUndoableAction(action);
    }

    public void OnRegenCompletion()
    {
        _blockUndoAndRedo = false;
        GuiManager.DeregisterRegenerateWorldPostProgressOp(OnRegenCompletion);
    }

    public void RegenerateWorldAltitudeScaleChange_Internal(float value)
    {
        _blockUndoAndRedo = true;
        GuiManager.RegisterRegenerateWorldPostProgressOp(OnRegenCompletion);

        RegenerateWorldAltitudeScaleChangeEvent.Invoke(value);
    }

    public void RegenerateWorldAltitudeScaleChange(float value)
    {
        EditorAction action = new RenegerateWorldAction {
            Action = RegenerateWorldAltitudeScaleChange_Internal,
            PreviousValue = Manager.AltitudeScale,
            NewValue = value
        };

        action.Do();

        PushUndoableAction(action);
        ResetRedoableActionsStack();
    }

    public void ResetActionStacks()
    {
        ResetUndoableActionsStack();
        ResetRedoableActionsStack();
    }

    public void PushUndoableAction(EditorAction action)
    {
        _undoableEditorActions.Push(action);
        UndoActionButton.interactable = true;
    }

    public void PushRedoableAction(EditorAction action)
    {
        _redoableEditorActions.Push(action);
        RedoActionButton.interactable = true;
    }

    public EditorAction PopUndoableAction()
    {
        EditorAction action = _undoableEditorActions.Pop();

        if (_undoableEditorActions.Count <= 0)
            UndoActionButton.interactable = false;

        return action;
    }

    public EditorAction PopRedoableAction()
    {
        EditorAction action = _redoableEditorActions.Pop();

        if (_redoableEditorActions.Count <= 0)
            RedoActionButton.interactable = false;

        return action;
    }

    public void ResetUndoableActionsStack()
    {
        _undoableEditorActions.Clear();
        UndoActionButton.interactable = false;
    }

    public void ResetRedoableActionsStack()
    {
        _redoableEditorActions.Clear();
        RedoActionButton.interactable = false;
    }
}
