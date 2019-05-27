using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class LayerBrushAction : BrushAction
{
    public string LayerId;

    private class LayerOffsetPair
    {
        public WorldPosition Position;

        public CellLayerData Before;
        public CellLayerData After;
    }

    private HashSet<TerrainCell> _cellsBeingModified = new HashSet<TerrainCell>();

    private Dictionary<WorldPosition, LayerOffsetPair> _layerOffsetPairs = 
        new Dictionary<WorldPosition, LayerOffsetPair>();

    public LayerBrushAction(string layerId)
    {
        LayerId = layerId;
    }

    public override void AddCellBeforeModification(TerrainCell cellBefore)
    {
        LayerOffsetPair pair;

        if (!_layerOffsetPairs.TryGetValue(cellBefore.Position, out pair))
        {
            CellLayerData data = null;
            CellLayerData cellData = cellBefore.GetLayerData(LayerId);

            if (cellData != null)
            {
                data = new CellLayerData(cellData);
            }

            pair = new LayerOffsetPair {
                Position = cellBefore.Position,
                Before = data
            };

            _layerOffsetPairs.Add(cellBefore.Position, pair);
        }
    }

    public override void AddCellAfterModification(TerrainCell cellAfter)
    {
        _cellsBeingModified.Add(cellAfter);
    }

    public override void FinalizeCellModifications()
    {
        foreach (TerrainCell cell in _cellsBeingModified)
        {
            _layerOffsetPairs[cell.Position].After = new CellLayerData(cell.GetLayerData(LayerId));
        }
    }

    public override void Do()
    {
        foreach (LayerOffsetPair pair in _layerOffsetPairs.Values)
        {
            Manager.CurrentWorld.SetTerrainCellLayerDataAndFinishRegenCell(pair.Position, LayerId, pair.After);
        }
    }

    public override void Undo()
    {
        foreach (LayerOffsetPair pair in _layerOffsetPairs.Values)
        {
            Manager.CurrentWorld.SetTerrainCellLayerDataAndFinishRegenCell(pair.Position, LayerId, pair.Before);
        }
    }
}
