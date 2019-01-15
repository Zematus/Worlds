using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class MapScript : MonoBehaviour
{
    public RawImage MapImage;
    public RawImage PointerOverlayImage;
    public GameObject InfoPanel;

    private Vector2 _beginDragPosition;
    private Rect _beginDragMapUvRect;

    public void SetVisible(bool value)
    {
        MapImage.enabled = value;
        PointerOverlayImage.enabled = value;

        InfoPanel.SetActive(value);
    }

    public bool IsVisible()
    {
        return MapImage.enabled;
    }

    public void RefreshTexture()
    {
        MapImage.texture = Manager.CurrentMapTexture;
    }

    public void EnablePointerOverlay(bool state)
    {
        PointerOverlayImage.enabled = state;
    }

    public void RefreshPointerOverlayTexture()
    {
        PointerOverlayImage.texture = Manager.PointerOverlayTexture;
    }

    public void SetBrushRadius(float value)
    {
        Manager.EditorBrushRadius = (int)value;
    }

    public void PointerEntersMap(BaseEventData data)
    {
        Manager.PointerIsOverMap = true;
    }

    public void PointerExitsMap(BaseEventData data)
    {
        Manager.PointerIsOverMap = false;
    }

    private void DragMap(PointerEventData pointerData)
    {
        Rect mapImageRect = MapImage.rectTransform.rect;

        Vector2 delta = pointerData.position - _beginDragPosition;

        float uvDelta = delta.x / mapImageRect.width;

        Rect newUvRect = _beginDragMapUvRect;
        newUvRect.x -= uvDelta;

        MapImage.uvRect = newUvRect;
        PointerOverlayImage.uvRect = newUvRect;
    }

    public void Drag(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;

        if (pointerData.button == PointerEventData.InputButton.Right)
        {
            DragMap(pointerData);
        }
    }

    private void BeginDragMap(PointerEventData pointerData)
    {
        _beginDragPosition = pointerData.position;
        _beginDragMapUvRect = MapImage.uvRect;
    }

    public void BeginDrag(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;

        if (pointerData.button == PointerEventData.InputButton.Right)
        {
            BeginDragMap(pointerData);
        }
        else if (pointerData.button == PointerEventData.InputButton.Left)
        {
            Manager.EditorBrushIsActive = true;
        }
    }

    public void EndDrag(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;

        if (pointerData.button == PointerEventData.InputButton.Left)
        {
            Manager.EditorBrushIsActive = false;
        }
    }
}
