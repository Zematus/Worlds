using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Territory : ISynchronizable, ICellSet
{
    public enum FilterType
    {
        None,
        Core,
        Selectable,
        Involved
    }

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

    [XmlIgnore]
    public FilterType SelectionFilterType = FilterType.None;

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

    private Dictionary<int, int> _longitudes = new Dictionary<int, int>();
    private Dictionary<int, int> _latitudes = new Dictionary<int, int>();

    private int _leftmost = -1;
    private int _rightmost = -1;
    private int _top = -1;
    private int _bottom = -1;

    private bool _validLeftmost = false;
    private bool _validRightmost = false;
    private bool _validTop = false;
    private bool _validBottom = false;

    private void AddLatitude(int latitude)
    {
        if (_latitudes.ContainsKey(latitude))
        {
            _latitudes[latitude]++;
        }
        else
        {
            _latitudes[latitude] = 1;
        }

        if (_top == -1)
        {
            _top = latitude;
            _validTop = true;
        }

        if (_bottom == -1)
        {
            _bottom = latitude;
            _validBottom = true;
        }

        if ((latitude - _top) > 0)
        {
            _top = latitude;
            _validTop = true;
        }

        if ((latitude - _bottom) < 0)
        {
            _bottom = latitude;
            _validBottom = true;
        }
    }

    private void RemoveLatitude(int latitude)
    {
        if (!_latitudes.ContainsKey(latitude))
        {
            throw new System.Exception($"Tryin to remove missing latitude: {latitude}, polity: {Polity.Id}");
        }

        _latitudes[latitude]--;

        if (_latitudes[latitude] == 0)
        {
            _latitudes.Remove(latitude);

            if (latitude == _top)
                _validTop = false;

            if (latitude == _bottom)
                _validBottom = false;
        }
    }

    private void UpdateLatitudeEdges()
    {
        if (_validBottom && _validTop)
            return;

        _top = 0;
        _bottom = 0;

        bool first = true;
        foreach (int latitude in _latitudes.Keys)
        {
            if (first)
            {
                _top = latitude;
                _bottom = latitude;
                first = false;
                continue;
            }

            if (_bottom > latitude)
            {
                _bottom = latitude;
            }

            if (_top < latitude)
            {
                _top = latitude;
            }
        }

        _validBottom = true;
        _validTop = true;
    }

    private void AddLongitude(int longitude)
    {
        if (_longitudes.ContainsKey(longitude))
        {
            _longitudes[longitude]++;
        }
        else
        {
            _longitudes[longitude] = 1;
        }

        if (_leftmost == -1)
        {
            _leftmost = longitude;
            _validLeftmost = true;
        }
        if (_rightmost == -1)
        {
            _rightmost = longitude;
            _validRightmost = true;
        }

        int diffLeft = longitude - _leftmost;
        int diffRight = longitude - _rightmost;

        if (diffLeft < 0)
        {
            if ((diffRight + diffLeft + Manager.WorldWidth) < 0)
            {
                // Wrapped around, the cell would be closer to the 
                // rightmost cell and would be even more rightmost
                _rightmost = longitude;
                _validRightmost = true;
            }
            else
            {
                _leftmost = longitude;
                _validLeftmost = true;
            }
        }

        if (diffRight > 0)
        {
            if ((diffRight + diffLeft - Manager.WorldWidth) > 0)
            {
                // Wrapped around, the cell would be closer to the 
                // leftmost cell and would be even more leftmost
                _leftmost = longitude;
                _validLeftmost = true;
            }
            else
            {
                _rightmost = longitude;
                _validRightmost = true;
            }
        }
    }

    private void RemoveLongitude(int longitude)
    {
        if (!_longitudes.ContainsKey(longitude))
        {
            throw new System.Exception($"Tryin to remove missing longitude: {longitude}, polity: {Polity.Id}");
        }

        _longitudes[longitude]--;

        if (_longitudes[longitude] == 0)
        {
            _longitudes.Remove(longitude);

            if (longitude == _leftmost)
                _validLeftmost = false;

            if (longitude == _rightmost)
                _validRightmost = false;
        }
    }

    private void UpdateLongitudeEdges()
    {
        if (_validLeftmost && _validRightmost)
            return;

        _leftmost = 0;
        _rightmost = 0;
        bool first = true;
        foreach (int longitude in _longitudes.Keys)
        {
            if (first)
            {
                _leftmost = longitude;
                _rightmost = longitude;
                first = false;
                continue;
            }

            int diffLeft = longitude - _leftmost;
            int diffRight = longitude - _rightmost;

            if (diffLeft < 0)
            {
                if ((diffRight + diffLeft + Manager.WorldWidth) < 0)
                {
                    // Wrap around
                    _rightmost = longitude;
                }
                else
                {
                    _leftmost = longitude;
                }
            }

            if (diffRight > 0)
            {
                if ((diffRight + diffLeft - Manager.WorldWidth) > 0)
                {
                    // Wrap around
                    _leftmost = longitude;
                }
                else
                {
                    _rightmost = longitude;
                }
            }
        }

        _validLeftmost = true;
        _validRightmost = true;
    }

    private bool AddCellInternal(TerrainCell cell)
    {
        if (!_cells.Add(cell))
            return false;

        AddLatitude(cell.Latitude);
        AddLongitude(cell.Longitude);

        return true;
    }

    private bool RemoveCellInternal(TerrainCell cell)
    {
        if (!_cells.Remove(cell))
        {
            return false;
        }

        RemoveLongitude(cell.Longitude);
        RemoveLatitude(cell.Latitude);

        return true;
    }

    private bool TryAddCell(TerrainCell cell)
    {
        if (!AddCellInternal(cell))
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
        if (!RemoveCellInternal(cell))
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

            AddCellInternal(cell);

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

        UpdateLongitudeEdges();
        UpdateLatitudeEdges();

        int xMin = _leftmost;
        int xMax = _rightmost;
        int yMin = _bottom;
        int yMax = _top;

        // this makes sure the rectagle can correctly wrap around the vertical edges of the map if needed
        if (xMax < xMin)
        {
            xMax += World.Width;
        }

        return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
    }
}
