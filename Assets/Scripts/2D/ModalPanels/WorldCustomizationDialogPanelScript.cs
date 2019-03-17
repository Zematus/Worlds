using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class WorldCustomizationDialogPanelScript : MenuPanelScript
{
    public LoadFileDialogPanelScript LoadHeightmapDialogPanel;

    public InputField SeedInputField;

    public Button GenerateButton;

    public Image HeightmapImage;
    public Text InvalidImageText;

    public Toggle UseHeightmapToggle;

    public InputField SelectedFilenameField;

    private bool _hasLoadedValidHeightmap = false;

    protected override void ReadKeyboardInput()
    {
        if (LoadHeightmapDialogPanel.gameObject.activeInHierarchy)
            return; // Do not capture keyboard if the load file dialog is active

        base.ReadKeyboardInput();
    }

    public void SetSeedString(string seedStr)
    {
        SeedInputField.text = seedStr;
    }

    public string GetSeedString()
    {
        return SeedInputField.text;
    }

    public void SeedValueChange()
    {
        int value = 0;

        int.TryParse(SeedInputField.text, out value);

        SeedInputField.text = value.ToString();
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
}
