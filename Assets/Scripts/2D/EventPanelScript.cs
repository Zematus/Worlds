using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPanelScript : MonoBehaviour
{
    public EventMessagePanelScript EventMessagePanelPrefab;

    public void AddEventMessage(string message, EventMessageGotoDelegate gotoDelegate = null)
    {
        EventMessagePanelScript eventMessagePanel = GameObject.Instantiate(EventMessagePanelPrefab) as EventMessagePanelScript;

        eventMessagePanel.SetText(message);
        eventMessagePanel.SetGotoDelegate(gotoDelegate);
        eventMessagePanel.transform.SetParent(transform);
    }
}
