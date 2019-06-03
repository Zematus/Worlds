using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public abstract class BrushAction : EditorAction
{
    public abstract void AddCellBeforeModification(TerrainCell cellBefore);
    public abstract void AddCellAfterModification(TerrainCell cellAfter);
    public abstract void FinalizeCellModifications();
}
