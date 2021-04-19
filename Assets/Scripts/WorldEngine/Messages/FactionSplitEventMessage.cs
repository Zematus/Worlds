using System.Xml;
using System.Xml.Serialization;

public class FactionSplitEventMessage : FactionEventMessage
{
    #region OldFactionId
    [XmlAttribute("OFId")]
    public string OldFactionIdStr
    {
        get { return OldFactionId; }
        set { OldFactionId = value; }
    }
    [XmlIgnore]
    public Identifier OldFactionId;
    #endregion

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
