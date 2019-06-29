using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class DiscoveryEventMessage : CellEventMessage
{
    [XmlAttribute]
    public string DiscoveryName;

    public DiscoveryEventMessage()
    {

    }

    public DiscoveryEventMessage(string discoveryName, TerrainCell cell, long id, long date) : base(cell, id, date)
    {
        DiscoveryName = discoveryName;
    }

    protected override string GenerateMessage()
    {
        string prefix = DiscoveryName + " discovered";

        Territory territory = World.GetCell(Position).EncompassingTerritory;

        if (territory != null)
        {
            return prefix + " in " + territory.Polity.Name.BoldText + " at " + Position;
        }

        return prefix + " at " + Position;
    }
}
