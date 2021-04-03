using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RegionCollectionEntity : CollectionEntity<Region>
{
    private int _selectedRegionIndex = 0;

    private readonly List<RegionEntity>
        _regionEntitiesToSet = new List<RegionEntity>();

    public RegionCollectionEntity(
        CollectionGetterMethod<Region> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    protected override EntityAttribute GenerateRequestSelectionAttribute(IExpression[] arguments)
    {
        int index = _selectedRegionIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        if ((arguments == null) && (arguments.Length < 1))
        {
            throw new System.ArgumentException(
                "'request_selection' is missing 1 argument");
        }

        IValueExpression<ModText> textExpression =
            ValueExpressionBuilder.ValidateValueExpression<ModText>(arguments[0]);

        RegionEntity entity = new RegionEntity(
            (out DelayedSetEntityInputRequest<Region> request) =>
            {
                request = new RegionSelectionRequest(
                    Collection, textExpression.Value);
                return true;
            },
            Context,
            BuildAttributeId("selected_region_" + index));

        _regionEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    protected override EntityAttribute GenerateSelectRandomAttribute()
    {
        int index = _selectedRegionIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        RegionEntity entity = new RegionEntity(
            () => {
                int offset = iterOffset + Context.GetBaseOffset();
                return Collection.RandomSelect(Context.GetNextRandomInt, offset); 
            },
            Context,
            BuildAttributeId("selected_region_" + index));

        _regionEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    public override string GetDebugString()
    {
        return "region_collection";
    }

    protected override void ResetInternal()
    {
        if (_isReset) return;

        foreach (RegionEntity regionEntity in _regionEntitiesToSet)
        {
            regionEntity.Reset();
        }
    }
}
