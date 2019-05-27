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
    public Toggle Toggle8;

    public Button UndoActionButton;
    public Button RedoActionButton;

    public ValueSetEvent RegenerateWorldAltitudeScaleChangeEvent;
    public ValueSetEvent RegenerateWorldSeaLevelOffsetChangeEvent;
    public ValueSetEvent RegenerateWorldTemperatureOffsetChangeEvent;
    public ValueSetEvent RegenerateWorldRainfallOffsetChangeEvent;

    public List<Toggle> BrushToggles = new List<Toggle>();

    public List<Toggle> LayerToggles = new List<Toggle>();

    public BrushControlPanelScript LayerBrushControlPanelScript;

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

    private void ToggleTool1()
    {
        if (LayerToggles.Contains(Toggle1) && !Manager.LayersPresent)
            return;

        Toggle1.isOn = !Toggle1.isOn;
    }

    private void ToggleTool2()
    {
        if (LayerToggles.Contains(Toggle2) && !Manager.LayersPresent)
            return;

        Toggle2.isOn = !Toggle2.isOn;
    }

    private void ToggleTool3()
    {
        if (LayerToggles.Contains(Toggle3) && !Manager.LayersPresent)
            return;

        Toggle3.isOn = !Toggle3.isOn;
    }

    private void ToggleTool4()
    {
        if (LayerToggles.Contains(Toggle4) && !Manager.LayersPresent)
            return;

        Toggle4.isOn = !Toggle4.isOn;
    }

    private void ToggleTool5()
    {
        if (LayerToggles.Contains(Toggle5) && !Manager.LayersPresent)
            return;

        Toggle5.isOn = !Toggle5.isOn;
    }

    private void ToggleTool6()
    {
        if (LayerToggles.Contains(Toggle6) && !Manager.LayersPresent)
            return;

        Toggle6.isOn = !Toggle6.isOn;
    }

    private void ToggleTool7()
    {
        if (LayerToggles.Contains(Toggle7) && !Manager.LayersPresent)
            return;

        Toggle7.isOn = !Toggle7.isOn;
    }

    private void ToggleTool8()
    {
        if (LayerToggles.Contains(Toggle8) && !Manager.LayersPresent)
            return;

        Toggle8.isOn = !Toggle8.isOn;
    }

    public void OverlaySubtypeChanged()
    {
        if (Manager.EditorBrushType == EditorBrushType.Layer)
        {
            bool isLayerOverlaySubtype =
                (Manager.PlanetOverlay == PlanetOverlay.Layer) &&
                (Manager.PlanetOverlaySubtype != Manager.NoOverlaySubtype);

            LayerBrushControlPanelScript.ActivateControls(isLayerOverlaySubtype);

            string layerTypeName = "<toggle layer subtype to use>";

            if (isLayerOverlaySubtype)
            {
                layerTypeName = Layer.Layers[Manager.PlanetOverlaySubtype].Name.FirstLetterToUpper();
            }

            LayerBrushControlPanelScript.Name.text = "Layer Brush: <b>" + layerTypeName + "</b>";

            Manager.EditorBrushIsVisible = isLayerOverlaySubtype;
        }
    }

    public bool IsBrushToggleActive()
    {
        foreach (Toggle toggle in BrushToggles)
        {
            if (toggle.isOn)
                return true;
        }

        return false;
    }

    private void ReadKeyboardInput()
    {
        Manager.HandleKeyUp(KeyCode.Z, true, true, RedoEditorAction);
        Manager.HandleKeyUp(KeyCode.Z, true, false, UndoEditorAction);

        Manager.HandleKeyUp(KeyCode.Alpha1, false, false, ToggleTool1);
        Manager.HandleKeyUp(KeyCode.Alpha2, false, false, ToggleTool2);
        Manager.HandleKeyUp(KeyCode.Alpha3, false, false, ToggleTool3);
        Manager.HandleKeyUp(KeyCode.Alpha4, false, false, ToggleTool4);
        Manager.HandleKeyUp(KeyCode.Alpha5, false, false, ToggleTool5);
        Manager.HandleKeyUp(KeyCode.Alpha6, false, false, ToggleTool6);
        Manager.HandleKeyUp(KeyCode.Alpha7, false, false, ToggleTool7);
        Manager.HandleKeyUp(KeyCode.Alpha8, false, false, ToggleTool8);
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

    public void SetVisible(bool state)
    {
        if (Manager.ViewingGlobe)
            return;

        if (state)
        {
            foreach (Toggle toggle in LayerToggles)
            {
                toggle.isOn = toggle.isOn && Manager.LayersPresent;
                toggle.gameObject.SetActive(Manager.LayersPresent);
            }
        }

        gameObject.SetActive(state);
    }
}
