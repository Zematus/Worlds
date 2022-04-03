using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupEntity : DelayedSetEntity<CellGroup>
{
    public const string CellAttributeId = "cell";
    public const string ProminenceValueAttributeId = "prominence_value";
    public const string FactionCoresCountAttributeId = "faction_cores_count";
    public const string GetFactionCoreDistanceAttributeId = "get_faction_core_distance";
    public const string PreferencesAttributeId = "preferences";
    public const string KnowledgesAttributeId = "knowledges";
    public const string PolityWithHighestProminenceValueAttributeId = "polity_with_highest_prominence_value";
    public const string HasPolityOfTypeAttributeId = "has_polity_of_type";
    public const string PresentPolitiesAttributeId = "present_polities";
    public const string ClosestFactionsAttributeId = "closest_factions";

    public virtual CellGroup Group
    {
        get => Setable;
        private set => Setable = value;
    }

    private ValueGetterEntityAttribute<float> _factionCoresCountAttribute;

    private CellEntity _cellEntity = null;
    private PolityEntity _polityWithHighestProminenceEntity = null;

    private AssignableCulturalPreferencesEntity _preferencesEntity = null;
    private CulturalKnowledgesEntity _knowledgesEntity = null;

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

    public EntityAttribute GetPolityWithHighestProminenceValueAttribute()
    {
        _polityWithHighestProminenceEntity =
            _polityWithHighestProminenceEntity ?? new PolityEntity(
                GetPolityWithHighestProminenceValue,
                Context,
                BuildAttributeId(PolityWithHighestProminenceValueAttributeId),
                this);

        return _polityWithHighestProminenceEntity.GetThisEntityAttribute();
    }

    public EntityAttribute GetPreferencesAttribute()
    {
        _preferencesEntity =
            _preferencesEntity ?? new AssignableCulturalPreferencesEntity(
                GetCulture,
                Context,
                BuildAttributeId(PreferencesAttributeId),
                this);

        return _preferencesEntity.GetThisEntityAttribute();
    }

    public EntityAttribute GetKnowledgesAttribute()
    {
        _knowledgesEntity =
            _knowledgesEntity ?? new CulturalKnowledgesEntity(
                GetCulture,
                Context,
                BuildAttributeId(KnowledgesAttributeId),
                this);

        return _knowledgesEntity.GetThisEntityAttribute();
    }

    private ValueGetterEntityAttribute<bool> GenerateHasPolityOfTypeAttribute(IExpression[] arguments)
    {
        IValueExpression<string> argumentExp = null;
        if (arguments.Length > 0)
        {
            argumentExp = ValueExpressionBuilder.ValidateValueExpression<string>(arguments[0]);
        }

        var attribute =
            new ValueGetterEntityAttribute<bool>(
                HasPolityOfTypeAttributeId, 
                this, 
                () => {
                    PolityType type = PolityEntity.ConvertToType(argumentExp?.Value);

                    return Group.HasPolityOfType(type);
                });

        return attribute;
    }

    protected override object _reference => Group;

    private PolityCollectionEntity _presentPolitiesEntity = null;

    private FactionCollectionEntity _closestFactionsEntity = null;

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

        return _presentPolitiesEntity.GetThisEntityAttribute();
    }

    public ICollection<Polity> GetPresentPolities() => Group.PresentPolities;

    public ICollection<Faction> GetClosestFactions() => Group.ClosestFactions;

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case CellAttributeId:
                return GetCellAttribute();

            case ProminenceValueAttributeId:
                return new ProminenceValueAttribute(this, arguments);

            case FactionCoresCountAttributeId:
                _factionCoresCountAttribute =
                    _factionCoresCountAttribute ?? new ValueGetterEntityAttribute<float>(
                        FactionCoresCountAttributeId, this, () => Group.GetFactionCores().Count);
                return _factionCoresCountAttribute;

            case GetFactionCoreDistanceAttributeId:
                return new GetFactionCoreDistanceAttribute(this, arguments);

            case PreferencesAttributeId:
                return GetPreferencesAttribute();

            case KnowledgesAttributeId:
                return GetKnowledgesAttribute();

            case PolityWithHighestProminenceValueAttributeId:
                return GetPolityWithHighestProminenceValueAttribute();

            case HasPolityOfTypeAttributeId:
                return GenerateHasPolityOfTypeAttribute(arguments);

            case PresentPolitiesAttributeId:
                return GetPresentPolitiesAttribute();

            case ClosestFactionsAttributeId:
                return GetClosestFactionsAttribute();
        }

        return base.GetAttribute(attributeId, arguments);
    }

    public override string GetDebugString()
    {
        return "group:" + Group.Cell.Position.ToString();
    }

    public override string GetFormattedString()
    {
        return Group.Cell.Position.ToBoldString();
    }

    protected override void ResetInternal()
    {
        if (_isReset)
        {
            return;
        }

        _cellEntity?.Reset();
        _polityWithHighestProminenceEntity?.Reset();

        _preferencesEntity?.Reset();
        _knowledgesEntity?.Reset();
    }

    public TerrainCell GetCell() => Group.Cell;

    public Polity GetPolityWithHighestProminenceValue() => Group.HighestPolityProminence?.Polity;

    public Culture GetCulture() => Group.Culture;
}
