using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class MapScript : MonoBehaviour
{
    public RawImage Image;
    public GameObject InfoPanel;

    public void SetVisible(bool value)
    {
        Image.enabled = value;
        InfoPanel.SetActive(value);
    }

    public bool IsVisible()
    {
        return Image.enabled;
    }

    public void RefreshTexture()
    {
        Texture2D texture = Manager.CurrentMapTexture;

        Image.texture = texture;
    }
}
