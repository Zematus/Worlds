using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class PanelDragScript : MonoBehaviour
{
    public Camera Camera;

    private bool _isDragging = false;
    private Vector3 _offset;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    public void StartDrag(BaseEventData eventData)
    {
        PointerEventData data = eventData as PointerEventData;

        if (data.button != PointerEventData.InputButton.Left)
            return;

        _isDragging = true;

        //Vector3 worldPosition = Camera.ScreenToWorldPoint(new Vector3(data.position.x, data.position.y, 0));
        Vector3 worldPosition = new Vector3(data.position.x, data.position.y, 0);

        _offset = worldPosition - transform.position;
    }
    
    public void Drag(BaseEventData eventData)
    {
        if (!_isDragging)
            return;

        PointerEventData data = eventData as PointerEventData;
        
        //Vector3 worldPosition = Camera.ScreenToWorldPoint(new Vector3(data.position.x, data.position.y, 0));
        Vector3 worldPosition = new Vector3(data.position.x, data.position.y, 0);

        transform.position = worldPosition - _offset;
    }
    
    public void EndDrag(BaseEventData eventData)
    {
        if (!_isDragging)
            return;

        PointerEventData data = eventData as PointerEventData;

        if (data.button != PointerEventData.InputButton.Left)
            return;

        _isDragging = false;
    }
}
