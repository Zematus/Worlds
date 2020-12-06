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

    private HashSet<TerrainCell> _borderCells = new HashSet<TerrainCell>();

    private HashSet<TerrainCell> _cellsToAdd = new HashSet<TerrainCell>();
    private HashSet<TerrainCell> _cellsToRemove = new HashSet<TerrainCell>();

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

    public bool IsPartOfBorder(TerrainCell cell)
    {
        return _borderCells.Contains(cell);
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

    public void Update()
    {
    }

    private void AddCell(TerrainCell cell)
    {
        _cells.Add(cell);

        cell.EncompassingTerritory = this;
        Manager.AddUpdatedCell(cell, CellUpdateType.Territory | CellUpdateType.Cluster, CellUpdateSubType.Membership);

        if (IsPartOfBorderInternal(cell))
        {
            _borderCells.Add(cell);
        }

        foreach (TerrainCell nCell in cell.NeighborList)
        {
            if (_borderCells.Contains(nCell))
            {
                if (!IsPartOfBorderInternal(nCell))
                {
                    _borderCells.Remove(nCell);
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

        cell.EncompassingTerritory = null;
        Manager.AddUpdatedCell(cell, CellUpdateType.Territory | CellUpdateType.Cluster, CellUpdateSubType.Membership);

        if (_borderCells.Contains(cell))
        {
            _borderCells.Remove(cell);
        }

        foreach (TerrainCell nCell in cell.NeighborList)
        {
            if (IsPartOfBorderInternal(nCell))
            {
                _borderCells.Add(nCell);
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
                    _borderCells.Add(cell);
                    break;
                }
            }
        }
    }
}
