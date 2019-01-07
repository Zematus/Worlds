using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class MapScript : MonoBehaviour
{
    public RawImage Image;
    public RawImage CursorOverlayImage;
    public GameObject InfoPanel;

    public void SetVisible(bool value)
    {
        Image.enabled = value;
        CursorOverlayImage.enabled = value;

        InfoPanel.SetActive(value);
    }

    public bool IsVisible()
    {
        return Image.enabled;
    }

    public void RefreshTexture()
    {
        Image.texture = Manager.CurrentMapTexture;
    }

    public void RefreshCursorOverlayTexture()
    {
        CursorOverlayImage.texture = Manager.CursorOverlayTexture;
    }
}
