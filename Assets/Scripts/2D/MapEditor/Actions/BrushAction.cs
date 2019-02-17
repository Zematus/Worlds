using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class BrushAction : EditorAction
{
    private class TerrainCellAlterationPair
    {
        public TerrainCellAlteration Before;
        public TerrainCellAlteration After;
    }

    private HashSet<TerrainCell> _cellsBeingModified = new HashSet<TerrainCell>();

    private Dictionary<WorldPosition, TerrainCellAlterationPair> _alterationPairs = 
        new Dictionary<WorldPosition, TerrainCellAlterationPair>();

    public void AddCellBeforeAlteration(TerrainCell cellBefore)
    {
        TerrainCellAlterationPair pair;

        if (!_alterationPairs.TryGetValue(cellBefore.Position, out pair))
        {
            pair = new TerrainCellAlterationPair { Before = cellBefore.GetAlteration(true) };

            _alterationPairs.Add(cellBefore.Position, pair);
        }
    }

    public void AddCellAfterAlteration(TerrainCell cellAfter)
    {
        _cellsBeingModified.Add(cellAfter);
    }

    public void FinalizeCellAlterations()
    {
        foreach (TerrainCell cell in _cellsBeingModified)
        {
            _alterationPairs[cell.Position].After = cell.GetAlteration(true);
        }
    }

    public override void Do()
    {
        foreach (TerrainCellAlterationPair pair in _alterationPairs.Values)
        {
            Manager.CurrentWorld.SetTerrainCellAlterationAndFinishRegenCell(pair.After);
        }
    }

    public override void Undo()
    {
        foreach (TerrainCellAlterationPair pair in _alterationPairs.Values)
        {
            Manager.CurrentWorld.SetTerrainCellAlterationAndFinishRegenCell(pair.Before);
        }
    }
}
