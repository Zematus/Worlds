using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class MapScript : MonoBehaviour
{
    public RawImage MapImage;
    public RawImage MapOverlayImage;
    public RawImage MapActivityImage;
    public RawImage PointerOverlayImage;
    public GameObject InfoPanel;

    public ShaderSettingsScript ShaderSettings;

    public Material DefaultMaterial;
    public Material DrainageMaterial;

    public UnityEvent ActivatedBrush;

    private bool _isDraggingMap = false;
    private Vector2 _beginDragPosition;
    private Rect _beginDragMapUvRect;

    private const float _maxZoomFactor = 1;
    private const float _minZoomFactor = 0.1f;
    private const float _zoomDeltaFactor = 0.05f;

    private float _zoomFactor = 1.0f;

    void Start()
    {
        // Prevent material (asset) from being overwritten
        MapImage.material = new Material(MapImage.material); //TODO: Doesn't work, but only a problem in Unity3D Editor
    }

    // Update is called once per frame
    void Update()
    {
        if (Manager.ViewingGlobe)
            return;

        ReadKeyboardInput();
    }

    private void ReadKeyboardInput()
    {
        ReadKeyboardInput_Zoom();
    }

    private void ZoomKeyIsPressed()
    {
        ZoomKeyPressed(true);
    }

    private void ZoomKeyIsNotPressed()
    {
        ZoomKeyPressed(false);
    }

    private void ReadKeyboardInput_Zoom()
    {
        Manager.HandleKey(KeyCode.KeypadPlus, false, false, ZoomKeyIsPressed);
        Manager.HandleKey(KeyCode.Equals, false, false, ZoomKeyIsPressed);

        Manager.HandleKey(KeyCode.KeypadMinus, false, false, ZoomKeyIsNotPressed);
        Manager.HandleKey(KeyCode.Minus, false, false, ZoomKeyIsNotPressed);
    }

    public void SetVisible(bool state)
    {
        MapImage.enabled = state;
        MapOverlayImage.enabled = state;
        MapActivityImage.enabled = state;
        PointerOverlayImage.enabled = state;
    }

    public void RefreshTexture()
    {
        MapImage.texture = Manager.CurrentMapTexture;
        MapOverlayImage.texture = Manager.CurrentMapOverlayTexture;
        MapActivityImage.texture = Manager.CurrentMapActivityTexture;

        if ((Manager.PlanetOverlay == PlanetOverlay.None) ||
            (Manager.PlanetOverlay == PlanetOverlay.General))
        {
            MapImage.material.SetColor("_Color", ShaderSettings.DefaultColor);
            MapImage.material.SetFloat("_EffectAmount", ShaderSettings.DefaultGrayness);
        }
        else
        {
            MapImage.material.SetColor("_Color", ShaderSettings.SubduedColor);
            MapImage.material.SetFloat("_EffectAmount", ShaderSettings.SubduedGrayness);
        }

        if (Manager.AnimationShadersEnabled && (Manager.PlanetOverlay == PlanetOverlay.DrainageBasins))
        {
            MapOverlayImage.material = DrainageMaterial;
            MapOverlayImage.material.SetTexture("_LengthTex", Manager.CurrentMapOverlayShaderInfoTexture);
        }
        else
        {
            MapOverlayImage.material = DefaultMaterial;
        }
    }

    public void RenderToTexture2D(Texture2D targetTexture)
    {
        Texture mapTexture = MapImage.texture;
        Texture overlayTexture = MapOverlayImage.texture;

        Rect uvRect = MapImage.uvRect;

        RenderTexture renderTexture =
            RenderTexture.GetTemporary(mapTexture.width, mapTexture.height);

        // Material blit pass

        Graphics.Blit(mapTexture, renderTexture, MapImage.material);
        Graphics.Blit(overlayTexture, renderTexture, DefaultMaterial);

        targetTexture.ReadPixels(
            new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        targetTexture.Apply();

        // scale and offset blit pass

        Graphics.Blit(
            targetTexture,
            renderTexture,
            new Vector2(uvRect.width, uvRect.height),
            new Vector2(uvRect.x, uvRect.y));

        targetTexture.ReadPixels(
            new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        targetTexture.Apply();

        RenderTexture.ReleaseTemporary(renderTexture);
    }

    public void EnablePointerOverlay(bool state)
    {
        PointerOverlayImage.enabled = state;
    }

    public void RefreshPointerOverlayTexture()
    {
        PointerOverlayImage.texture = Manager.PointerOverlayTexture;
    }

    public void PointerEntersMap()
    {
        Manager.PointerIsOverMap = true;
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
        MapOverlayImage.uvRect = newUvRect;
        MapActivityImage.uvRect = newUvRect;
        PointerOverlayImage.uvRect = newUvRect;
    }

    private void BeginDragMap(PointerEventData pointerData)
    {
        if (Manager.EditorBrushIsActive)
            return;

        _beginDragPosition = pointerData.position;
        _beginDragMapUvRect = MapImage.uvRect;

        _isDraggingMap = true;
    }

    public void EndDragMap(PointerEventData pointerData)
    {
        if (!_isDraggingMap)
            return;

        _isDraggingMap = false;
    }

    public void BeginDrag(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;

        if (pointerData.button == PointerEventData.InputButton.Right)
        {
            BeginDragMap(pointerData);
        }
    }

    public void Drag(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;

        if (pointerData.button == PointerEventData.InputButton.Right)
        {
            DragMap(pointerData);
        }
    }

    public void EndDrag(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;

        if (pointerData.button == PointerEventData.InputButton.Right)
        {
            EndDragMap(pointerData);
        }
    }

    /// <summary>
    /// Handler for pointer down events within the map.
    /// </summary>
    /// <param name="data">
    /// Mouse data related to the event.
    /// </param>
    public void PointerDown(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;

        if (pointerData.button == PointerEventData.InputButton.Left)
        {
            if (!Manager.ViewingGlobe &&
                !_isDraggingMap &&
                (Manager.GameMode == GameMode.Editor) &&
                Manager.CanActivateBrush())
            {
                Manager.ActivateEditorBrush(true);
                ActivatedBrush.Invoke();
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
        if (Manager.ViewingGlobe)
            return;

        Vector2 pointerPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        Vector2 uvPosition;

        if (!GetUvCoordinatesFromPointerPosition(pointerPosition, out uvPosition))
        {
            uvPosition = MapImage.uvRect.center;
        }

        float zoomDelta = 2f * (state ? _zoomDeltaFactor : -_zoomDeltaFactor);

        ZoomMap(zoomDelta, uvPosition);
    }

    private void ZoomKeyPressed(bool state)
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

    public void ShiftSurfaceToPosition(WorldPosition mapPosition)
    {
        Rect mapImageRect = MapImage.rectTransform.rect;

        Vector2 uvPos = Manager.GetUVFromMapCoordinates(mapPosition);

        Vector2 mapImagePos = uvPos - MapImage.uvRect.center;
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

        Vector2 uvPos = Manager.GetUVFromMapCoordinates(mapPosition);

        Vector2 mapImagePos = uvPos - MapImage.uvRect.min;
        mapImagePos.x = Mathf.Repeat(mapImagePos.x, 1.0f);
        mapImagePos /= _zoomFactor;

        mapImagePos.Scale(mapImageRect.size);

        return MapImage.rectTransform.TransformPoint(mapImagePos + mapImageRect.min);
    }
}
