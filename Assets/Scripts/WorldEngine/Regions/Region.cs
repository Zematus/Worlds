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
    public bool IsUiFilteredIn = false;

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

    [XmlIgnore]
    public HashSet<Region> NeighborRegions = new HashSet<Region>();

    public Identifier Id => Info.Id;

    public Name Name => Info.Name;

    public Dictionary<string, RegionAttribute.Instance> Attributes => Info.Attributes;

    public virtual List<Element.Instance> Elements => Info.Elements;

    public World World => Info.World;

    protected Dictionary<string, float> _biomePresences;

    protected event System.Action<Region> UpdateEvent;

    public Region()
    {

    }

    public Region(TerrainCell originCell, long idOffset, Language language)
    {
        Info = new RegionInfo(this, originCell, idOffset, language);
    }

    public static void SetAsNeighbors(Region r1, Region r2)
    {
        r1.AddNeighbor(r2);
        r2.AddNeighbor(r1);
    }

    private void AddNeighbor(Region region)
    {
        if (NeighborRegions.Add(region))
        {
            UpdateEvent?.Invoke(this);
        }
    }

    public void AddUpdateEventHandler(System.Action<Region> updateEventHandler)
    {
        UpdateEvent += updateEventHandler;
    }

    public void RemoveUpdateEventHandler(System.Action<Region> updateEventHandler)
    {
        UpdateEvent -= updateEventHandler;
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

        return region;
    }

    public override int GetHashCode()
    {
        return Info.GetHashCode();
    }

    public string GetRandomUnstranslatedAreaName(GetRandomIntDelegate getRandomInt, bool isNounAdjunct)
    {
        return Info.GetRandomUnstranslatedAreaName(getRandomInt, isNounAdjunct);
    }

    public abstract TerrainCell GetMostCenteredCell();
}
