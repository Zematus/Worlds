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

    public RegionEntity(Context c, string id) : base(c, id)
    {
    }

    public RegionEntity(
        ValueGetterMethod<Region> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    public RegionEntity(
        TryRequestGenMethod<Region> tryRequestGenMethod, Context c, string id)
        : base(tryRequestGenMethod, c, id)
    {
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
        }

        throw new System.ArgumentException(Id + ": Unable to find attribute: " + attributeId);
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
