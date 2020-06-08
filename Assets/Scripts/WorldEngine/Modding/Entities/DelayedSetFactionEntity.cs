using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DelayedSetFactionEntity : FactionEntity
{
    private ValueGetterMethod<Faction> _getterMethod;

    private Faction _faction = null;

    public DelayedSetFactionEntity(ValueGetterMethod<Faction> getterMethod, string id)
        : base(id)
    {
        _getterMethod = getterMethod;
    }

    public void Reset()
    {
        _faction = null;

        ResetInternal();
    }

    public override Faction Faction
    {
        get
        {
            if (_faction == null)
            {
                _faction = _getterMethod();

                Set(_faction);
            }

            return _faction;
        }
    }
}
