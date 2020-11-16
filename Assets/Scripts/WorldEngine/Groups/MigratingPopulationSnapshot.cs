using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// Represents a snapshot of a population migration event
/// </summary>
public class MigratingPopulationSnapshot
{
    public int Population = 0;
    public Identifier SourceGroupId = null;
    public Identifier PolityId = null;

    [XmlIgnore]
    public CellGroup SourceGroup;

    [XmlIgnore]
    public PolityInfo PolityInfo;

    public void Set(int population, CellGroup sourceGroup, PolityInfo polityInfo)
    {
        Population = population;

        SourceGroup = sourceGroup;
        SourceGroupId = sourceGroup.Id;

        PolityInfo = polityInfo;

        if (polityInfo != null)
        {
            PolityId = polityInfo.Id;
        }
    }
}
