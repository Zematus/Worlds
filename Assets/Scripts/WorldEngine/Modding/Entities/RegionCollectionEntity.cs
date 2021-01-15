using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RegionCollectionEntity : CollectionEntity<Region>
{
    public RegionCollectionEntity(Context c, string id) : base(c, id)
    {
    }

    public RegionCollectionEntity(
        CollectionGetterMethod<Region> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
        }

        throw new System.ArgumentException("Agent: Unable to find attribute: " + attributeId);
    }

    public override string GetDebugString()
    {
        return "region_collection";
    }
}
