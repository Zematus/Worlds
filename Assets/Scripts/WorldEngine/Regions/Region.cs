using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

[XmlInclude(typeof(CellRegion))]
[XmlInclude(typeof(SuperRegion))]
public abstract class Region : ISynchronizable
{
    [XmlIgnore]
    public RegionInfo Info;

    [XmlIgnore]
    public bool IsSelected = false;

    [XmlIgnore]
    public Region Parent = null;

    [XmlIgnore]
    public float AverageAltitude;
    [XmlIgnore]
    public float AverageRainfall;
    [XmlIgnore]
    public float AverageTemperature;
    [XmlIgnore]
    public float AverageFlowingWater;

    [XmlIgnore]
    public float AverageSurvivability;
    [XmlIgnore]
    public float AverageForagingCapacity;
    [XmlIgnore]
    public float AverageAccessibility;
    [XmlIgnore]
    public float AverageArability;

    [XmlIgnore]
    public float AverageFarmlandPercentage;

    [XmlIgnore]
    public float TotalArea;

    [XmlIgnore]
    public string BiomeWithMostPresence = null;
    [XmlIgnore]
    public float MostBiomePresence;

    [XmlIgnore]
    public List<string> PresentBiomeIds = new List<string>();
    [XmlIgnore]
    public List<float> BiomePresences = new List<float>();

    [XmlIgnore]
    public float AverageOuterBorderAltitude;
    [XmlIgnore]
    public float MinAltitude;
    [XmlIgnore]
    public float MaxAltitude;
    [XmlIgnore]
    public float CoastPercentage;
    [XmlIgnore]
    public float WaterPercentage;

    public Identifier UniqueIndentifier
    {
        get
        {
            return Info.UniqueIdentifier;
        }
    }

    public Name Name
    {
        get
        {
            return Info.Name;
        }
    }

    public Dictionary<string, RegionAttribute.Instance> Attributes
    {
        get
        {
            return Info.Attributes;
        }
    }

    public virtual List<Element.Instance> Elements
    {
        get
        {
            return Info.Elements;
        }
    }

    public World World
    {
        get
        {
            return Info.World;
        }
    }

    protected Dictionary<string, float> _biomePresences;

    private HashSet<Region> _subRegions = new HashSet<Region>();

    public Region()
    {

    }

    public Region(long date, long id, TerrainCell originCell, Language language)
    {
        Info = new RegionInfo(date, id, this, originCell, language);
    }

    [System.Obsolete]
    public void ResetInfo()
    {
        //RegionInfo newInfo = new RegionInfo(this, Info.OriginCell, Info.Language);

        //Info.Region = null; // Old region info object should no longer point to this region but remain in memory for further references

        //Info = newInfo; // Replace info object with new one
    }

    public bool IsEqualToOrDescentantFrom(Region region)
    {
        Region p = this;

        while (p != null)
        {
            if (p == region) return true;

            p = p.Parent;
        }

        return false;
    }

    public abstract ICollection<TerrainCell> GetCells();

    public abstract bool IsInnerBorderCell(TerrainCell cell);

    public abstract bool IsWithinRegion(TerrainCell cell);

    public abstract void Synchronize();

    public abstract void FinalizeLoad();

    public float GetBiomePresence(string biomeId)
    {
        float presence;

        if (!_biomePresences.TryGetValue(biomeId, out presence))
        {
            return 0.0f;
        }

        return presence;
    }

    public static Region TryGenerateRegion(TerrainCell startCell, Language establishmentLanguage)
    {
        Region region =
            BiomeCellRegionBuilder.TryGenerateRegion(startCell, establishmentLanguage);

        //if (startCell.WaterBiomePresence >= 1)
        //    return null;

        //if (startCell.Region != null)
        //    return null;

        //Region region = BiomeCellRegionBuilder.TryGenerateRegion_original(
        //    startCell, establishmentLanguage, startCell.BiomeWithMostPresence);

        return region;
    }

    public string GetRandomUnstranslatedAreaName(GetRandomIntDelegate getRandomInt, bool isNounAdjunct)
    {
        return Info.GetRandomUnstranslatedAreaName(getRandomInt, isNounAdjunct);
    }

    public abstract TerrainCell GetMostCenteredCell();
}
