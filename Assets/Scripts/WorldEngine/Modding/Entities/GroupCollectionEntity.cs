using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupCollectionEntity : CollectionEntity<CellGroup>
{
    private int _selectedGroupIndex = 0;

    private readonly List<GroupEntity>
        _groupEntitiesToSet = new List<GroupEntity>();

    public GroupCollectionEntity(Context c, string id)
        : base(c, id)
    {
    }

    public GroupCollectionEntity(
        CollectionGetterMethod<CellGroup> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    protected override EntityAttribute GenerateRequestSelectionAttribute(IExpression[] arguments)
    {
        int index = _selectedGroupIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        if ((arguments == null) && (arguments.Length < 1))
        {
            throw new System.ArgumentException(
                "'request_selection' is missing 1 argument");
        }

        IValueExpression<ModText> textExpression =
            ValueExpressionBuilder.ValidateValueExpression<ModText>(arguments[0]);

        GroupEntity entity = new GroupEntity(
            (out DelayedSetEntityInputRequest<CellGroup> request) =>
            {
                request = new GroupSelectionRequest(
                    Collection, textExpression.Value);
                return true;
            },
            Context,
            BuildAttributeId("selected_group_" + index));

        _groupEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    protected override EntityAttribute GenerateSelectRandomAttribute()
    {
        int index = _selectedGroupIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        GroupEntity entity = new GroupEntity(
            () => {
                int offset = iterOffset + Context.GetBaseOffset();
                return Collection.RandomSelect(Context.GetNextRandomInt, offset); 
            },
            Context,
            BuildAttributeId("selected_group_" + index));

        _groupEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    public override string GetDebugString()
    {
        return "group_collection";
    }

    protected override void ResetInternal()
    {
        if (_isReset) return;

        foreach (var entity in _groupEntitiesToSet)
        {
            entity.Reset();
        }
    }
}
