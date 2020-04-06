using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupEntity : Entity
{
    public const string CellAttributeId = "cell";

    public CellGroup Group { get; private set; }

    private CellEntity _cellEntity;
    private EntityAttribute _cellEntityAttribute;

    public GroupEntity(string id) : base(id)
    {
        _cellEntity = new CellEntity(BuildInternalEntityId(CellAttributeId));
    }

    protected override object _reference => Group;

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case CellAttributeId:
                _cellEntityAttribute =
                    _cellEntityAttribute ??
                    new FixedValueEntityAttribute<Entity>(_cellEntity, CellAttributeId, this);
                return _cellEntityAttribute;
        }

        throw new System.ArgumentException("Group: Unable to find attribute: " + attributeId);
    }

    public override string GetFormattedString()
    {
        return Group.Cell.Position.ToString();
    }

    public override void Set(object o)
    {
        if ((Group = o as CellGroup) == null)
        {
            throw new System.Exception("Entity reference is not of type " + typeof(CellGroup));
        }

        _cellEntity.Set(Group.Cell);
    }
}
