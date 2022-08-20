using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WorldCustomizationDialogPanelScript : MenuPanelScript
{
    public LoadFileDialogPanelScript LoadFileDialogPanel;

    public InputField SeedInputField;

    public Button GenerateButton;

    public Image HeightmapImage;
    public Text InvalidImageText;

    public Toggle UseHeightmapToggle;

    public InputField SelectedFilenameField;

    private bool _hasLoadedValidHeightmap = false;

    private int _previousInputValue = 0;

    protected override void ReadKeyboardInput()
    {
        if (LoadFileDialogPanel.gameObject.activeInHierarchy)
            return; // Do not capture keyboard if the load file dialog is active

        base.ReadKeyboardInput();
    }

    public void SetSeed(int seed)
    {
        _previousInputValue = seed;
        SeedInputField.text = _previousInputValue.ToString();
    }

    public int GetSeed()
    {
        return _previousInputValue;
    }

    public void SeedValueChange()
    {
        if (SeedInputField.text == string.Empty)
        {
            GenerateButton.interactable = false;
            return;
        }

        if (int.TryParse(SeedInputField.text, out int value))
        {
            _previousInputValue = value;
        }

        SeedInputField.text = _previousInputValue.ToString();
        GenerateButton.interactable = true;
    }

    public void SetImageLoadingPaneState(bool state)
    {
        if (state && !_hasLoadedValidHeightmap)
        {
            GenerateButton.interactable = false;
        }
        else
        {
            GenerateButton.interactable = true;
        }
    }

    public void SetImageTexture(string filename, Texture2D texture, TextureValidationResult result)
    {
        SelectedFilenameField.text = filename;
        
        _hasLoadedValidHeightmap = (result == TextureValidationResult.Ok) && (texture != null);

        switch (result)
        {
            case TextureValidationResult.Ok:
                break;
            case TextureValidationResult.NotMinimumRequiredDimensions:
                InvalidImageText.text = "Loaded image doesn't met minimum dimensions...";
                break;
            case TextureValidationResult.InvalidColorPallete:
                InvalidImageText.text = "Loaded image is not grayscale...";
                break;
            case TextureValidationResult.Unknown:
                InvalidImageText.text = "Unknown error trying to load image...";
                break;
            default:
                throw new System.Exception("Unhandled Texture Validation Result: " + result);
        }

        if (_hasLoadedValidHeightmap)
        {
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));

            HeightmapImage.sprite = sprite;
        }
        else
        {
            HeightmapImage.sprite = null;
        }

        InvalidImageText.gameObject.SetActive(!_hasLoadedValidHeightmap);
        GenerateButton.interactable = _hasLoadedValidHeightmap;
    }

    public void SetActiveMods()
    {
        SetVisible(false);

        LoadFileDialogPanel.Initialize(
            "Select Mod Folders to Use...",
            "Use",
            SetActiveModsAction,
            CancelSetActiveModsAction,
            Manager.DefaultModPath,
            loadDirectory: true,
            selectMultiple: true,
            prevSelectedItems: Manager.ActiveModPaths);

        LoadFileDialogPanel.SetVisible(true);
    }

    private void SetActiveModsAction()
    {
        Manager.SetActiveModPaths(LoadFileDialogPanel.GetPathsToLoad());
        Manager.ResetLayerSettings();

        SetVisible(true);
    }

    private void CancelSetActiveModsAction()
    {
        SetVisible(true);
    }
}
