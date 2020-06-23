using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

[XmlInclude(typeof(CellRegion))]
public abstract class Region : ISynchronizable
{
    public const float BaseMaxAltitudeDifference = 1000;
    public const int AltitudeRoundnessTarget = 2000;

    public const float MaxClosedness = 0.5f;

    [XmlIgnore]
    public RegionInfo Info;

    [XmlIgnore]
    public bool IsSelected = false;

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

    [XmlIgnore]
    public long Id
    {
        get
        {
            return Info.Id;
        }
    }

    [XmlIgnore]
    public Name Name
    {
        get
        {
            return Info.Name;
        }
    }

    [XmlIgnore]
    public Dictionary<string, RegionAttribute.Instance> Attributes
    {
        get
        {
            return Info.Attributes;
        }
    }

    [XmlIgnore]
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

    protected static TerrainCell _startCell;
    protected static int _rngOffset;

    public Region()
    {

    }

    public Region(TerrainCell originCell, Language language)
    {
        Info = new RegionInfo(this, originCell, language);
    }

    public void ResetInfo()
    {
        RegionInfo newInfo = new RegionInfo(this, Info.OriginCell, Info.Language);

        Info.Region = null; // Old region info object should no longer point to this region but remain in memory for further references

        Info = newInfo; // Replace info object with new one
    }

    public abstract ICollection<TerrainCell> GetCells();

    public abstract bool IsInnerBorderCell(TerrainCell cell);

    public virtual void Synchronize()
    {
    }

    public virtual void FinalizeLoad()
    {
    }

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
        if (startCell.WaterBiomePresence >= 1)
            return null;

        if (startCell.Region != null)
            return null;

        Region region = CellRegion.TryGenerateBiomeRegion_original(startCell, establishmentLanguage, startCell.BiomeWithMostPresence);

        return region;
    }

    protected static int GetRandomInt(int maxValue)
    {
        return _startCell.GetNextLocalRandomInt(_rngOffset++, maxValue);
    }

    public string GetRandomAttributeVariation(GetRandomIntDelegate getRandomInt)
    {
        return Info.GetRandomAttributeVariation(getRandomInt);
    }

    public string GetRandomUnstranslatedAreaName(GetRandomIntDelegate getRandomInt, bool isNounAdjunct)
    {
        return Info.GetRandomUnstranslatedAreaName(getRandomInt, isNounAdjunct);
    }

    public abstract TerrainCell GetMostCenteredCell();
}
