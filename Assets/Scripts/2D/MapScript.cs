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

    private bool _isDraggingMap = false;
    private Vector2 _beginDragPosition;
    private Rect _beginDragMapUvRect;

    private const float _maxZoomFactor = 1;
    private const float _minZoomFactor = 0.1f;
    private const float _zoomDeltaFactor = 0.05f;

    private float _zoomFactor = 1.0f;

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

        float maxUvY = 1 - _zoomFactor;

        float uvDeltaX = _zoomFactor * delta.x / mapImageRect.width;
        float uvDeltaY = _zoomFactor * delta.y / mapImageRect.height;

        Rect newUvRect = _beginDragMapUvRect;
        newUvRect.x -= uvDeltaX;
        newUvRect.y = Mathf.Clamp(newUvRect.y - uvDeltaY, 0, maxUvY);

        SetUvRect(newUvRect);
    }

    private void SetUvRect(Rect newUvRect)
    {
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

        _isDraggingMap = true;
    }

    public void BeginDrag(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;

        if (pointerData.button == PointerEventData.InputButton.Right)
        {
            BeginDragMap(pointerData);
        }
    }

    public void EndDrag(BaseEventData data)
    {
        _isDraggingMap = false;
    }

    public void PointerDown(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;

        if (pointerData.button == PointerEventData.InputButton.Left)
        {
            Manager.EditorBrushIsActive = true;
        }
    }

    public void PointerUp(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;

        if (pointerData.button == PointerEventData.InputButton.Left)
        {
            Manager.EditorBrushIsActive = false;
        }
    }

    public void Scroll(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;

        ZoomMap(_zoomDeltaFactor * pointerData.scrollDelta.y);
    }

    public bool GetMapCoordinatesFromPointerPosition(Vector2 pointerPosition, out Vector2 mapPosition, bool allowWrap = false)
    {
        Rect mapImageRect = MapImage.rectTransform.rect;

        Vector3 positionOverMapRect3D = MapImage.rectTransform.InverseTransformPoint(pointerPosition);

        Vector2 positionOverMapRect = new Vector2(positionOverMapRect3D.x, positionOverMapRect3D.y);

        if (mapImageRect.Contains(positionOverMapRect) || allowWrap)
        {
            Vector2 relPos = positionOverMapRect - mapImageRect.min;

            Vector2 uvPos = new Vector2(relPos.x / mapImageRect.size.x, relPos.y / mapImageRect.size.y);

            uvPos += MapImage.uvRect.min;

            float worldLong = Mathf.Repeat(Mathf.Floor(uvPos.x * Manager.CurrentWorld.Width), Manager.CurrentWorld.Width);
            float worldLat = Mathf.Floor(uvPos.y * Manager.CurrentWorld.Height);

            if (worldLat > (Manager.CurrentWorld.Height - 1))
            {
                worldLat = Mathf.Max(0, (2 * Manager.CurrentWorld.Height) - worldLat - 1);
                worldLong = Mathf.Repeat(Mathf.Floor(worldLong + (Manager.CurrentWorld.Width / 2f)), Manager.CurrentWorld.Width);
            }
            else if (worldLat < 0)
            {
                worldLat = Mathf.Min(Manager.CurrentWorld.Height - 1, -1 - worldLat);
                worldLong = Mathf.Repeat(Mathf.Floor(worldLong + (Manager.CurrentWorld.Width / 2f)), Manager.CurrentWorld.Width);
            }

            mapPosition = new Vector2(worldLong, worldLat);

            return true;
        }

        mapPosition = -Vector2.one;

        return false;
    }

    public void ZoomMap(float delta)
    {
        if (_isDraggingMap)
            return; // do not zoom in or out while dragging to avoid image displacement miscalculations.

        _zoomFactor = Mathf.Clamp(_zoomFactor - delta, _minZoomFactor, _maxZoomFactor);

#if DEBUG
        if (_zoomFactor <= 0)
            throw new System.Exception("invalid _zoomFactor: " + _zoomFactor);
#endif

        float maxUvY = 1f - _zoomFactor;

        Rect newUvRect = MapImage.uvRect;
        Vector2 uvCenter = newUvRect.center;
        newUvRect.x = uvCenter.x - _zoomFactor / 2f;
        newUvRect.y = Mathf.Clamp(uvCenter.y - _zoomFactor / 2f, 0, maxUvY);

        newUvRect.width = _zoomFactor;
        newUvRect.height = _zoomFactor;

        SetUvRect(newUvRect);
    }

    public void ShiftMapToPosition(WorldPosition mapPosition)
    {
        Rect mapImageRect = MapImage.rectTransform.rect;

        Vector2 normalizedMapPos = new Vector2(mapPosition.Longitude / (float)Manager.CurrentWorld.Width, mapPosition.Latitude / (float)Manager.CurrentWorld.Height);

        Vector2 mapImagePos = normalizedMapPos - MapImage.uvRect.center;
        mapImagePos.x = Mathf.Repeat(mapImagePos.x, 1.0f);

        float maxUvY = 1f - _zoomFactor;

        Rect newUvRect = MapImage.uvRect;
        newUvRect.x += mapImagePos.x;
        newUvRect.y = Mathf.Clamp(newUvRect.y + mapImagePos.y, 0, maxUvY);

        SetUvRect(newUvRect);
    }

    public Vector3 GetScreenPositionFromMapCoordinates(WorldPosition mapPosition)
    {
        Rect mapImageRect = MapImage.rectTransform.rect;

        Vector2 normalizedMapPos = new Vector2(mapPosition.Longitude / (float)Manager.CurrentWorld.Width, mapPosition.Latitude / (float)Manager.CurrentWorld.Height);

        Vector2 mapImagePos = normalizedMapPos - MapImage.uvRect.min;
        mapImagePos.x = Mathf.Repeat(mapImagePos.x, 1.0f);

        mapImagePos.Scale(mapImageRect.size);

        return MapImage.rectTransform.TransformPoint(mapImagePos + mapImageRect.min);
    }
}
