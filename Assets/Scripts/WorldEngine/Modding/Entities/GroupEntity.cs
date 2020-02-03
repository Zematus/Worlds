using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupEntity : Entity
{
    private CellGroup _group;

    private CellEntity _cellEntity;
    private EntityAttribute _cellEntityAttribute;

    public GroupEntity()
    {
        _cellEntity = new CellEntity();
    }

    public override EntityAttribute GetAttribute(string attributeId, Expression[] arguments = null)
    {
        switch (attributeId)
        {
            case "cell":
                _cellEntityAttribute =
                    _cellEntityAttribute ?? new FixedEntityEntityAttribute(_cellEntity);
                return _cellEntityAttribute;
        }

        throw new System.ArgumentException("Group: Unable to find attribute: " + attributeId);
    }

    public void Set(CellGroup group)
    {
        _group = group;

        _cellEntity.Set(_group.Cell);
    }
}
