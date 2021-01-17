using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class EntityValueEntityAttribute : FixedValueEntityAttribute<IEntity>
{
    public override bool RequiresInput => _attrValue.RequiresInput;

    public EntityValueEntityAttribute(
        IEntity entity, string id, Entity parent)
        : base(entity, id, parent)
    {
    }

    public override bool TryGetRequest(out InputRequest request) =>
        _attrValue.TryGetRequest(out request);
}
