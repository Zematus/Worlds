using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class DiscoveryEventMessage : CellEventMessage
{
    [XmlAttribute("DId")]
    public string DiscoveryId;

    [XmlIgnore]
    public Discovery Discovery;

    public DiscoveryEventMessage()
    {

    }

    public DiscoveryEventMessage(Discovery discovery, TerrainCell cell, long id, long date) : base(cell, id, date)
    {
        DiscoveryId = discovery.Id;
        Discovery = discovery;
    }

    protected override string GenerateMessage()
    {
        string prefix = Discovery.Name + " discovered";

        Territory territory = World.GetCell(Position).EncompassingTerritory;

        if (territory != null)
        {
            return prefix + " in " + territory.Polity.Name.BoldText + " at " + Position;
        }

        return prefix + " at " + Position;
    }
}
