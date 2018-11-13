using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

public class CellRegion : Region
{
    public List<WorldPosition> CellPositions;

    private HashSet<TerrainCell> _cells = new HashSet<TerrainCell>();

    private HashSet<TerrainCell> _innerBorderCells = new HashSet<TerrainCell>();

    private HashSet<TerrainCell> _outerBorderCells = new HashSet<TerrainCell>();

    private TerrainCell _mostCenteredCell = null;

    public CellRegion()
    {

    }

    public CellRegion(TerrainCell originCell, Language language) : base(originCell, language)
    {

    }

    public void Update()
    {
        Manager.AddUpdatedCells(_cells, CellUpdateType.Region, CellUpdateSubType.Membership);
    }

    public bool AddCell(TerrainCell cell)
    {
        if (!_cells.Add(cell))
            return false;

        cell.Region = this;
        //Manager.AddUpdatedCell(cell, CellUpdateType.Region, CellUpdateSubType.Membership);

        return true;
    }

    public override ICollection<TerrainCell> GetCells()
    {
        return _cells;
    }

    public override bool IsInnerBorderCell(TerrainCell cell)
    {
        return _innerBorderCells.Contains(cell);
    }

    public void EvaluateAttributes()
    {
        Dictionary<string, float> biomePresences = new Dictionary<string, float>();

        float oceanicArea = 0;
        float coastalOuterBorderArea = 0;
        float outerBorderArea = 0;

        MinAltitude = float.MaxValue;
        MaxAltitude = float.MinValue;

        AverageOuterBorderAltitude = 0;

        AverageAltitude = 0;
        AverageRainfall = 0;
        AverageTemperature = 0;

        AverageSurvivability = 0;
        AverageForagingCapacity = 0;
        AverageAccessibility = 0;
        AverageArability = 0;

        AverageFarmlandPercentage = 0;

        TotalArea = 0;

        MostBiomePresence = 0;

        _innerBorderCells.Clear();
        _outerBorderCells.Clear();

        foreach (TerrainCell cell in _cells)
        {
            float cellArea = cell.Area;

            bool isInnerBorder = false;

            bool isNotFullyOceanic = (cell.GetBiomePresence(Biome.Ocean) < 1);

            foreach (TerrainCell nCell in cell.Neighbors.Values)
            {
                if (nCell.Region != this)
                {
                    isInnerBorder = true;

                    if (_outerBorderCells.Add(nCell))
                    {
                        float nCellArea = nCell.Area;

                        outerBorderArea += nCellArea;
                        AverageOuterBorderAltitude += cell.Altitude * nCellArea;

                        if (isNotFullyOceanic && (nCell.GetBiomePresence(Biome.Ocean) >= 1))
                        {
                            coastalOuterBorderArea += nCellArea;
                        }
                    }
                }
            }

            if (isInnerBorder)
            {
                _innerBorderCells.Add(cell);
            }

            if (MinAltitude > cell.Altitude)
            {
                MinAltitude = cell.Altitude;
            }

            if (MaxAltitude < cell.Altitude)
            {
                MaxAltitude = cell.Altitude;
            }

            AverageAltitude += cell.Altitude * cellArea;
            AverageRainfall += cell.Rainfall * cellArea;
            AverageTemperature += cell.Temperature * cellArea;

            AverageSurvivability += cell.Survivability * cellArea;
            AverageForagingCapacity += cell.ForagingCapacity * cellArea;
            AverageAccessibility += cell.Accessibility * cellArea;
            AverageArability += cell.Arability * cellArea;

            AverageFarmlandPercentage += cell.FarmlandPercentage * cellArea;

            foreach (string biomeName in cell.PresentBiomeNames)
            {
                float presenceArea = cell.GetBiomePresence(biomeName) * cellArea;

                if (biomePresences.ContainsKey(biomeName))
                {
                    biomePresences[biomeName] += presenceArea;
                }
                else
                {
                    biomePresences.Add(biomeName, presenceArea);
                }

                if (biomeName == Biome.Ocean.Name)
                {
                    oceanicArea += presenceArea;
                }
            }

            TotalArea += cellArea;
        }

        AverageAltitude /= TotalArea;
        AverageRainfall /= TotalArea;
        AverageTemperature /= TotalArea;

        AverageSurvivability /= TotalArea;
        AverageForagingCapacity /= TotalArea;
        AverageAccessibility /= TotalArea;
        AverageArability /= TotalArea;

        AverageFarmlandPercentage /= TotalArea;

        OceanPercentage = oceanicArea / TotalArea;

        AverageOuterBorderAltitude /= outerBorderArea;

        CoastPercentage = coastalOuterBorderArea / outerBorderArea;

        PresentBiomeNames = new List<string>(biomePresences.Count);
        BiomePresences = new List<float>(biomePresences.Count);

        _biomePresences = new Dictionary<string, float>(biomePresences.Count);

        foreach (KeyValuePair<string, float> pair in biomePresences)
        {
            float presence = pair.Value / TotalArea;

            PresentBiomeNames.Add(pair.Key);
            BiomePresences.Add(presence);

            _biomePresences.Add(pair.Key, presence);

            if (MostBiomePresence < presence)
            {
                MostBiomePresence = presence;
                BiomeWithMostPresence = pair.Key;
            }
        }

        //#if DEBUG
        //        if ((Manager.RegisterDebugEvent != null) && (Manager.TracingData.Priority <= 0))
        //        {
        //            //			if ((originCell.Longitude == Manager.TracingData.Longitude) && (originCell.Latitude == Manager.TracingData.Latitude)) {
        //            string regionId = "Id:" + Id;

        //            SaveLoadTest.DebugMessage debugMessage = new SaveLoadTest.DebugMessage(
        //                "CellRegion::EvaluateAttributes - Region: " + regionId,
        //                "CurrentDate: " + World.CurrentDate +
        //                ", cell count: " + _cells.Count +
        //                ", TotalArea: " + TotalArea +
        //                "");

        //            Manager.RegisterDebugEvent("DebugMessage", debugMessage);
        //            //			}
        //        }
        //#endif

        CalculateMostCenteredCell();

        DefineAttributes();
        DefineElements();
    }

    public bool RemoveCell(TerrainCell cell)
    {
        if (!_cells.Remove(cell))
            return false;

        cell.Region = null;
        Manager.AddUpdatedCell(cell, CellUpdateType.Region, CellUpdateSubType.Membership);

        return true;
    }

    public override void Synchronize()
    {
        CellPositions = new List<WorldPosition>(_cells.Count);

        foreach (TerrainCell cell in _cells)
        {
            CellPositions.Add(cell.Position);
        }

        base.Synchronize();
    }

    public override void FinalizeLoad()
    {
        base.FinalizeLoad();

        foreach (WorldPosition position in CellPositions)
        {
            TerrainCell cell = World.GetCell(position);

            if (cell == null)
            {
                throw new System.Exception("Cell missing at position " + position.Longitude + "," + position.Latitude);
            }

            _cells.Add(cell);

            cell.Region = this;
        }

        EvaluateAttributes();
    }

    private void DefineAttributes()
    {
        if ((CoastPercentage > 0.45f) && (CoastPercentage < 0.70f))
        {
            Info.AddAttribute(RegionAttribute.Coast);
        }
        else if ((CoastPercentage >= 0.70f) && (CoastPercentage < 1f))
        {
            Info.AddAttribute(RegionAttribute.Peninsula);
        }
        else if (CoastPercentage >= 1f)
        {
            Info.AddAttribute(RegionAttribute.Island);
        }

        if (AverageAltitude > (AverageOuterBorderAltitude + 200f))
        {
            Info.AddAttribute(RegionAttribute.Highland);
        }

        if (AverageAltitude < (AverageOuterBorderAltitude - 200f))
        {
            Info.AddAttribute(RegionAttribute.Valley);

            if (AverageRainfall > 1000)
            {
                Info.AddAttribute(RegionAttribute.Basin);
            }
        }

        if (MostBiomePresence > 0.65f)
        {
            switch (BiomeWithMostPresence)
            {
                case "Desert":
                    Info.AddAttribute(RegionAttribute.Desert);
                    break;

                case "Desertic Tundra":
                    Info.AddAttribute(RegionAttribute.Desert);
                    break;

                case "Forest":
                    Info.AddAttribute(RegionAttribute.Forest);
                    break;

                case "Glacier":
                    Info.AddAttribute(RegionAttribute.Glacier);
                    break;

                case "Grassland":
                    Info.AddAttribute(RegionAttribute.Grassland);
                    break;

                case "Ice Cap":
                    Info.AddAttribute(RegionAttribute.IceCap);
                    break;

                case "Rainforest":
                    Info.AddAttribute(RegionAttribute.Rainforest);

                    if (AverageTemperature > 20)
                        Info.AddAttribute(RegionAttribute.Jungle);
                    break;

                case "Taiga":
                    Info.AddAttribute(RegionAttribute.Taiga);
                    break;

                case "Tundra":
                    Info.AddAttribute(RegionAttribute.Tundra);
                    break;
            }
        }

        if (Attributes.Count <= 0)
        {
            Info.AddAttribute(RegionAttribute.Region);
        }
    }

    private void DefineElements()
    {
        foreach (Element e in Element.Elements.Values)
        {
            if (e.Assignable(this))
            {
                Info.AddElement(e);
            }
        }
    }

    private void CalculateMostCenteredCell()
    {
        int centerLongitude = 0, centerLatitude = 0;

        foreach (TerrainCell cell in _cells)
        {
            centerLongitude += cell.Longitude;
            centerLatitude += cell.Latitude;
        }

        centerLongitude /= _cells.Count;
        centerLatitude /= _cells.Count;

        TerrainCell closestCell = null;
        int closestDistCenter = int.MaxValue;

        foreach (TerrainCell cell in _cells)
        {
            int distCenter = Mathf.Abs(cell.Longitude - centerLongitude) + Mathf.Abs(cell.Latitude - centerLatitude);

            if ((closestCell == null) || (distCenter < closestDistCenter))
            {
                closestDistCenter = distCenter;
                closestCell = cell;
            }
        }

        _mostCenteredCell = closestCell;
    }

    public override TerrainCell GetMostCenteredCell()
    {
        return _mostCenteredCell;
    }
}
