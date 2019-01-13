using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class MapScript : MonoBehaviour
{
    public RawImage Image;
    public RawImage PointerOverlayImage;
    public GameObject InfoPanel;

    public void SetVisible(bool value)
    {
        Image.enabled = value;
        PointerOverlayImage.enabled = value;

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

    public void RefreshPointerOverlayTexture()
    {
        PointerOverlayImage.texture = Manager.PointerOverlayTexture;
    }
}
