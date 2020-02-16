using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupEntity : Entity
{
    public const string CellAttributeId = "cell";

    private CellGroup _group;

    private CellEntity _cellEntity = new CellEntity(CellAttributeId);
    private EntityAttribute _cellEntityAttribute;

    public GroupEntity(string id) : base(id)
    {
    }

    protected override object _reference => _group;

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case CellAttributeId:
                _cellEntityAttribute =
                    _cellEntityAttribute ??
                    new FixedEntityEntityAttribute(_cellEntity, CellAttributeId, this);
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
