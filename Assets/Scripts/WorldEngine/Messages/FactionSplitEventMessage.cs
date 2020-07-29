using System.Xml;
using System.Xml.Serialization;

public class FactionSplitEventMessage : FactionEventMessage
{
    public Identifier OldFactionId;

    public FactionSplitEventMessage()
    {
    }

    public FactionSplitEventMessage(Faction oldFaction, Faction newFaction, long date)
        : base(newFaction, FactionSplitEventMessageId, date)
    {
        OldFactionId = oldFaction.Id;
    }

    protected override string GenerateMessage()
    {
        FactionInfo oldFaction = World.GetFactionInfo(OldFactionId);

        return "A new faction, " + FactionInfo.Name.BoldText +
            ", has split from " + oldFaction.GetNameAndTypeStringBold();
    }
}
