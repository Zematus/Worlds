using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class PlanetScript : MonoBehaviour
{
    public Camera Camera;
    public SphereCollider SphereCollider;

    private bool _isDraggingSurface = false;

    private Vector3 _sphereRotateLastDragPosition;

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

        RaycastHit raycastHit;

        if (!SphereCollider.Raycast(ray, out raycastHit, 50))
        {
            return;
        }
        
        Vector3 currentDragPosition = raycastHit.point - transform.position;

        Vector3 localLastDragPosition = transform.InverseTransformVector(_sphereRotateLastDragPosition);
        Vector3 localCurrentDragPosition = transform.InverseTransformVector(currentDragPosition);

        float angleX = Vector3.SignedAngle(localLastDragPosition, localCurrentDragPosition, Vector3.right);
        float angleY = Vector3.SignedAngle(localLastDragPosition, localCurrentDragPosition, Vector3.up);
        float angleZ = Vector3.SignedAngle(localLastDragPosition, localCurrentDragPosition, Vector3.forward);

        transform.Rotate(angleX, 0, 0);

        _sphereRotateLastDragPosition = currentDragPosition;
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

        _sphereRotateLastDragPosition = raycastHit.point - transform.position;

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
