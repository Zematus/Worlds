using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DelayedSetPolityEntity : PolityEntity
{
    private ValueGetterMethod<Polity> _getterMethod;

    private Polity _polity = null;

    public DelayedSetPolityEntity(ValueGetterMethod<Polity> getterMethod, string id)
        : base(id)
    {
        _getterMethod = getterMethod;
    }

    public void Reset()
    {
        _polity = null;

        ResetInternal();
    }

    public override Polity Polity
    {
        get
        {
            if (_polity == null)
            {
                _polity = _getterMethod();

                Set(_polity);
            }

            return _polity;
        }
    }
}
