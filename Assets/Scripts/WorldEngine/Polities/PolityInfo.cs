using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;
using System.Linq;
using System.Xml.Schema;

public class PolityInfo : ISynchronizable, IKeyedValue<long>
{
	[XmlAttribute("T")]
	public string Type;

	[XmlAttribute]
	public long Id;

	public Name Name;

    public Polity Polity;

    private string _nameFormat;

    public PolityInfo()
    {
	
	}

	public PolityInfo(string type, long id, Polity polity)
    {
		Type = type;
        Id = id;

        Polity = polity;

        switch (Type)
        {
            case Tribe.PolityType:
                _nameFormat = Tribe.PolityNameFormat;
                break;
            default:
                throw new System.Exception("Unhandled Polity type: " + Type);
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
        if (Polity != null)
            Polity.FinalizeLoad();

        switch (Type)
        {
            case Tribe.PolityType:
                _nameFormat = Tribe.PolityNameFormat;
                break;
            default:
                throw new System.Exception("Unhandled Polity type: " + Type);
        }
    }

    public void Synchronize()
    {
        if (Polity != null)
            Polity.Synchronize();
    }
}
