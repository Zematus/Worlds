using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class CulturalPreferenceInfo : IKeyedValue<string>, ISynchronizable
{
    [XmlAttribute]
    public string Id;

    [XmlIgnore]
    public string Name;

    [XmlIgnore]
    public int RngOffset;

    public CulturalPreferenceInfo()
    {
    }

    public CulturalPreferenceInfo(string id, string name, int rngOffset)
    {
        Id = id;
        Name = name;
        RngOffset = rngOffset;
    }

    public CulturalPreferenceInfo(CulturalPreferenceInfo basePreference)
    {
        Id = basePreference.Id;
        Name = basePreference.Name;
        RngOffset = basePreference.RngOffset;
    }

    public string GetKey()
    {
        return Id;
    }

    public virtual void Synchronize()
    {
    }

    public virtual void FinalizeLoad()
    {
        PreferenceGenerator generator = World.GetPreferenceGenerator(Id);

        if (generator == null)
        {
            throw new System.Exception("Unable to load preference generator: " + Id);
        }

        Name = generator.Name;
        RngOffset = generator.IdHash;
    }
}
