using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DelayedSetGroupEntity : GroupEntity
{
    private ValueGetterMethod<CellGroup> _getterMethod;

    private CellGroup _group = null;

    public DelayedSetGroupEntity(ValueGetterMethod<CellGroup> getterMethod, string id)
        : base(id)
    {
        _getterMethod = getterMethod;
    }

    public void Reset()
    {
        _group = null;
    }

    public override CellGroup Group
    {
        get
        {
            if (_group == null)
            {
                _group = _getterMethod();

                Set(_group);
            }

            return _group;
        }
    }
}
