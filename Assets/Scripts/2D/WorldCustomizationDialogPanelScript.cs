using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class WorldCustomizationDialogPanelScript : DialogPanelScript
{
    public InputField SeedInputField;

    public Button GenerateButton;

    public Image HeightmapImage;
    public Text InvalidImageText;

    public InputField SelectedFilenameField;

    public InputField TemperatureInputField;
    public Slider TemperatureSlider;
    public float TemperatureOffset = 0;

    public InputField RainfallInputField;
    public Slider RainfallSlider;
    public float RainfallOffset = 0;

    public InputField SeaLevelInputField;
    public Slider SeaLevelSlider;
    public float SeaLevelOffset = 0;

    private bool _hasLoadedValidHeightmap = false;

    private float _minTemperatureOffset = -50 + World.AvgPossibleTemperature;
    private float _maxTemperatureOffset = 50 + World.AvgPossibleTemperature;

    private float _minRainfallOffset = -800f + World.AvgPossibleRainfall;
    private float _maxRainfallOffset = 800f + World.AvgPossibleRainfall;

    private float _minSeaLevelOffset = -10000;
    private float _maxSeaLevelOffset = 10000;

    void Awake()
    {
        if (TemperatureSlider != null)
        {
            TemperatureSlider.minValue = _minTemperatureOffset;
            TemperatureSlider.maxValue = _maxTemperatureOffset;
        }

        if (RainfallSlider != null)
        {
            RainfallSlider.minValue = _minRainfallOffset;
            RainfallSlider.maxValue = _maxRainfallOffset;
        }

        if (SeaLevelSlider != null)
        {
            SeaLevelSlider.minValue = _minSeaLevelOffset;
            SeaLevelSlider.maxValue = _maxSeaLevelOffset;
        }
    }

    public void SetSeedString(string seedStr)
    {
        SeedInputField.text = seedStr;
    }

    private void SetTemperatureFieldValue(float offset)
    {
        float avgTemp = offset;
        TemperatureInputField.text = avgTemp.ToString("0.00");
    }

    private bool GetTemperatureFieldValue(out float offset)
    {
        float value;

        if (!float.TryParse(TemperatureInputField.text, out value))
        {
            offset = 0;

            return false;
        }

        offset = value;

        return true;
    }

    public void SetTemperatureOffset(float offset)
    {
        TemperatureOffset = offset;
        TemperatureSlider.value = offset;

        SetTemperatureFieldValue(offset);
    }

    private void SetRainfallFieldValue(float offset)
    {
        float avgRainfall = offset;
        RainfallInputField.text = avgRainfall.ToString();
    }

    private bool GetRainfallFieldValue(out float offset)
    {
        float value;

        if (!float.TryParse(RainfallInputField.text, out value))
        {

            offset = 0;

            return false;
        }

        offset = value;

        return true;
    }

    public void SetRainfallOffset(float offset)
    {
        RainfallOffset = offset;
        RainfallSlider.value = offset;

        SetRainfallFieldValue(offset);
    }

    private void SetAltitudeFieldValue(float offset)
    {
        SeaLevelInputField.text = offset.ToString();
    }

    private bool GetAltitudeFieldValue(out float offset)
    {
        float value;

        if (!float.TryParse(SeaLevelInputField.text, out value))
        {
            offset = 0;

            return false;
        }

        offset = value;

        return true;
    }

    public void SetSeaLevelOffset(float offset)
    {
        SeaLevelOffset = offset;
        SeaLevelInputField.text = offset.ToString();

        SetAltitudeFieldValue(offset);
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

    public void TemperatureValueChange(bool fromField)
    {
        float value = 0;

        if (fromField)
        {
            if (!GetTemperatureFieldValue(out value))
            {
                return;
            }
        }
        else
        {
            value = TemperatureSlider.value;
        }

        SetTemperatureValue(value);
    }

    public void ResetTemperatureValue()
    {
        SetTemperatureValue(World.AvgPossibleTemperature);
    }

    private void SetTemperatureValue(float value)
    {
        value = Mathf.Clamp(value, _minTemperatureOffset, _maxTemperatureOffset);

        TemperatureSlider.value = value;
        TemperatureOffset = value;

        SetTemperatureFieldValue(value);
    }

    public void RainfallValueChange(bool fromField)
    {
        float value = 0;

        if (fromField)
        {
            if (!GetRainfallFieldValue(out value))
            {
                return;
            }
        }
        else
        {
            value = RainfallSlider.value;
        }

        SetRainfallValue(value);
    }

    public void ResetRainfallValue()
    {
        SetRainfallValue(World.AvgPossibleRainfall);
    }

    private void SetRainfallValue(float value)
    {
        value = Mathf.Clamp(value, _minRainfallOffset, _maxRainfallOffset);

        RainfallSlider.value = value;
        RainfallOffset = value;

        SetRainfallFieldValue(value);
    }

    public void SeaLevelValueChange(bool fromField)
    {
        float value = 0;

        if (fromField)
        {
            if (!GetAltitudeFieldValue(out value))
            {
                return;
            }
        }
        else
        {
            value = SeaLevelSlider.value;
        }

        SetSeaLevelValue(value);
    }

    public void ResetSeaLevelValue()
    {
        SetSeaLevelValue(0);
    }

    private void SetSeaLevelValue(float value)
    {
        value = Mathf.Clamp(value, _minSeaLevelOffset, _maxSeaLevelOffset);

        SeaLevelSlider.value = value;
        SeaLevelOffset = value;

        SetAltitudeFieldValue(value);
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

    public void SetImageTexture(string filename, Texture2D texture)
    {
        SelectedFilenameField.text = filename;

        _hasLoadedValidHeightmap = texture != null;

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
