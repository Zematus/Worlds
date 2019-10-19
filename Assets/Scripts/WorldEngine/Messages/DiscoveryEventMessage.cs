using ProtoBuf;

[ProtoContract]
public class DiscoveryEventMessage : CellEventMessage
{
    [ProtoMember(1)]
    public string DiscoveryId;

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
