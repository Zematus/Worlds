using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupEntity : CulturalEntity<CellGroup>
{
    public const string CellAttributeId = "cell";
    public const string ProminenceValueAttributeId = "prominence_value";
    public const string GetCoreDistanceAttributeId = "get_core_distance";
    public const string MostProminentPolityAttributeId = "most_prominent_polity";
    public const string PresentPolitiesAttributeId = "present_polities";
    public const string ClosestFactionsAttributeId = "closest_factions";
    public const string NavigationRangeAttributeId = "navigation_range";
    public const string ArabilityModifierAttributeId = "arability_modifier";
    public const string AccessibilityModifierAttributeId = "accessibility_modifier";
    public const string PropertiesAttributeId = "properties";
    public const string PopulationAttributeId = "population";

    public virtual CellGroup Group
    {
        get => Setable;
        private set => Setable = value;
    }

    private ValueGetterSetterEntityAttribute<float> _navigationRangeAttribute;
    private ValueGetterSetterEntityAttribute<float> _arabilityModifierAttribute;
    private ValueGetterSetterEntityAttribute<float> _accessibilityModifierAttribute;
    private ValueGetterEntityAttribute<float> _populationAttribute;

    private CellEntity _cellEntity = null;
    private PolityEntity _mostProminentPolityEntity = null;
    private PolityCollectionEntity _presentPolitiesEntity = null;
    private FactionCollectionEntity _closestFactionsEntity = null;
    private ModifiableGroupPropertyContainerEntity _propertiesEntity = null;

    protected override object _reference => Group;

    public GroupEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public GroupEntity(
        ValueGetterMethod<CellGroup> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public GroupEntity(
        TryRequestGenMethod<CellGroup> tryRequestGenMethod, Context c, string id, IEntity parent)
        : base(tryRequestGenMethod, c, id, parent)
    {
    }

    public EntityAttribute GetCellAttribute()
    {
        _cellEntity =
            _cellEntity ?? new CellEntity(
                GetCell,
                Context,
                BuildAttributeId(CellAttributeId),
                this);

        return _cellEntity.GetThisEntityAttribute();
    }

    public EntityAttribute GetMostProminentPolityAttribute()
    {
        _mostProminentPolityEntity =
            _mostProminentPolityEntity ?? new PolityEntity(
                GetMostProminentPolity,
                Context,
                BuildAttributeId(MostProminentPolityAttributeId),
                this);

        return _mostProminentPolityEntity.GetThisEntityAttribute();
    }

    public EntityAttribute GetPresentPolitiesAttribute()
    {
        _presentPolitiesEntity =
            _presentPolitiesEntity ?? new PolityCollectionEntity(
                GetPresentPolities,
                Context,
                BuildAttributeId(PresentPolitiesAttributeId),
                this);

        return _presentPolitiesEntity.GetThisEntityAttribute();
    }

    public EntityAttribute GetClosestFactionsAttribute()
    {
        _closestFactionsEntity =
            _closestFactionsEntity ?? new FactionCollectionEntity(
                GetClosestFactions,
                Context,
                BuildAttributeId(ClosestFactionsAttributeId),
                this);

        return _closestFactionsEntity.GetThisEntityAttribute();
    }

    private EntityAttribute GetPropertiesAttribute()
    {
        _propertiesEntity =
            _propertiesEntity ?? new ModifiableGroupPropertyContainerEntity(
                GetGroup,
                Context,
                BuildAttributeId(PropertiesAttributeId),
                this);

        return _propertiesEntity.GetThisEntityAttribute();
    }

    protected override ICulturalActivitiesEntity CreateCulturalActivitiesEntity() =>
        new ModifiableCellCulturalActivitiesEntity(
            GetCulture,
            Context,
            BuildAttributeId(ActivitiesAttributeId),
            this);

    protected override ICulturalSkillsEntity CreateCulturalSkillsEntity() =>
        new ModifiableCellCulturalSkillsEntity(
            GetCulture,
            Context,
            BuildAttributeId(SkillsAttributeId),
            this);

    protected override ICulturalKnowledgesEntity CreateCulturalKnowledgesEntity() =>
        new ModifiableCellCulturalKnowledgesEntity(
            GetCulture,
            Context,
            BuildAttributeId(KnowledgesAttributeId),
            this);

    protected override ICulturalDiscoveriesEntity CreateCulturalDiscoveriesEntity() =>
        new ModifiableCulturalDiscoveriesEntity(
            GetCulture,
            Context,
            BuildAttributeId(DiscoveriesAttributeId),
            this);

    private ICollection<Polity> GetPresentPolities() => Group.PresentPolities;

    private ICollection<Faction> GetClosestFactions() => Group.ClosestFactions;

    private void UpdateNavigationRangeModifier(float value)
    {
        Group.NavigationRangeModifier = value;
        Group.SetToUpdate(warnIfUnexpected: false);
    }

    private void UpdateArabilityModifier(float value)
    {
        Group.ArabilityModifier = value;
        Group.SetToUpdate(warnIfUnexpected: false);
    }

    private void UpdateAccessibilityModifier(float value)
    {
        Group.AccessibilityModifier = value;
        Group.SetToUpdate(warnIfUnexpected: false);
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case CellAttributeId:
                return GetCellAttribute();

            case ProminenceValueAttributeId:
                return new ProminenceValueAttribute(this, arguments);

            case GetCoreDistanceAttributeId:
                return new GetCoreDistanceAttribute(this, arguments);

            case MostProminentPolityAttributeId:
                return GetMostProminentPolityAttribute();

            case PresentPolitiesAttributeId:
                return GetPresentPolitiesAttribute();

            case ClosestFactionsAttributeId:
                return GetClosestFactionsAttribute();

            case PropertiesAttributeId:
                return GetPropertiesAttribute();

            case NavigationRangeAttributeId:
                _navigationRangeAttribute =
                    _navigationRangeAttribute ?? new ValueGetterSetterEntityAttribute<float>(
                        NavigationRangeAttributeId, 
                        this, 
                        () => Group.NavigationRangeModifier,
                        UpdateNavigationRangeModifier);
                return _navigationRangeAttribute;

            case ArabilityModifierAttributeId:
                _arabilityModifierAttribute =
                    _arabilityModifierAttribute ?? new ValueGetterSetterEntityAttribute<float>(
                        ArabilityModifierAttributeId,
                        this,
                        () => Group.ArabilityModifier,
                        UpdateArabilityModifier);
                return _arabilityModifierAttribute;

            case AccessibilityModifierAttributeId:
                _accessibilityModifierAttribute =
                    _accessibilityModifierAttribute ?? new ValueGetterSetterEntityAttribute<float>(
                        AccessibilityModifierAttributeId,
                        this,
                        () => Group.AccessibilityModifier,
                        UpdateAccessibilityModifier);
                return _accessibilityModifierAttribute;

            case PopulationAttributeId:
                _populationAttribute =
                    _populationAttribute ?? new ValueGetterEntityAttribute<float>(
                        PopulationAttributeId,
                        this,
                        () => Group.Population);
                return _populationAttribute;
        }

        return base.GetAttribute(attributeId, arguments);
    }

    public override string GetDebugString() => $"group:{Group.Cell.Position}";

    public override string GetFormattedString() => Group.Cell.Position.ToBoldString();

    protected override void ResetInternal()
    {
        if (_isReset)
        {
            return;
        }

        _cellEntity?.Reset();
        _mostProminentPolityEntity?.Reset();
        _presentPolitiesEntity?.Reset();
        _closestFactionsEntity?.Reset();
        _propertiesEntity?.Reset();

        base.ResetInternal();
    }

    private TerrainCell GetCell() => Group.Cell;

    private Polity GetMostProminentPolity() => Group.HighestPolityProminence?.Polity;

    protected override Culture GetCulture() => Group.Culture;

    private CellGroup GetGroup() => Group;
}
