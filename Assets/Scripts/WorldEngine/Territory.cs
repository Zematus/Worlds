using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Territory : ISynchronizable, ICellCollectionGetter
{
    public List<WorldPosition> CellPositions;
    public List<CellArea> EnclosedAreas;

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

    private HashSet<TerrainCell> _outerBorderCellsToValidate = new HashSet<TerrainCell>();
    private HashSet<TerrainCell> _validatedOuterBorderCells = new HashSet<TerrainCell>();

    private Dictionary<Region, int> _regionAccessCounts = new Dictionary<Region, int>();

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
        return _regionAccessCounts.Keys;
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
        return
            (cell.Group != null) &&
            (cell.Group.HighestPolityProminence != null) &&
            (cell.Group.HighestPolityProminence.Polity == Polity);
    }

    private void TestOuterBorderCell(TerrainCell cell)
    {
        if (!HasThisHighestPolityProminence(cell))
        {
            InvalidateEnclosedArea(cell);

//#if DEBUG
//            if (cell.Position.Equals(395, 134))
//            {
//                //if (debugCounter2 >= 90)
//                //{
//                    Debug.LogWarning("Debugging TestOuterBorderCell on cell " + cell.Position +
//                        ", attempt: " + debugCounter2);
//                //}

//                debugCounter2++;
//            }
//#endif

            _outerBorderCellsToValidate.Add(cell);
            return;
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

//#if DEBUG
//        if (cell.Position.Equals(6, 111))
//        {
//            Debug.LogWarning("Debugging SetCellToAdd, cell: " + cell.Position + ", group: "+
//                cell.Group + ", polity: " + Polity.Id);
//        }
//#endif

        if (_cellsToRemove.Contains(cell))
        {
            // This cell is part of an already enclosed piece of land. No need to add again
            _cellsToRemove.Remove(cell);
            return;
        }

        if (_enclosedCells.Contains(cell))
        {
            // This cell is part of an already enclosed piece of land. No need to
            // add it again
            return;
        }

        if ((cell.TerritoryToAddTo != null) &&
            (cell.TerritoryToAddTo != this))
        {
            //// If this cell was to be added to another territory, override that
            //cell.TerritoryToAddTo.RemoveCellToAdd(cell);

            // We should avoid this scenario
            throw new System.Exception(
                "We are already attempting to add this cell " + cell.Position +
                " to the territory of polity " + cell.TerritoryToAddTo.Polity.Id);
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

    public void RemoveCellToAdd(TerrainCell cell)
    {
        cell.TerritoryToAddTo = null;
        _cellsToAdd.Remove(cell);
    }

    public void SetCellToRemove(TerrainCell cell)
    {

//#if DEBUG
//        if (cell.Position.Equals(6, 111))
//        {
//            Debug.LogWarning("Debugging SetCellToRemove, cell: " + cell.Position + ", group: " +
//                cell.Group + ", polity: " + Polity.Id);
//        }
//#endif

        if (_cellsToAdd.Contains(cell))
        {
            RemoveCellToAdd(cell);
            return;
        }

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

            AddCell(cell);
        }

        _cellsToAdd.Clear();
    }

    public void RemoveCells()
    {
        foreach (TerrainCell cell in _cellsToRemove)
        {
            TestOuterBorderCell(cell);

            _enclosedCells.Remove(cell);

            RemoveCell(cell);
        }

        _cellsToRemove.Clear();
    }

    public bool IsPartOfOuterBorder(TerrainCell cell)
    {
        foreach (TerrainCell nCell in cell.NeighborList)
        {
            if (HasThisHighestPolityProminence(nCell))
            {
                return true;
            }
        }

        return false;
    }

//#if DEBUG
//    private int debugCounter1 = 1;
//    private int debugCounter2 = 1;
//#endif

    public Border BuildOuterBorder(TerrainCell startCell)
    {
        Border border = new Border(startCell.GetIndex(), startCell);

        Queue<TerrainCell> cellsToExplore = new Queue<TerrainCell>();

        cellsToExplore.Enqueue(startCell);
        _validatedOuterBorderCells.Add(startCell);

        while (cellsToExplore.Count > 0)
        {
            TerrainCell cell = cellsToExplore.Dequeue();

//#if DEBUG
//            if (cell.Position.Equals(395, 134))
//            {
//                if (debugCounter1 >= 90)
//                {
//                    Debug.LogWarning("Debugging BuildOuterBorder on cell " + cell.Position +
//                        ", attempt: " + debugCounter1);
//                }

//                debugCounter1++;
//            }
//#endif

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

            foreach (TerrainCell cell in enclosedSet.Cells)
            {
                _enclosedCells.Add(cell);

//#if DEBUG
//                if (cell.Position.Equals(6, 111))
//                {
//                    Debug.LogWarning("Debugging AddEnclosedAreas, cell: " + cell.Position + ", group: " +
//                        cell.Group + ", polity: " + Polity.Id);
//                }
//#endif

                AddCell(cell);
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

    private void AddCell(TerrainCell cell)
    {

//#if DEBUG
//        if (cell.Position.Equals(6, 111))
//        {
//            Debug.LogWarning("Debugging AddCell, cell: " + cell.Position + ", group: " +
//                cell.Group + ", polity: " + Polity.Id);
//        }
//#endif

        if (!_cells.Add(cell))
        {
            // the cell has already been added, there's nothing else that needs to be done
            return;
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

        Region region = cell.GetRegion(Polity.Culture.Language);

        if (region == null)
        {
            throw new System.Exception(
                "Unable to generate region for cell " + cell.Position);
        }

        IncreaseAccessToRegion(region);

        foreach (TerrainCell nCell in cell.NeighborList)
        {
            region = nCell.GetRegion(Polity.Culture.Language);

            if (region != null)
            {
                IncreaseAccessToRegion(region);
            }
        }
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
    }

    public void FinalizeLoad()
    {
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
        if (_regionAccessCounts.ContainsKey(region))
        {
            _regionAccessCounts[region]++;
        }
        else
        {
            _regionAccessCounts[region] = 1;

            Polity.AccessibleRegionsUpdate();
        }
    }

    private void DecreaseAccessToRegion(Region region)
    {
        if (!_regionAccessCounts.TryGetValue(region, out int count))
        {
            throw new System.Exception("Region was not accessible");
        }

        if (count == 1)
        {
            _regionAccessCounts.Remove(region);

            Polity.AccessibleRegionsUpdate();
            return;
        }

        _regionAccessCounts[region]--;
    }
}
