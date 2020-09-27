using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DelayedSetCellEntity : CellEntity
{
    private ValueGetterMethod<TerrainCell> _getterMethod;

    private TerrainCell _cell = null;

    public DelayedSetCellEntity(ValueGetterMethod<TerrainCell> getterMethod, Context c, string id)
        : base(c, id)
    {
        _getterMethod = getterMethod;
    }

    public void Reset()
    {
        _cell = null;
    }

    public override TerrainCell Cell
    {
        get
        {
            if (_cell == null)
            {
                _cell = _getterMethod();

                Set(_cell);
            }

            return _cell;
        }
    }
}
