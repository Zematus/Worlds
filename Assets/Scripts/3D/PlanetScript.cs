using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class PlanetScript : MonoBehaviour
{
    public Camera Camera;

    public GameObject SunLight;
    public GameObject FocusLight;

    public SphereCollider SphereCollider;

    public GameObject Pivot;
    public GameObject InnerPivot;

    public GameObject Surface;

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

    private bool _autoRotateSet = true;

    // Update is called once per frame

    void Update()
    {
        ReadKeyboardInput();

        Update_HandleMouse();

        if (_autoRotateSet)
        {
            Surface.transform.Rotate(Vector3.up * Time.deltaTime * -2.5f);
        }
    }

    private void ReadKeyboardInput()
    {
        ReadKeyboardInput_Rotation();
        ReadKeyboardInput_Zoom();
    }

    private void ReadKeyboardInput_Zoom()
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

    private void ReadKeyboardInput_Rotation()
    {
        bool controlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (controlPressed)
        {
            if (Input.GetKeyUp(KeyCode.R))
            {
                ToggleAutoRotate();
            }
        }
    }

    private void ToggleAutoRotate()
    {
        SetAutoRotate(!_autoRotateSet);
    }

    private void SetAutoRotate(bool state)
    {
        _autoRotateSet = state;

        SunLight.SetActive(state);
        FocusLight.SetActive(!state);
    }

    private void Update_HandleMouse()
    {
        if (_isDraggingSurface)
        {
            if (Input.GetMouseButton(1))
            {
                DragSurface(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(1))
            {
                EndDragSurface(Input.mousePosition);
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(1))
            {
                BeginDragSurface(Input.mousePosition);
            }

            HandleMouseScroll();
        }
    }

    public void RefreshTexture()
    {
        Texture2D texture = Manager.CurrentMapTexture;

        GetComponent<Renderer>().material.mainTexture = texture;
    }

    private void HandleMouseScroll()
    {
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

        float oldZoomFactor = _zoomFactor;
        _zoomFactor = Mathf.Clamp(_zoomFactor - delta, _minZoomFactor, _maxZoomFactor);

        Vector3 cameraPosition = Camera.transform.localPosition;
        cameraPosition.z = Mathf.Lerp(_minCameraDistance, _maxCameraDistance, _zoomFactor);

        Camera.transform.localPosition = cameraPosition;
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

        Pivot.transform.Rotate(0, pivotRotY, 0);
        InnerPivot.transform.Rotate(-innerPivotRotX, 0, 0, Space.Self);

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
}
