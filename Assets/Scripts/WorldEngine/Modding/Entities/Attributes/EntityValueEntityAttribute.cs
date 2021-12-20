using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class EntityValueEntityAttribute : FixedValueEntityAttribute<IEntity>
{
    public override bool RequiresInput => base.RequiresInput || _attrValue.RequiresInput;

    public EntityValueEntityAttribute(
        IEntity entity, string id, IEntity parent)
        : base(entity, id, parent)
    {
    }

    public override bool TryGetRequest(out InputRequest request)
    {
        if (base.TryGetRequest(out request))
            return true;

        return _attrValue.TryGetRequest(out request);
    }
}
