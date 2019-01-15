using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class PanelDragScript : MonoBehaviour
{
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
        
        Vector3 pointerPosition = new Vector3(data.position.x, data.position.y, 0);

        _offset = pointerPosition - transform.position;
    }
    
    public void Drag(BaseEventData eventData)
    {
        if (!_isDragging)
            return;

        PointerEventData data = eventData as PointerEventData;
        
        Vector3 pointerPosition = new Vector3(data.position.x, data.position.y, 0);

        Vector3 newPosition = pointerPosition - _offset;

        transform.position = newPosition;
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
