﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class FactionInfo : ISynchronizable, IKeyedValue<long>
{
    [XmlAttribute("D")]
    public long FormationDate = -1;

    [XmlAttribute("T")]
    public string Type;

    [XmlAttribute]
    public long Id;

    public Name Name = null;

    public Faction Faction;

    private string _nameFormat;

    public FactionInfo()
    {

    }

    public FactionInfo(string type, long id, Faction faction)
    {
        Type = type;
        Id = id;

        FormationDate = faction.World.CurrentDate;

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

    public long GetKey()
    {
        return Id;
    }

    public void FinalizeLoad()
    {
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

    public void Synchronize()
    {
        if (Faction != null)
            Faction.Synchronize();
    }
}
