using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class PlanetScript : MonoBehaviour
{
    public Camera Camera;
    public SphereCollider SphereCollider;

    public GameObject Pivot;

    private bool _isDraggingSurface = false;

    private Vector3 _sphereRotateLastDragPosition;
    private Vector3 _initialDragMousePosition;

    private const float _rayDistFactor = 2.5f;

    // Update is called once per frame

    void Update()
    {
        Update_HandleMouse();

        //transform.Rotate(Vector3.up * Time.deltaTime * 10);
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
        }
    }

    public void RefreshTexture()
    {
        Texture2D texture = Manager.CurrentMapTexture;

        GetComponent<Renderer>().material.mainTexture = texture;
    }

    private void DragSurface(Vector3 mousePosition)
    {
        if (!_isDraggingSurface)
            return;

        Ray ray = Camera.ScreenPointToRay(Input.mousePosition);

        float distance = (Camera.transform.position - transform.position).magnitude * _rayDistFactor - Camera.nearClipPlane;

        Vector3 currentDragPosition = ray.GetPoint(distance);

        Vector3 pivotLastDragPosition = Pivot.transform.InverseTransformVector(_sphereRotateLastDragPosition);
        Vector3 pivotCurrentDragPosition = Pivot.transform.InverseTransformVector(currentDragPosition);

        Vector3 localLastDragPosition = transform.InverseTransformVector(_sphereRotateLastDragPosition);
        Vector3 localCurrentDragPosition = transform.InverseTransformVector(currentDragPosition);

        float angleXPivot = Vector3.SignedAngle(pivotLastDragPosition, pivotCurrentDragPosition, Vector3.right);
        float angleY = Vector3.SignedAngle(localLastDragPosition, localCurrentDragPosition, Vector3.up);

        float absDX = Mathf.Max(0.001f, Mathf.Abs(_initialDragMousePosition.x - mousePosition.x));
        float absDY = Mathf.Max(0.001f, Mathf.Abs(_initialDragMousePosition.y - mousePosition.y));

        // Introduce a rotation bias toward a single axis to minimize rotation wobble
        angleXPivot *= Mathf.Min(1, absDY / absDX);
        angleY *= Mathf.Min(1, absDX / absDY);

        Pivot.transform.Rotate(angleXPivot, 0, 0);
        transform.Rotate(0, angleY, 0);

        Quaternion pivotRotation = Pivot.transform.rotation;
        Vector3 pivotEulerAngles = pivotRotation.eulerAngles;
        
        // Prevent the globe pivot from rotating beyond the poles
        if ((pivotEulerAngles.x > 89) && (pivotEulerAngles.x <= 135))
        {
            pivotEulerAngles.x = 89;
        }
        if ((pivotEulerAngles.x < 271) && (pivotEulerAngles.x > 135))
        {
            pivotEulerAngles.x = 271;
        }
        pivotEulerAngles.y = 0;
        pivotEulerAngles.z = 0;

        pivotRotation.eulerAngles = pivotEulerAngles;
        Pivot.transform.rotation = pivotRotation;

        _sphereRotateLastDragPosition = currentDragPosition;

        // Lerp initial mouse pos toward current mouse pos to soften axis switching in rotation bias
        _initialDragMousePosition = Vector3.Lerp(_initialDragMousePosition, mousePosition, 0.25f);
    }

    private void BeginDragSurface(Vector3 mousePosition)
    {
        Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
        
        RaycastHit raycastHit; 

        if (!SphereCollider.Raycast(ray, out raycastHit, 50))
        {
            Debug.Log("Mouse not within sphere");

            return; // Didn't actually pressed mouse on the sphere's surface
        }

        _initialDragMousePosition = mousePosition;

        float distance = (Camera.transform.position - transform.position).magnitude * _rayDistFactor - Camera.nearClipPlane;

        _sphereRotateLastDragPosition = ray.GetPoint(distance);

        _isDraggingSurface = true;

        Debug.Log("Started surface drag");
    }

    public void EndDragSurface(Vector3 mousePosition)
    {
        if (!_isDraggingSurface)
            return;

        _isDraggingSurface = false;

        Debug.Log("Ended surface drag");
    }
}
