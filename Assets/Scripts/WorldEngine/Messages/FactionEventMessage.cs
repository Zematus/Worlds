using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class FactionEventMessage : CellEventMessage
{
    public Identifier FactionId;

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
