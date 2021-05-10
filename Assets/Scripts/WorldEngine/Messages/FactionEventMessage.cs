using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class FactionEventMessage : CellEventMessage
{
    #region FactionId
    [XmlAttribute("FId")]
    public string FactionIdStr
    {
        get { return FactionId; }
        set { FactionId = value; }
    }
    [XmlIgnore]
    public Identifier FactionId;
    #endregion

    [XmlIgnore]
    public FactionInfo FactionInfo
    {
        get { return World.GetFactionInfo(FactionId); }
    }

    public FactionEventMessage()
    {

    }

    public FactionEventMessage(Faction faction, long id, long date) :
        base(faction.CoreGroup.Cell, id, date)
    {
        FactionId = faction.Id;
    }
}
