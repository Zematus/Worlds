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

    // Update is called once per frame
    void Update()
    {
        ReadKeyboardInput();
    }

    public void ReadKeyboardInput()
    {
        ReadKeyboardInput_Zoom();
    }

    public void ReadKeyboardInput_Zoom()
    {
        if (Input.GetKey(KeyCode.KeypadPlus) ||
            Input.GetKey(KeyCode.Equals))
        {
            ZoomKeyPressed(true);
        }
        else if (Input.GetKey(KeyCode.KeypadMinus) ||
            Input.GetKey(KeyCode.Minus))
        {
            ZoomKeyPressed(false);
        }
    }

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
        if (!_isDraggingMap)
            return;

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
        if (Manager.EditorBrushIsActive)
            return;

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
            if (!_isDraggingMap)
            {
                Manager.ActivateEditorBrush(true);
            }
        }
    }

    public void PointerUp(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;

        if (pointerData.button == PointerEventData.InputButton.Left)
        {
            Manager.ActivateEditorBrush(false);
        }
    }

    public void Scroll(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;
        
        Vector2 uvPosition;

        if (!GetUvCoordinatesFromPointerPosition(pointerData.position, out uvPosition))
        {
            uvPosition = MapImage.uvRect.center;
        }

        ZoomMap(_zoomDeltaFactor * pointerData.scrollDelta.y, uvPosition);
    }

    public void ZoomButtonPressed(bool state)
    {
        Vector2 pointerPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        Vector2 uvPosition;

        if (!GetUvCoordinatesFromPointerPosition(pointerPosition, out uvPosition))
        {
            uvPosition = MapImage.uvRect.center;
        }

        float zoomDelta = 2f * (state ? _zoomDeltaFactor : -_zoomDeltaFactor);

        ZoomMap(zoomDelta, uvPosition);
    }

    public void ZoomKeyPressed(bool state)
    {
        Vector2 pointerPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        Vector2 uvPosition;

        if (!GetUvCoordinatesFromPointerPosition(pointerPosition, out uvPosition))
        {
            uvPosition = MapImage.uvRect.center;
        }

        float zoomDelta = 0.25f * (state ? _zoomDeltaFactor : -_zoomDeltaFactor);

        ZoomMap(zoomDelta, uvPosition);
    }

    public bool GetUvCoordinatesFromPointerPosition(Vector2 pointerPosition, out Vector2 uvPosition, bool allowWrap = false)
    {
        Rect mapImageRect = MapImage.rectTransform.rect;

        Vector3 positionOverMapRect3D = MapImage.rectTransform.InverseTransformPoint(pointerPosition);
        Vector2 positionOverMapRect = new Vector2(positionOverMapRect3D.x, positionOverMapRect3D.y);

        if (allowWrap || mapImageRect.Contains(positionOverMapRect))
        {
            Vector2 relPos = positionOverMapRect - mapImageRect.min;

            Vector2 normPos = new Vector2(relPos.x / mapImageRect.size.x, relPos.y / mapImageRect.size.y);

            uvPosition = (_zoomFactor * normPos) + MapImage.uvRect.min;

            return true;
        }

        uvPosition = -Vector2.one;

        return false;
    }

    public bool GetMapCoordinatesFromPointerPosition(Vector2 pointerPosition, out Vector2 mapPosition, bool allowWrap = false)
    {
        Vector2 uvPosition;

        if (!GetUvCoordinatesFromPointerPosition(pointerPosition, out uvPosition, allowWrap))
        {
            mapPosition = -Vector2.one;

            return false;
        }

        float worldLong = Mathf.Repeat(Mathf.Floor(uvPosition.x * Manager.CurrentWorld.Width), Manager.CurrentWorld.Width);
        float worldLat = Mathf.Floor(uvPosition.y * Manager.CurrentWorld.Height);

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

    public void ZoomMap(float delta, Vector2 uvRefPosition)
    {
        if (_isDraggingMap || Manager.EditorBrushIsActive)
            return;

        float oldZoomFactor = _zoomFactor;
        _zoomFactor = Mathf.Clamp(_zoomFactor - delta, _minZoomFactor, _maxZoomFactor);

#if DEBUG
        if (_zoomFactor <= 0)
            throw new System.Exception("invalid _zoomFactor: " + _zoomFactor);
#endif

        float maxUvY = 1f - _zoomFactor;

        Rect newUvRect = MapImage.uvRect;
        
        float zoomDeltaFactor = _zoomFactor / oldZoomFactor;
        Vector2 refDelta = (uvRefPosition - newUvRect.min) * zoomDeltaFactor;
        Vector2 newUvMin = uvRefPosition - refDelta;
        
        newUvRect.x = newUvMin.x;
        newUvRect.y = Mathf.Clamp(newUvMin.y, 0, maxUvY);

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

        Vector2 normalizedMapPos = GetNormalizedPositionFromMapCoordinates(mapPosition);

        Vector2 mapImagePos = normalizedMapPos - MapImage.uvRect.min;
        mapImagePos.x = Mathf.Repeat(mapImagePos.x, 1.0f);
        mapImagePos /= _zoomFactor;

        mapImagePos.Scale(mapImageRect.size);

        return MapImage.rectTransform.TransformPoint(mapImagePos + mapImageRect.min);
    }

    public Vector2 GetNormalizedPositionFromMapCoordinates(WorldPosition mapPosition)
    {
        return new Vector2(mapPosition.Longitude / (float)Manager.CurrentWorld.Width, mapPosition.Latitude / (float)Manager.CurrentWorld.Height);
    }
}
