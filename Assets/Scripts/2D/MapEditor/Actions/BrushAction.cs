using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class BrushAction : EditorAction
{
    private struct TerrainCellAlterationPair
    {
        public TerrainCellAlteration Before;
        public TerrainCellAlteration After;
    }

    private Dictionary<WorldPosition, TerrainCellAlterationPair> _alterationPairs = new Dictionary<WorldPosition, TerrainCellAlterationPair>();

    public void AddAlterationPair(TerrainCellAlteration before, TerrainCellAlteration after)
    {
        TerrainCellAlterationPair pair;

        if (!_alterationPairs.TryGetValue(before.Position, out pair))
        {
            pair = new TerrainCellAlterationPair { Before = before };

            _alterationPairs.Add(before.Position, pair);
        }

        pair.After = after;
    }

    public override void Do()
    {
    }

    public override void Undo()
    {
    }
}
