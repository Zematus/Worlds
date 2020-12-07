using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Territory : ISynchronizable
{
    public List<WorldPosition> CellPositions;

    [XmlIgnore]
    public bool IsSelected = false;

    [XmlIgnore]
    public World World;

    [XmlIgnore]
    public Polity Polity;

    private HashSet<TerrainCell> _cells = new HashSet<TerrainCell>();

    private HashSet<TerrainCell> _innerBorderCells = new HashSet<TerrainCell>();

    private HashSet<TerrainCell> _cellsToAdd = new HashSet<TerrainCell>();
    private HashSet<TerrainCell> _cellsToRemove = new HashSet<TerrainCell>();

    private HashSet<Border> _outerBorders = new HashSet<Border>();
    private HashSet<Border> _newOuterBorders = new HashSet<Border>();
    private HashSet<TerrainCell> _outerBorderCellsToValidate = new HashSet<TerrainCell>();
    private HashSet<TerrainCell> _validatedOuterBorderCells = new HashSet<TerrainCell>();

    public Territory()
    {

    }

    public Territory(Polity polity)
    {
        World = polity.World;
        Polity = polity;
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

    public void InvalidateBorders(TerrainCell cell)
    {
        List<Border> bordersToRemove = null;

        foreach (Border border in _outerBorders)
        {
            if (border.HasCell(cell))
            {
                if (bordersToRemove == null)
                {
                    bordersToRemove = new List<Border>();
                }

                bordersToRemove.Add(border);
            }
        }

        if (bordersToRemove != null)
        {
            foreach (Border b in bordersToRemove)
            {
                _outerBorders.Remove(b);
            }
        }
    }

    public bool HasThisPolityProminence(TerrainCell cell)
    {
        return
            (cell.Group != null) &&
            (cell.Group.GetPolityProminence(Polity) != null);
    }

    public void TestOuterBorderCell(TerrainCell cell)
    {
        if (!HasThisPolityProminence(cell))
        {
            InvalidateBorders(cell);

            _outerBorderCellsToValidate.Add(cell);
            return;
        }
    }

    public void TestNeighborsForBorders(TerrainCell cell)
    {
        foreach (TerrainCell nCell in cell.NeighborList)
        {
            TestOuterBorderCell(nCell);
        }
    }

    public void SetCellToAdd(TerrainCell cell)
    {
        if ((cell.TerritoryToAddTo != null) &&
            (cell.TerritoryToAddTo != this))
        {
            cell.TerritoryToAddTo.RemoveCellToAdd(cell);
        }

        if (_cellsToRemove.Contains(cell))
        {
            _cellsToRemove.Remove(cell);
            return;
        }

        if (_cells.Contains(cell))
        {
            throw new System.Exception(
                "Trying to add cell that has already been added. Cell: " + cell.Position + " Polity.Id: " + Polity.Id);
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

    public void AddCells()
    {
        foreach (TerrainCell cell in _cellsToAdd)
        {
            cell.TerritoryToAddTo = null;
            AddCell(cell);
        }

        _cellsToAdd.Clear();
    }

    public void RemoveCells()
    {
        foreach (TerrainCell cell in _cellsToRemove)
        {
            RemoveCell(cell);
        }

        _cellsToRemove.Clear();
    }

    public bool IsPartOfOuterBorder(TerrainCell cell)
    {
        foreach (TerrainCell nCell in cell.NeighborList)
        {
            if (HasThisPolityProminence(nCell))
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

                if (HasThisPolityProminence(nCell))
                    continue;

                if (!IsPartOfOuterBorder(nCell))
                    continue;

                border.AddCell(nCell);
                cellsToExplore.Enqueue(nCell);
            }
        }

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

            _outerBorders.Add(border);
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
            if (border.TryGetEnclosedCellSet(
                _cells,
                out CellSet cellSet,
                CanAddCellToEnclosedArea))
                continue;
        }

        _newOuterBorders.Clear();
    }

    public void Update()
    {
        UpdateOuterBorders();

        AddEnclosedAreas();
    }

    private void AddCell(TerrainCell cell)
    {
        _cells.Add(cell);

        TestNeighborsForBorders(cell);

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

        Region cellRegion = cell.Region;

        if (cellRegion == null)
        {
            cellRegion = Region.TryGenerateRegion(cell, Polity.Culture.Language);

            if (cellRegion != null)
            {
                if (World.GetRegionInfo(cellRegion.Id) != null)
                {
                    throw new System.Exception("RegionInfo with Id " + cellRegion.Id + " already present");
                }

                World.AddRegionInfo(cellRegion.Info);
            }
            else
            {
                throw new System.Exception("No region could be generated");
            }
        }
    }

    private void RemoveCell(TerrainCell cell)
    {
        _cells.Remove(cell);

        TestOuterBorderCell(cell);

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
    }

    public void Synchronize()
    {
        CellPositions = new List<WorldPosition>(_cells.Count);

        foreach (TerrainCell cell in _cells)
        {
            CellPositions.Add(cell.Position);
        }
    }

    public void FinalizeLoad()
    {
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
}
