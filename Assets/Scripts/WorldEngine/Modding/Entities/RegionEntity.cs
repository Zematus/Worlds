using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RegionEntity : DelayedSetEntity<Region>
{
    public virtual Region Region
    {
        get => Setable;
        private set => Setable = value;
    }

    protected override object _reference => Region;

    public RegionEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public RegionEntity(
        ValueGetterMethod<Region> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public RegionEntity(
        TryRequestGenMethod<Region> tryRequestGenMethod, Context c, string id, IEntity parent)
        : base(tryRequestGenMethod, c, id, parent)
    {
    }

    public override string GetDebugString()
    {
        return "region:" + Region.Name.Text;
    }

    public override string GetFormattedString()
    {
        return Region.Name.BoldText;
    }
}
