using System.Xml;
using System.Xml.Serialization;

public class DiscoveryEventMessage : CellEventMessage
{
    [XmlAttribute("DId")]
    public string DiscoveryId;

    [XmlIgnore]
    public IDiscovery Discovery;

    public DiscoveryEventMessage()
    {

    }

    public DiscoveryEventMessage(IDiscovery discovery, TerrainCell cell, long id, long date) : base(cell, id, date)
    {
        DiscoveryId = discovery.Id;
        Discovery = discovery;
    }

    protected override string GenerateMessage()
    {
        string prefix = $"{Discovery.Name} discovered";

        Territory territory = World.GetCell(Position).EncompassingTerritory;

        if (territory != null)
        {
            return $"{prefix} in {territory.Polity.Name.BoldText} at {Position}";
        }

        return $"{prefix} at {Position}";
    }
}
