using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class DiscoveryEvent : CellGroupEvent
{
    public DiscoveryEvent()
    {

    }

    public DiscoveryEvent(CellGroup group, long triggerDate, long eventTypeId) : base(group, triggerDate, eventTypeId)
    {

    }

    public void TryGenerateEventMessage(long discoveryEventId, string discoveryId)
    {
        DiscoveryEventMessage eventMessage = null;

        if (!World.HasEventMessage(discoveryEventId))
        {
            eventMessage = new DiscoveryEventMessage(discoveryId, Group.Cell, discoveryEventId, TriggerDate);

            World.AddEventMessage(eventMessage);
        }

        if (Group.Cell.EncompassingTerritory != null)
        {
            Polity encompassingPolity = Group.Cell.EncompassingTerritory.Polity;

            if (!encompassingPolity.HasEventMessage(discoveryEventId))
            {
                if (eventMessage == null)
                    eventMessage = new DiscoveryEventMessage(discoveryId, Group.Cell, discoveryEventId, TriggerDate);

                encompassingPolity.AddEventMessage(eventMessage);
            }
        }
    }
}
