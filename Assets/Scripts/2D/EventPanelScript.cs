using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPanelScript : MonoBehaviour
{
    public EventMessagePanelScript EventMessagePanelPrefab;

    private HashSet<EventMessagePanelScript> _eventMessagePanels = new HashSet<EventMessagePanelScript>();

    public void AddEventMessage(string message, EventMessageGotoDelegate gotoDelegate = null)
    {
        EventMessagePanelScript eventMessagePanel = GameObject.Instantiate(EventMessagePanelPrefab) as EventMessagePanelScript;

        eventMessagePanel.SetParentPanel(this);
        eventMessagePanel.SetText(message);
        eventMessagePanel.SetGotoDelegate(gotoDelegate);
        eventMessagePanel.transform.SetParent(transform);
        eventMessagePanel.transform.localScale = Vector3.one;

        _eventMessagePanels.Add(eventMessagePanel);
    }

    public void DestroyMessagePanels()
    {
        EventMessagePanelScript[] messagePanels = new EventMessagePanelScript[_eventMessagePanels.Count];

        _eventMessagePanels.CopyTo(messagePanels);

        foreach (EventMessagePanelScript messagePanel in messagePanels)
        {
            messagePanel.Remove();
        }

        _eventMessagePanels.Clear();
    }

    public void RemoveMessagePanel(EventMessagePanelScript messagePanel)
    {
        _eventMessagePanels.Remove(messagePanel);
    }
}
