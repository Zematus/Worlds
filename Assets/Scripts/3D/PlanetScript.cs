using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public enum SphereRotationType
{
    None,
    Auto,
    AutoCameraFollow
}

public enum SphereLightingType
{
    SunLight,
    CameraLight
}

public class PlanetScript : MonoBehaviour
{
    public Camera Camera;

    public GameObject SunLight;
    public GameObject FocusLight;

    public GameObject SunReference;

    public GameObject Pivot;
    public GameObject InnerPivot;

    public GameObject AutoRotationPivot;
    public GameObject Surface;

    public ToggleEvent LightingTypeChangeEvent;

    private bool _isDraggingSurface = false;

    private Vector3 _lastDragMousePosition;

    private const float _maxCameraDistance = -2.5f;
    private const float _minCameraDistance = -1.15f;

    private const float _maxZoomFactor = 1f;
    private const float _minZoomFactor = 0f;

    private const float _maxZoomDragFactor = 1f;
    private const float _minZoomDragFactor = 0.1f;

    private const float _zoomDeltaFactor = 0.05f;

    private float _zoomFactor = 1.0f;

    private SphereRotationType _rotationType = SphereRotationType.Auto;
    private SphereLightingType _lightingType = SphereLightingType.SunLight;

    // Update is called once per frame

    void Update()
    {
        if (!Manager.ViewingGlobe)
            return;

        ReadKeyboardInput();

        if ((_rotationType == SphereRotationType.Auto) ||
            (_rotationType == SphereRotationType.AutoCameraFollow))
        {
            AutoRotationPivot.transform.Rotate(Vector3.up * Time.deltaTime * -0.5f);
        }
    }

    private void ReadKeyboardInput()
    {
        ReadKeyboardInput_Rotation();
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

    private void ReadKeyboardInput_Rotation()
    {
        Manager.HandleKeyUp(KeyCode.R, false, false, ToggleRotationType);
        Manager.HandleKeyUp(KeyCode.L, false, false, ToggleLightingType);
    }

    public void ToggleRotationType()
    {
        switch (_rotationType)
        {
            case SphereRotationType.Auto:
                SetRotationType(SphereRotationType.AutoCameraFollow);
                break;
            case SphereRotationType.AutoCameraFollow:
                SetRotationType(SphereRotationType.Auto);
                break;
            case SphereRotationType.None:
                break;
            default:
                throw new System.Exception("Unhandled SphereRotationType: " + _rotationType);
        }
    }

    private void SetRotationType(SphereRotationType rotationType)
    {
        _rotationType = rotationType;
        
        if (_rotationType == SphereRotationType.AutoCameraFollow)
        {
            Pivot.transform.SetParent(AutoRotationPivot.transform);
        }
        else
        {
            Pivot.transform.SetParent(transform);
        }
    }

    public void ToggleLightingType()
    {
        switch (_lightingType)
        {
            case SphereLightingType.SunLight:
                SetLightingType(SphereLightingType.CameraLight);
                break;
            case SphereLightingType.CameraLight:
                SetLightingType(SphereLightingType.SunLight);
                break;
            default:
                throw new System.Exception("Unhandled SphereLightingType: " + _lightingType);
        }
    }

    private void SetLightingType(SphereLightingType lightingType)
    {
        _lightingType = lightingType;
        
        SunLight.SetActive(_lightingType == SphereLightingType.SunLight);
        FocusLight.SetActive(_lightingType == SphereLightingType.CameraLight);

        LightingTypeChangeEvent.Invoke(_lightingType == SphereLightingType.SunLight);
    }

    public void SetVisible(bool state)
    {
        Surface.SetActive(state);
    }

    public void RefreshTexture()
    {
        Material[] materials = Surface.GetComponent<Renderer>().materials;

        materials[1].mainTexture = Manager.CurrentMapTexture;
        materials[0].mainTexture = Manager.CurrentMapOverlayTexture;
    }

    public void ZoomButtonPressed(bool state)
    {
        if (!Manager.ViewingGlobe)
            return;

        float zoomDelta = 2f * (state ? _zoomDeltaFactor : -_zoomDeltaFactor);

        ZoomCamera(zoomDelta);
    }

    private void ZoomKeyPressed(bool state)
    {
        float zoomDelta = 0.25f * (state ? _zoomDeltaFactor : -_zoomDeltaFactor);

        ZoomCamera(zoomDelta);
    }

    public void ZoomCamera(float delta)
    {
        if (_isDraggingSurface)
            return;
        
        _zoomFactor = Mathf.Clamp(_zoomFactor - delta, _minZoomFactor, _maxZoomFactor);

        Vector3 cameraPosition = Camera.transform.localPosition;
        cameraPosition.z = Mathf.Lerp(_minCameraDistance, _maxCameraDistance, _zoomFactor);

        Camera.transform.localPosition = cameraPosition;
    }

    private void ValidateInnerPivotRotation()
    {
        Quaternion innerPivotRotation = InnerPivot.transform.localRotation;
        Vector3 innerPivotEulerAngles = innerPivotRotation.eulerAngles;

        // Prevent the globe's inner pivot from rotating beyond the poles
        if ((innerPivotEulerAngles.x > 88) && (innerPivotEulerAngles.x <= 135))
        {
            innerPivotEulerAngles.x = 88;
        }
        if ((innerPivotEulerAngles.x < 272) && (innerPivotEulerAngles.x > 135))
        {
            innerPivotEulerAngles.x = 272;
        }
        innerPivotEulerAngles.y = 0;
        innerPivotEulerAngles.z = 0;

        innerPivotRotation.eulerAngles = innerPivotEulerAngles;
        InnerPivot.transform.localRotation = innerPivotRotation;
    }

    private void RotateOuterPivot(float horizontalRotation)
    {
        Pivot.transform.Rotate(0, horizontalRotation, 0);
    }

    private void RotateInnerPivot(float verticalRotation)
    {
        InnerPivot.transform.Rotate(verticalRotation, 0, 0, Space.Self);

        ValidateInnerPivotRotation();
    }

    private void RotateSurface(float horizontalRotation, float verticalRotation)
    {
        RotateOuterPivot(horizontalRotation);
        RotateInnerPivot(verticalRotation);
    }

    private void DragSurface(Vector3 mousePosition)
    {
        if (!_isDraggingSurface)
            return;

        float zoomDragFactor = Mathf.Lerp(_minZoomDragFactor, _maxZoomDragFactor, _zoomFactor);
        
        float screenFactor =  110f / Mathf.Min(Screen.height, Screen.width);

        float lastOffsetX = _lastDragMousePosition.x - Screen.height / 2;
        float lastOffsetY = _lastDragMousePosition.y - Screen.width / 2;

        float offsetX = mousePosition.x - Screen.height / 2;
        float offsetY = mousePosition.y - Screen.width / 2;

        float innerPivotRotX = (offsetY - lastOffsetY) * screenFactor * zoomDragFactor;
        float pivotRotY = (offsetX - lastOffsetX) * screenFactor * zoomDragFactor;

        RotateSurface(pivotRotY, -innerPivotRotX);

        _lastDragMousePosition = mousePosition;
    }

    private void BeginDragSurface(Vector3 mousePosition)
    {
        _lastDragMousePosition = mousePosition;
        
        _isDraggingSurface = true;
    }

    public void EndDragSurface(Vector3 mousePosition)
    {
        if (!_isDraggingSurface)
            return;

        _isDraggingSurface = false;
    }

    public bool GetUvCoordinatesFromPointerPosition(Vector2 pointerPosition, out Vector2 uvPosition)
    {
        Ray ray = Camera.ScreenPointToRay(Input.mousePosition);

        RaycastHit raycastHit;

        Collider collider = Surface.GetComponent<Collider>();

        if (!collider.Raycast(ray, out raycastHit, 50))
        {
            uvPosition = -Vector2.one;

            return false;
        }

        uvPosition = raycastHit.textureCoord;

        return true;
    }

    public bool GetMapCoordinatesFromPointerPosition(Vector2 pointerPosition, out Vector2 mapPosition)
    {
        Vector2 uvPosition;

        if (!GetUvCoordinatesFromPointerPosition(pointerPosition, out uvPosition))
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

    private Vector3 GetWorldPositionFromMapCoordinates(WorldPosition mapPosition)
    {
        Vector2 uvPos = Manager.GetUVFromMapCoordinates(mapPosition);

        Vector3 closestVertex = Vector3.zero;
        float closestDistance = float.MaxValue;

        Vector2[] uvOffsets = Surface.GetComponent<MeshFilter>().mesh.uv;
        Vector3[] vertices = Surface.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector2 uvOffset = uvOffsets[i];
            Vector3 vertex = vertices[i];

            float distance = (uvOffset - uvPos).magnitude;

            if (distance < closestDistance)
            {
                closestVertex = vertex;
                closestDistance = distance;
            }
        }
        
        return Surface.transform.localToWorldMatrix.MultiplyPoint3x4(closestVertex);
    }

    public Vector3 GetScreenPositionFromMapCoordinates(WorldPosition mapPosition)
    {
        Vector3 worldPosition = GetWorldPositionFromMapCoordinates(mapPosition);

        return Camera.WorldToScreenPoint(worldPosition);
    }

    public void ShiftSurfaceToPosition(WorldPosition mapPosition)
    {
        // First, we horizontally rotate the outer pivot

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        Vector3 worldPosScreenCenter = Camera.ScreenToWorldPoint(screenCenter);
        Vector3 pivotPosScreenCenter = Pivot.transform.worldToLocalMatrix.MultiplyPoint3x4(worldPosScreenCenter).normalized;
        pivotPosScreenCenter.y = 0; // we project the vector over the globes's equator

        Vector3 worldPosition = GetWorldPositionFromMapCoordinates(mapPosition);
        Vector3 pivotPosition = Pivot.transform.worldToLocalMatrix.MultiplyPoint3x4(worldPosition).normalized;
        pivotPosition.y = 0; // we project the vector over the globes's equator

        Vector3 eulerAnglesHorizontal = Quaternion.FromToRotation(pivotPosScreenCenter, pivotPosition).eulerAngles;

        RotateOuterPivot(eulerAnglesHorizontal.y);

        // Second, we vertically rotate the inner pivot

        worldPosScreenCenter = Camera.ScreenToWorldPoint(screenCenter); // the screen position in the scene has to be recalculated
        Vector3 innerPivotPosScreenCenter = InnerPivot.transform.worldToLocalMatrix.MultiplyPoint3x4(worldPosScreenCenter).normalized;

        worldPosition = GetWorldPositionFromMapCoordinates(mapPosition); // so do we need a new scene position for the surface map position
        Vector3 innerPivotPosition = InnerPivot.transform.worldToLocalMatrix.MultiplyPoint3x4(worldPosition).normalized;
        
        Vector3 eulerAnglesVertical = Quaternion.FromToRotation(innerPivotPosScreenCenter, innerPivotPosition).eulerAngles;

        RotateInnerPivot(eulerAnglesVertical.x);

        SetRotationType(SphereRotationType.AutoCameraFollow);
        SetLightingType(SphereLightingType.CameraLight);
    }

    public void BeginDrag(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;

        if (pointerData.button == PointerEventData.InputButton.Right)
        {
            BeginDragSurface(pointerData.position);
        }
    }

    public void Drag(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;

        if (pointerData.button == PointerEventData.InputButton.Right)
        {
            DragSurface(pointerData.position);
        }
    }

    public void EndDrag(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;

        if (pointerData.button == PointerEventData.InputButton.Right)
        {
            EndDragSurface(pointerData.position);
        }
    }

    public void Scroll(BaseEventData data)
    {
        PointerEventData pointerData = data as PointerEventData;

        ZoomCamera(_zoomDeltaFactor * pointerData.scrollDelta.y);
    }
}
