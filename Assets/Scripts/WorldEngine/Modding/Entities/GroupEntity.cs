using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupEntity : DelayedSetEntity<CellGroup>
{
    public const string CellAttributeId = "cell";
    public const string ProminenceValueAttributeId = "prominence_value";
    public const string GetCoreDistanceAttributeId = "get_core_distance";
    public const string PreferencesAttributeId = "preferences";
    public const string KnowledgesAttributeId = "knowledges";
    public const string MostProminentPolityAttributeId = "most_prominent_polity";
    public const string PresentPolitiesAttributeId = "present_polities";
    public const string ClosestFactionsAttributeId = "closest_factions";

    public virtual CellGroup Group
    {
        get => Setable;
        private set => Setable = value;
    }

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

    public EntityAttribute GetMostProminentPolityAttribute()
    {
        _polityWithHighestProminenceEntity =
            _polityWithHighestProminenceEntity ?? new PolityEntity(
                GetMostProminentPolity,
                Context,
                BuildAttributeId(MostProminentPolityAttributeId),
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

            case GetCoreDistanceAttributeId:
                return new GetCoreDistanceAttribute(this, arguments);

            case PreferencesAttributeId:
                return GetPreferencesAttribute();

            case KnowledgesAttributeId:
                return GetKnowledgesAttribute();

            case MostProminentPolityAttributeId:
                return GetMostProminentPolityAttribute();

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

    public Polity GetMostProminentPolity() => Group.HighestPolityProminence?.Polity;

    public Culture GetCulture() => Group.Culture;
}
