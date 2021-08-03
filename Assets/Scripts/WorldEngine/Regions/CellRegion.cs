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

    private RectInt _rect;

    public CellRegion()
    {

    }

    public CellRegion(TerrainCell originCell, Language language) : base(originCell, 0, language)
    {

    }

    public void Update()
    {
        Manager.AddUpdatedCells(this, CellUpdateType.Region, CellUpdateSubType.Membership);
    }

    public void AddCells(IEnumerable<TerrainCell> cells)
    {
        foreach (TerrainCell cell in cells)
        {
            AddCell(cell);
        }
    }

    private void UpdateRectangle(TerrainCell cell, bool first)
    {
        if (first)
        {
            _rect = new RectInt(cell.Position, Vector2Int.zero);
        }
        else
        {
            _rect.Extend(cell.Position, World.Width);
        }
    }

    public bool AddCell(TerrainCell cell)
    {
        UpdateRectangle(cell, _cells.Count == 0);

        if (!_cells.Add(cell))
            return false;

        cell.Region = this;

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

        float waterArea = 0;
        float coastalOuterBorderArea = 0;
        float outerBorderArea = 0;

        MinAltitude = float.MaxValue;
        MaxAltitude = float.MinValue;

        AverageOuterBorderAltitude = 0;

        AverageAltitude = 0;
        AverageRainfall = 0;
        AverageTemperature = 0;
        AverageFlowingWater = 0;

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

            bool isNotAllWater = !cell.IsAllWater;

            foreach (TerrainCell nCell in cell.NeighborList)
            {
                if (nCell.Region != this)
                {
                    isInnerBorder = true;

                    if (_outerBorderCells.Add(nCell))
                    {
                        if (nCell.Region != null)
                        {
                            SetAsNeighbors(this, nCell.Region);
                        }

                        float nCellArea = nCell.Area;

                        outerBorderArea += nCellArea;
                        AverageOuterBorderAltitude += cell.Altitude * nCellArea;

                        if (isNotAllWater && nCell.IsAllWater)
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
            AverageFlowingWater += cell.FlowingWater * cellArea;

            AverageSurvivability += cell.Survivability * cellArea;
            AverageForagingCapacity += cell.ForagingCapacity * cellArea;
            AverageAccessibility += cell.BaseAccessibility * cellArea;
            AverageArability += cell.BaseArability * cellArea;

            AverageFarmlandPercentage += cell.FarmlandPercentage * cellArea;

            foreach (string biomeId in cell.PresentBiomeIds)
            {
                float presenceArea = cell.GetBiomePresence(biomeId) * cellArea;

                if (biomePresences.ContainsKey(biomeId))
                {
                    biomePresences[biomeId] += presenceArea;
                }
                else
                {
                    biomePresences.Add(biomeId, presenceArea);
                }
            }

            foreach (string biomeId in cell.PresentWaterBiomeIds)
            {
                waterArea += cell.GetBiomePresence(biomeId) * cellArea;
            }

            TotalArea += cellArea;
        }

        AverageAltitude /= TotalArea;
        AverageRainfall /= TotalArea;
        AverageTemperature /= TotalArea;
        AverageFlowingWater /= TotalArea;

        AverageSurvivability /= TotalArea;
        AverageForagingCapacity /= TotalArea;
        AverageAccessibility /= TotalArea;
        AverageArability /= TotalArea;

        AverageFarmlandPercentage /= TotalArea;

        WaterPercentage = waterArea / TotalArea;

        AverageOuterBorderAltitude /= outerBorderArea;

        CoastPercentage = coastalOuterBorderArea / outerBorderArea;

        PresentBiomeIds = new List<string>(biomePresences.Count);
        BiomePresences = new List<float>(biomePresences.Count);

        _biomePresences = new Dictionary<string, float>(biomePresences.Count);

        foreach (KeyValuePair<string, float> pair in biomePresences)
        {
            float presence = pair.Value / TotalArea;

            PresentBiomeIds.Add(pair.Key);
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

    public override void Synchronize()
    {
        CellPositions = new List<WorldPosition>(_cells.Count);

        foreach (TerrainCell cell in _cells)
        {
            CellPositions.Add(cell.Position);
        }
    }

    public override void FinalizeLoad()
    {
        foreach (WorldPosition position in CellPositions)
        {
            TerrainCell cell = World.GetCell(position);

            if (cell == null)
            {
                throw new System.Exception("Cell missing at position " + position.Longitude + "," + position.Latitude);
            }

            UpdateRectangle(cell, _cells.Count == 0);

            _cells.Add(cell);

            cell.Region = this;
        }

        EvaluateAttributes();
    }

    private void DefineAttributes()
    {
        bool hasAddedAttribute;

        HashSet<RegionAttribute> attributesToSkip = new HashSet<RegionAttribute>();

        // Since there are attributes that have dependencies on other attributes, we might need to test each attribute more than once.
        do
        {
            hasAddedAttribute = false;

            foreach (RegionAttribute r in RegionAttribute.Attributes.Values)
            {
                if (!attributesToSkip.Contains(r) && r.Assignable(this))
                {
                    Info.AddAttribute(r.GetInstanceForRegion(this));
                    hasAddedAttribute = true;

                    attributesToSkip.Add(r); // If the attribute has already been added then we don't need it to test it again
                }
            }
        }
        while (hasAddedAttribute); // Repeat if at least one new attribute was added in the previous loop

        attributesToSkip.Clear();

        // Now validate secondary attributes
        do
        {
            hasAddedAttribute = false;

            foreach (RegionAttribute r in RegionAttribute.SecondaryAttributes.Values)
            {
                if (!attributesToSkip.Contains(r) && r.Assignable(this))
                {
                    Info.AddAttribute(r.GetInstanceForRegion(this));
                    hasAddedAttribute = true;

                    attributesToSkip.Add(r); // If the attribute has already been added then we don't need it to test it again
                }
            }
        }
        while (hasAddedAttribute); // Repeat if at least one new attribute was added in the previous loop
    }

    private void DefineElements()
    {
        foreach (Element e in Element.Elements.Values)
        {
            if (e.Assignable(this))
            {
                Info.AddElement(e.GetInstanceForRegion(this));
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

    public override bool IsWithinRegion(TerrainCell cell)
    {
        return cell.Region == this;
    }

    public override RectInt GetRectangle()
    {
        return _rect;
    }
}
