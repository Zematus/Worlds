using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Territory : ISynchronizable, ICellSet
{
    public List<WorldPosition> CellPositions;
    public List<CellArea> EnclosedAreas;

    public List<RegionAccess> RegionAccesses;

    [XmlIgnore]
    public bool IsSelected = false;

    [XmlIgnore]
    public bool IsHovered = false;

    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public Polity Polity;

    private HashSet<TerrainCell> _cells = new HashSet<TerrainCell>();

    private HashSet<TerrainCell> _innerBorderCells = new HashSet<TerrainCell>();

    private HashSet<TerrainCell> _cellsToAdd = new HashSet<TerrainCell>();
    private HashSet<TerrainCell> _cellsToRemove = new HashSet<TerrainCell>();

    private HashSet<CellArea> _enclosedAreas = new HashSet<CellArea>();
    private HashSet<CellArea> _enclosedAreasToRemove = new HashSet<CellArea>();

    private HashSet<Border> _newOuterBorders = new HashSet<Border>();

    private HashSet<TerrainCell> _enclosedCells = new HashSet<TerrainCell>();

    private HashSet<TerrainCell> _outerBorderCellsToValidate =
        new HashSet<TerrainCell>();
    private HashSet<TerrainCell> _validatedOuterBorderCells =
        new HashSet<TerrainCell>();

    private Dictionary<Region, RegionAccess> _regionAccesses =
        new Dictionary<Region, RegionAccess>();

    public Territory()
    {
    }

    public Territory(Polity polity)
    {
        World = polity.World;
        Polity = polity;
    }

    public ICollection<Region> GetAccessibleRegions()
    {
        return _regionAccesses.Keys;
    }

    public ICollection<TerrainCell> GetCells()
    {
        return _cells;
    }

    private bool IsPartOfBorderInternal(TerrainCell cell)
    {
        if (!_cells.Contains(cell))
        {
            return false;
        }

        foreach (TerrainCell nCell in cell.NeighborList)
        {
            if (!_cells.Contains(nCell))
                return true;
        }

        return false;
    }

    public bool IsInside(TerrainCell cell)
    {
        return _cells.Contains(cell);
    }

    public bool IsPartOfInnerBorder(TerrainCell cell)
    {
        return _innerBorderCells.Contains(cell);
    }

    private void InvalidateEnclosedArea(TerrainCell cell)
    {
        foreach (CellArea area in _enclosedAreas)
        {
            if (_enclosedAreasToRemove.Contains(area))
                continue;

            if (area.Cells.Contains(cell))
            {
                _enclosedAreasToRemove.Add(area);
            }
        }
    }

    private void RemoveInvalidatedEnclosedAreas()
    {
        foreach (CellArea area in _enclosedAreasToRemove)
        {
            _enclosedAreas.Remove(area);

            foreach (TerrainCell cell in area.Cells)
            {
                _enclosedCells.Remove(cell);

                if (HasThisHighestPolityProminence(cell))
                    continue;

//#if DEBUG
//                if (cell.Position.Equals(6, 111))
//                {
//                    Debug.LogWarning("Debugging RemoveInvalidatedEnclosedAreas, cell: " + cell.Position + ", group: " +
//                        cell.Group + ", polity: " + Polity.Id);
//                }
//#endif

                RemoveCell(cell);

                // make sure we test the removed cells again as they might end up 
                // forming a new enclosed area by themselves
                _outerBorderCellsToValidate.Add(cell);
            }
        }

        _enclosedAreasToRemove.Clear();
    }

    private bool HasThisHighestPolityProminence(TerrainCell cell)
    {
        return cell.Group?.HighestPolityProminence?.Polity == Polity;
    }

    private void TestOuterBorderCell(TerrainCell cell)
    {
        if (!HasThisHighestPolityProminence(cell))
        {
            InvalidateEnclosedArea(cell);

            _outerBorderCellsToValidate.Add(cell);
        }
    }

    private void TestNeighborsForBorders(TerrainCell cell)
    {
        foreach (TerrainCell nCell in cell.NeighborList)
        {
            TestOuterBorderCell(nCell);
        }
    }

    public void SetCellToAdd(TerrainCell cell)
    {
        if (_cellsToRemove.Contains(cell))
        {
            // This cell is being removed
            _cellsToRemove.Remove(cell);
            return;
        }

        if (_enclosedCells.Contains(cell))
        {
            // This cell is part of an already enclosed piece of land. No need to
            // add it again
            return;
        }

        if (_cells.Contains(cell))
        {
            throw new System.Exception(
                "Trying to add cell that has already been added. Cell: " + cell.Position +
                " Polity.Id: " + Polity.Id);
        }

        cell.TerritoryToAddTo = this;
        _cellsToAdd.Add(cell);

        World.AddTerritoryToUpdate(this);
    }

    public bool TryRemoveCellToAdd(TerrainCell cell)
    {
        if (_cellsToAdd.Contains(cell))
        {
            cell.TerritoryToAddTo = null;
            _cellsToAdd.Remove(cell);

            return true;
        }

        return false;
    }

    public void SetCellToRemove(TerrainCell cell)
    {
        if (TryRemoveCellToAdd(cell))
            return;

        if (!_cells.Contains(cell))
        {
            throw new System.Exception(
                "Trying to remove cell that is not present in territory. Cell: " + cell.Position + " Polity.Id: " + Polity.Id);
        }

        _cellsToRemove.Add(cell);

        World.AddTerritoryToUpdate(this);
    }

    public void PrepareToAddNonEnclosedCell(TerrainCell cell)
    {
        cell.TerritoryToAddTo = null;

        _enclosedCells.Remove(cell);

        InvalidateEnclosedArea(cell);

        TestNeighborsForBorders(cell);
    }

    public void AddCells()
    {
        foreach (TerrainCell cell in _cellsToAdd)
        {
            PrepareToAddNonEnclosedCell(cell);

            if (!TryAddCell(cell))
            {
                throw new System.Exception($"Failed to add cell {cell.Position}");
            }
        }

        _cellsToAdd.Clear();
    }

    public void RemoveCells()
    {
        foreach (TerrainCell cell in _cellsToRemove)
        {
            // Test to invalidate any enclosed area it might 
            // have been part of
            TestOuterBorderCell(cell);

            // Also test neighbors
            TestNeighborsForBorders(cell);

            _enclosedCells.Remove(cell);

            RemoveCell(cell);
        }

        _cellsToRemove.Clear();
    }

    public bool IsPartOfOuterBorder(TerrainCell cell)
    {
        if (HasThisHighestPolityProminence(cell))
            return false;

        foreach (TerrainCell nCell in cell.NeighborList)
        {
            if (HasThisHighestPolityProminence(nCell))
            {
                return true;
            }
        }

        return false;
    }

    public Border BuildOuterBorder(TerrainCell startCell)
    {
        Border border = new Border(startCell.GetIndex(), startCell);

        Queue<TerrainCell> cellsToExplore = new Queue<TerrainCell>();

        cellsToExplore.Enqueue(startCell);
        _validatedOuterBorderCells.Add(startCell);

        while (cellsToExplore.Count > 0)
        {
            TerrainCell cell = cellsToExplore.Dequeue();

            foreach (TerrainCell nCell in cell.NonDiagonalNeighbors.Values)
            {
                if (_validatedOuterBorderCells.Contains(nCell))
                    continue;

                _validatedOuterBorderCells.Add(nCell);

                if (HasThisHighestPolityProminence(nCell))
                    continue;

                if (!IsPartOfOuterBorder(nCell))
                    continue;

                border.AddCell(nCell);
                cellsToExplore.Enqueue(nCell);
            }
        }

        border.Update();

        return border;
    }

    public void UpdateOuterBorders()
    {
        foreach (TerrainCell cell in _outerBorderCellsToValidate)
        {
            if (_validatedOuterBorderCells.Contains(cell))
                continue;

            if (!IsPartOfOuterBorder(cell))
                continue;

            Border border = BuildOuterBorder(cell);

            _newOuterBorders.Add(border);
        }

        _outerBorderCellsToValidate.Clear();
        _validatedOuterBorderCells.Clear();
    }

    private static bool CanAddCellToEnclosedArea(TerrainCell cell)
    {
        if (cell.IsLiquidSea)
            return false;

        return cell.Group == null;

        //if (cell.Group == null)
        //    return true;

        //return cell.Group.TotalPolityProminenceValue <= 0;
    }

    public void AddEnclosedAreas()
    {
        foreach (Border border in _newOuterBorders)
        {
            if (!border.TryGetEnclosedCellSet(
                _cells,
                out CellSet enclosedSet,
                CanAddCellToEnclosedArea))
                continue;

            _enclosedAreas.Add(enclosedSet.GetArea());

            List<TerrainCell> cellsToTryToAddAgain = new List<TerrainCell>();

            foreach (var cell in enclosedSet.Cells)
            {
                _enclosedCells.Add(cell);

                if (!TryAddCell(cell))
                {
                    // Sometimes a land region can't be generated from this cell (rivers),
                    // so we can delay adding this cell until a it has already been added
                    // to a region
                    cellsToTryToAddAgain.Add(cell);
                }
            }

            foreach (var cell in cellsToTryToAddAgain)
            {
                if (!TryAddCell(cell))
                {
                    // No land region could be generated that encompasses this cell, so we can't
                    // add it to the territory.
                    Debug.LogWarning($"Failed to add enclosed cell {cell.Position} after second attempt");
                }
            }
        }

        _newOuterBorders.Clear();
    }

    public void Update()
    {
        RemoveInvalidatedEnclosedAreas();

        UpdateOuterBorders();

        AddEnclosedAreas();
    }

    private bool TryAddCell(TerrainCell cell)
    {
        if (!_cells.Add(cell))
        {
            // the cell has already been added, there's nothing else that needs to be done
            return true;
        }

        Region region = cell.GetRegion(Polity.Culture.Language);

        if (region == null)
        {
            return false;
        }

        cell.EncompassingTerritory = this;
        Manager.AddUpdatedCell(cell, CellUpdateType.Territory | CellUpdateType.Cluster, CellUpdateSubType.Membership);

        if (IsPartOfBorderInternal(cell))
        {
            _innerBorderCells.Add(cell);
        }

        foreach (TerrainCell nCell in cell.NeighborList)
        {
            if (_innerBorderCells.Contains(nCell))
            {
                if (!IsPartOfBorderInternal(nCell))
                {
                    _innerBorderCells.Remove(nCell);
                    Manager.AddUpdatedCell(nCell, CellUpdateType.Territory, CellUpdateSubType.Membership);
                }
            }
        }

        IncreaseAccessToRegion(region);

        foreach (TerrainCell nCell in cell.NeighborList)
        {
            if (nCell.IsAllWater)
                continue;

            region = nCell.GetRegion(Polity.Culture.Language);

            if (region != null)
            {
                IncreaseAccessToRegion(region);
            }
        }

        return true;
    }

    private void RemoveCell(TerrainCell cell)
    {

//#if DEBUG
//        if (cell.Position.Equals(395, 134))
//        {
//            Debug.LogWarning("Debugging RemoveCell, cell: " + cell.Position + ", group: " +
//                cell.Group + ", polity: " + Polity.Id);
//        }
//#endif

        if (!_cells.Remove(cell))
        {
            // the cell has already been removed, there's nothing else that needs to be done
            return;
        }

        cell.EncompassingTerritory = null;
        Manager.AddUpdatedCell(cell, CellUpdateType.Territory | CellUpdateType.Cluster, CellUpdateSubType.Membership);

        if (_innerBorderCells.Contains(cell))
        {
            _innerBorderCells.Remove(cell);
        }

        foreach (TerrainCell nCell in cell.NeighborList)
        {
            if (IsPartOfBorderInternal(nCell))
            {
                _innerBorderCells.Add(nCell);
                Manager.AddUpdatedCell(nCell, CellUpdateType.Territory, CellUpdateSubType.Membership);
            }
        }

        DecreaseAccessToRegion(cell.Region);

        foreach (TerrainCell nCell in cell.NeighborList)
        {
            if (nCell.IsAllWater)
                continue;

            if (nCell.Region != null)
            {
                DecreaseAccessToRegion(nCell.Region);
            }
        }
    }

    public void Synchronize()
    {
        CellPositions = new List<WorldPosition>(_cells.Count);

        foreach (TerrainCell cell in _cells)
        {
            CellPositions.Add(cell.Position);
        }

        EnclosedAreas = new List<CellArea>(_enclosedAreas);

        foreach (CellArea area in EnclosedAreas)
        {
            area.Synchronize();
        }

        RegionAccesses = new List<RegionAccess>(_regionAccesses.Values);
    }

    public void FinalizeLoad()
    {
        foreach (RegionAccess access in RegionAccesses)
        {
            access.Region = World.GetRegionInfo(access.RegionId).Region;

            _regionAccesses.Add(access.Region, access);
        }

        foreach (CellArea area in EnclosedAreas)
        {
            area.World = World;
            area.FinalizeLoad();

            foreach (TerrainCell cell in area.Cells)
            {
                _enclosedCells.Add(cell);
            }

            _enclosedAreas.Add(area);
        }

        foreach (WorldPosition position in CellPositions)
        {
            TerrainCell cell = World.GetCell(position);

            if (cell == null)
            {
                throw new System.Exception("Cell missing at position " + position.Longitude + "," + position.Latitude);
            }

            _cells.Add(cell);

            cell.EncompassingTerritory = this;
        }

        foreach (TerrainCell cell in _cells)
        {
            foreach (TerrainCell nCell in cell.NeighborList)
            {
                if (!_cells.Contains(nCell))
                {
                    _innerBorderCells.Add(cell);
                    break;
                }
            }
        }
    }

    private void IncreaseAccessToRegion(Region region)
    {
        if (_regionAccesses.ContainsKey(region))
        {
            _regionAccesses[region].Count++;
        }
        else
        {
            _regionAccesses[region] = new RegionAccess()
            {
                Region = region,
                RegionId = region.Id,
                Count = 1
            };

            Polity.AccessibleRegionsUpdate();
        }
    }

    private void DecreaseAccessToRegion(Region region)
    {
        if (!_regionAccesses.TryGetValue(region, out var regionAccess))
        {
            throw new System.Exception(
                "Region was not accessible. Polity: " +
                Polity.Id + ", Region: " + region.Id);
        }

        if (regionAccess.Count == 1)
        {
            _regionAccesses.Remove(region);

            Polity.AccessibleRegionsUpdate();
            return;
        }

        _regionAccesses[region].Count--;
    }

    public RectInt GetBoundingRectangle()
    {
        if (_cells.Count == 0)
        {
            return default;
        }

        bool first = true;
        int xMin = 0;
        int xMax = 0;
        int yMin = 0;
        int yMax = 0;

        foreach (TerrainCell cell in _cells)
        {
            if (first)
            {
                xMin = cell.Longitude;
                xMax = cell.Longitude;
                yMin = cell.Latitude;
                yMax = cell.Latitude;

                first = false;
                continue;
            }

            int longitude = cell.Longitude;

            // this will make sure that we get the smallest rect that encompasses the territory
            // regardless of the territory wrapping around the horizontal edges of the map
            if (longitude < xMax)
            {
                if ((longitude + World.Width - xMax) < (xMax - longitude))
                {
                    longitude += World.Width;
                }
            }
            else
            {
                if ((xMax + World.Width - longitude) < (longitude - xMax))
                {
                    xMax += World.Width;
                }
            }

            xMin = Mathf.Min(longitude, xMin);
            xMax = Mathf.Max(longitude, xMax);
            yMin = Mathf.Min(cell.Latitude, yMin);
            yMax = Mathf.Max(cell.Latitude, yMax);
        }

        return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
    }
}
