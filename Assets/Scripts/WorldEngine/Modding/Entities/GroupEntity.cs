using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupEntity : DelayedSetEntity<CellGroup>
{
    public const string CellAttributeId = "cell";
    public const string ProminenceAttributeId = "prominence";
    public const string FactionCoresCountAttributeId = "faction_cores_count";
    public const string FactionCoreDistanceAttributeId = "faction_core_distance";
    public const string PreferencesAttributeId = "preferences";
    public const string KnowledgesAttributeId = "knowledges";
    public const string PolityWithHighestProminenceAttributeId = "polity_with_highest_prominence";

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

    public GroupEntity(Context c, string id) : base(c, id)
    {
    }

    public GroupEntity(
        ValueGetterMethod<CellGroup> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    public EntityAttribute GetCellAttribute()
    {
        _cellEntity =
            _cellEntity ?? new CellEntity(
                GetCell,
                Context,
                BuildAttributeId(CellAttributeId));

        return _cellEntity.GetThisEntityAttribute(this);
    }

    public EntityAttribute GetPolityWithHighestProminenceAttribute()
    {
        _polityWithHighestProminenceEntity =
            _polityWithHighestProminenceEntity ?? new PolityEntity(
                GetPolityWithHighestProminence,
                Context,
                BuildAttributeId(PolityWithHighestProminenceAttributeId));

        return _polityWithHighestProminenceEntity.GetThisEntityAttribute(this);
    }

    public EntityAttribute GetPreferencesAttribute()
    {
        _preferencesEntity =
            _preferencesEntity ?? new AssignableCulturalPreferencesEntity(
                Context,
                BuildAttributeId(PreferencesAttributeId));

        return _preferencesEntity.GetThisEntityAttribute(this);
    }

    public EntityAttribute GetKnowledgesAttribute()
    {
        _knowledgesEntity =
            _knowledgesEntity ?? new CulturalKnowledgesEntity(
                GetCulture,
                Context,
                BuildAttributeId(KnowledgesAttributeId));

        return _knowledgesEntity.GetThisEntityAttribute(this);
    }

    protected override object _reference => Group;

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case CellAttributeId:
                return GetCellAttribute();

            case ProminenceAttributeId:
                return new ProminenceAttribute(this, arguments);

            case FactionCoresCountAttributeId:
                _factionCoresCountAttribute =
                    _factionCoresCountAttribute ?? new ValueGetterEntityAttribute<float>(
                        FactionCoresCountAttributeId, this, () => Group.GetFactionCores().Count);
                return _factionCoresCountAttribute;

            case FactionCoreDistanceAttributeId:
                return new FactionCoreDistanceAttribute(this, arguments);

            case PreferencesAttributeId:
                return GetPreferencesAttribute();

            case KnowledgesAttributeId:
                return GetKnowledgesAttribute();

            case PolityWithHighestProminenceAttributeId:
                return GetPolityWithHighestProminenceAttribute();
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

    public Polity GetPolityWithHighestProminence() => Group.HighestPolityProminence?.Polity;

    public Culture GetCulture() => Group.Culture;
}
