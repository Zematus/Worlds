using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class CulturalEntryModifiableContainerEntity : EntryModifiableContainerEntity<Culture>, ICulturalEntryContainerEntity
{
    public virtual Culture Culture
    {
        get => Setable;
        private set => Setable = value;
    }

    protected override object _reference => Culture;

    public CulturalEntryModifiableContainerEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public CulturalEntryModifiableContainerEntity(
        ValueGetterMethod<Culture> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }
}
