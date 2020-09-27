using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupEntity : Entity
{
    public const string CellAttributeId = "cell";

    public virtual CellGroup Group { get; private set; }

    private DelayedSetCellEntity _cellEntity = null;

    private bool _alreadyReset = false;

    public GroupEntity(Context c, string id) : base(c, id)
    {
    }

    public EntityAttribute GetCellAttribute()
    {
        _cellEntity =
            _cellEntity ?? new DelayedSetCellEntity(
                GetCell,
                Context,
                BuildAttributeId(CellAttributeId));

        return _cellEntity.GetThisEntityAttribute(this);
    }

    protected override object _reference => Group;

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case CellAttributeId:
                return GetCellAttribute();
        }

        throw new System.ArgumentException("Group: Unable to find attribute: " + attributeId);
    }

    public override string GetDebugString()
    {
        return "group:" + Group.Cell.Position.ToString();
    }

    public override string GetFormattedString()
    {
        return Group.Cell.Position.ToString().ToBoldFormat();
    }

    public void Set(CellGroup g)
    {
        Group = g;

        ResetInternal();

        _alreadyReset = false;
    }

    protected void ResetInternal()
    {
        if (_alreadyReset)
        {
            return;
        }

        _cellEntity?.Reset();

        _alreadyReset = true;
    }

    public TerrainCell GetCell() => Group.Cell;

    public override void Set(object o)
    {
        if (o is GroupEntity e)
        {
            Set(e.Group);
        }
        else if (o is CellGroup g)
        {
            Set(g);
        }
        else
        {
            throw new System.ArgumentException("Unexpected type: " + o.GetType());
        }
    }
}
