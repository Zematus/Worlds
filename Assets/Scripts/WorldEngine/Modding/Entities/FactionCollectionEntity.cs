using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FactionCollectionEntity : CollectionEntity<Faction>
{
    private int _selectedFactionIndex = 0;

    private readonly List<FactionEntity>
        _factionEntitiesToSet = new List<FactionEntity>();

    public FactionCollectionEntity(Context c, string id)
        : base(c, id)
    {
    }

    public FactionCollectionEntity(
        CollectionGetterMethod<Faction> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    protected override EntityAttribute GenerateRequestSelectionAttribute(IExpression[] arguments)
    {
        int index = _selectedFactionIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        if ((arguments == null) && (arguments.Length < 1))
        {
            throw new System.ArgumentException(
                "'request_selection' is missing 1 argument");
        }

        IValueExpression<ModText> textExpression =
            ValueExpressionBuilder.ValidateValueExpression<ModText>(arguments[0]);

        FactionEntity entity = new FactionEntity(
            (out DelayedSetEntityInputRequest<Faction> request) =>
            {
                request = new FactionSelectionRequest(Collection);
                return true;
            },
            Context,
            BuildAttributeId("selected_faction_" + index));

        _factionEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    protected override EntityAttribute GenerateSelectRandomAttribute()
    {
        int index = _selectedFactionIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        FactionEntity entity = new FactionEntity(
            () => {
                int offset = iterOffset + Context.GetBaseOffset();
                return Collection.RandomSelect(Context.GetNextRandomInt, offset); 
            },
            Context,
            BuildAttributeId("selected_faction_" + index));

        _factionEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    public override string GetDebugString()
    {
        return "faction_collection";
    }

    protected override void ResetInternal()
    {
        if (_isReset) return;

        foreach (var entity in _factionEntitiesToSet)
        {
            entity.Reset();
        }
    }
}
