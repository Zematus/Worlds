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
    public List<Element.Instance> Elements
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

    private static TerrainCell _startCell;
    private static int _rngOffset;

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

        Region region = TryGenerateBiomeRegion(startCell, establishmentLanguage, startCell.BiomeWithMostPresence);

        return region;
    }

    private static int GetRandomInt(int maxValue)
    {
        return _startCell.GetNextLocalRandomInt(_rngOffset++, maxValue);
    } 

    public static Region TryGenerateBiomeRegion(TerrainCell startCell, Language establishmentLanguage, string biomeId)
    {
        int regionSize = 1;

        HashSet<CellRegion> borderingRegions = new HashSet<CellRegion>();

        // round the base altitude
        float baseAltitude = AltitudeRoundnessTarget * Mathf.Round(startCell.Altitude / AltitudeRoundnessTarget);

        HashSet<TerrainCell> acceptedCells = new HashSet<TerrainCell>();
        HashSet<TerrainCell> unacceptedCells = new HashSet<TerrainCell>();

        acceptedCells.Add(startCell);

        HashSet<TerrainCell> cellsToExplore = new HashSet<TerrainCell>();

        foreach (TerrainCell cell in startCell.Neighbors.Values)
        {
            cellsToExplore.Add(cell);
        }

        bool addedAcceptedCells = true;

        int borderCells = 0;

        while (addedAcceptedCells)
        {
            HashSet<TerrainCell> nextCellsToExplore = new HashSet<TerrainCell>();
            addedAcceptedCells = false;

            if (cellsToExplore.Count <= 0)
                break;

            float closedness = 1 - cellsToExplore.Count / (float)(cellsToExplore.Count + borderCells);

            foreach (TerrainCell cell in cellsToExplore)
            {
                float closednessFactor = 1;
                float cutOffFactor = 2;

                if (MaxClosedness < 1)
                {
                    closednessFactor = (1 + MaxClosedness / cutOffFactor) * (1 - closedness) / (1 - MaxClosedness) - MaxClosedness / cutOffFactor;
                }

                float maxAltitudeDifference = BaseMaxAltitudeDifference * closednessFactor;

                bool accepted = false;

                string cellBiomeId = cell.BiomeWithMostPresence;

                if (cell.Region != null)
                {
                    borderingRegions.Add(cell.Region as CellRegion);
                }
                else if (cellBiomeId == biomeId)
                {
                    if (Mathf.Abs(cell.Altitude - baseAltitude) < maxAltitudeDifference)
                    {
                        accepted = true;
                        acceptedCells.Add(cell);
                        addedAcceptedCells = true;
                        regionSize++;

                        foreach (KeyValuePair<Direction, TerrainCell> pair in cell.Neighbors)
                        {
                            TerrainCell ncell = pair.Value;

                            if (cellsToExplore.Contains(ncell))
                                continue;

                            if (unacceptedCells.Contains(ncell))
                                continue;

                            if (acceptedCells.Contains(ncell))
                                continue;

                            nextCellsToExplore.Add(ncell);
                        }
                    }
                }

                if (!accepted)
                {
                    unacceptedCells.Add(cell);
                    borderCells++;
                }
            }

            cellsToExplore = nextCellsToExplore;
        }

        CellRegion region = null;

        if ((regionSize <= 20) && (borderingRegions.Count > 0))
        {
            _rngOffset = RngOffsets.REGION_SELECT_BORDER_REGION_TO_REPLACE_WITH;
            _startCell = startCell;
            
            region = borderingRegions.RandomSelect(GetRandomInt);

            region.ResetInfo();
        }
        else
        {
            region = new CellRegion(startCell, establishmentLanguage);
        }

        foreach (TerrainCell cell in acceptedCells)
        {
            region.AddCell(cell);
        }

        region.EvaluateAttributes();

        region.Update();

        return region;
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
