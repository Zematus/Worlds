using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class FactionInfo : Identifiable
{
    [XmlAttribute("T")]
    public string Type;

    public Name Name = null;

    public Faction Faction;

    public long FormationDate => InitDate;

    private string _nameFormat;

    public FactionInfo()
    {

    }

    public FactionInfo(Faction faction, string type, long formationDate, long initId) :
        base (formationDate, initId)
    {
        Type = type;

        Faction = faction;

        switch (Type)
        {
            case Clan.FactionType:
                _nameFormat = Clan.FactionNameFormat;
                break;
            default:
                throw new System.Exception("Unhandled Faction type: " + Type);
        }
    }

    public string GetNameAndTypeString()
    {
        return string.Format(_nameFormat, Name);
    }

    public string GetNameAndTypeStringBold()
    {
        return string.Format(_nameFormat, Name.BoldText);
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        if (Faction != null)
            Faction.FinalizeLoad();

        switch (Type)
        {
            case Clan.FactionType:
                _nameFormat = Clan.FactionNameFormat;
                break;
            default:
                throw new System.Exception("Unhandled Faction type: " + Type);
        }
    }

    public override void Synchronize()
    {
        if (Faction != null)
            Faction.Synchronize();
    }
}
