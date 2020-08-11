using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

[System.Obsolete]
public abstract class HumanGroup : Identifiable
{
    [XmlIgnore]
    public World World;

    public HumanGroup()
    {
    }

    public HumanGroup(World world)
    {
        World = world;
    }
}
