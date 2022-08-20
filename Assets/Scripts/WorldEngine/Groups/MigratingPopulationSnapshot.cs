using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// Represents a snapshot of a population migration event
/// </summary>
public class MigratingPopulationSnapshot : ISynchronizable
{

    [XmlAttribute("P")]
    public int Population = 0;
    [XmlAttribute("SD")]
    public long StartDate = 0;
    [XmlAttribute("ED")]
    public long EndDate = 0;

    #region SourceGroupId
    [XmlAttribute("SGId")]
    public string SourceGroupIdStr
    {
        get { return SourceGroupId; }
        set { SourceGroupId = value; }
    }
    [XmlIgnore]
    public Identifier SourceGroupId;
    #endregion

    #region PolityId
    [XmlAttribute("PId")]
    public string PolityIdStr
    {
        get { return PolityId; }
        set { PolityId = value; }
    }
    [XmlIgnore]
    public Identifier PolityId;
    #endregion

    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public CellGroup SourceGroup;

    [XmlIgnore]
    public PolityInfo PolityInfo;

    public void FinalizeLoad()
    {
        SourceGroup = World.GetGroup(SourceGroupId);

        if (PolityId != null)
        {
            PolityInfo = World.GetPolityInfo(PolityId);
        }
    }

    public void Set(
        int population,
        CellGroup sourceGroup,
        PolityInfo polityInfo,
        long startDate,
        long endDate)
    {
        World = sourceGroup.World;

        Population = population;

        SourceGroup = sourceGroup;
        SourceGroupId = sourceGroup.Id;

        PolityInfo = polityInfo;

        if (polityInfo != null)
        {
            PolityId = polityInfo.Id;
        }

        StartDate = startDate;
        EndDate = endDate;
    }

    public void Synchronize()
    {
    }
}
