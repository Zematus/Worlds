using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Profiling;

public class FactionEntity : DelayedSetEntity<Faction>
{
    public const string AdministrativeLoadAttributeId = "administrative_load";
    public const string InfluenceAttributeId = "influence";
    public const string LeaderAttributeId = "leader";
    public const string PolityAttributeId = "polity";
    public const string PreferencesAttributeId = "preferences";
    public const string TriggerDecisionAttributeId = "trigger_decision";
    public const string SplitAttributeId = "split";
    public const string CoreGroupAttributeId = "core_group";
    public const string TypeAttributeId = "type";
    public const string GetRelationshipAttributeId = "get_relationship";
    public const string SetRelationshipAttributeId = "set_relationship";

    public virtual Faction Faction
    {
        get => Setable;
        private set => Setable = value;
    }

    private ValueGetterEntityAttribute<string> _typeAttribute;
    private ValueGetterEntityAttribute<float> _administrativeLoadAttribute;
    private ValueGetterEntityAttribute<float> _influenceAttribute;

    private AgentEntity _leaderEntity = null;
    private PolityEntity _polityEntity = null;
    private GroupEntity _coreGroupEntity = null;

    private AssignableCulturalPreferencesEntity _preferencesEntity = null;

    protected override object _reference => Faction;

    public override string GetDebugString()
    {
        return "faction:" + Faction.GetName();
    }

    public override string GetFormattedString()
    {
        return Faction.GetNameBold();
    }

    public FactionEntity(Context c, string id) : base(c, id)
    {
    }

    public FactionEntity(
        ValueGetterMethod<Faction> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    public EntityAttribute GetPreferencesAttribute()
    {
        _preferencesEntity =
            _preferencesEntity ?? new AssignableCulturalPreferencesEntity(
                GetCulture,
                Context,
                BuildAttributeId(PreferencesAttributeId));

        return _preferencesEntity.GetThisEntityAttribute(this);
    }

    public EntityAttribute GetLeaderAttribute()
    {
        _leaderEntity =
            _leaderEntity ?? new AgentEntity(
                GetLeader,
                Context,
                BuildAttributeId(LeaderAttributeId));

        return _leaderEntity.GetThisEntityAttribute(this);
    }

    public EntityAttribute GetPolityAttribute()
    {
        _polityEntity =
            _polityEntity ?? new PolityEntity(
                GetPolity,
                Context,
                BuildAttributeId(PolityAttributeId));

        return _polityEntity.GetThisEntityAttribute(this);
    }

    public EntityAttribute GetCoreGroupAttribute()
    {
        _coreGroupEntity =
            _coreGroupEntity ?? new GroupEntity(
                GetCoreGroup,
                Context,
                BuildAttributeId(CoreGroupAttributeId));

        return _coreGroupEntity.GetThisEntityAttribute(this);
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case TypeAttributeId:
                _typeAttribute =
                    _typeAttribute ?? new ValueGetterEntityAttribute<string>(
                        TypeAttributeId, this, () => Faction.Type);
                return _typeAttribute;

            case AdministrativeLoadAttributeId:
                _administrativeLoadAttribute =
                    _administrativeLoadAttribute ?? new ValueGetterEntityAttribute<float>(
                        AdministrativeLoadAttributeId, this, () => Faction.AdministrativeLoad);
                return _administrativeLoadAttribute;

            case InfluenceAttributeId:
                _influenceAttribute =
                    _influenceAttribute ?? new ValueGetterEntityAttribute<float>(
                        InfluenceAttributeId, this, () => Faction.Influence);
                return _influenceAttribute;

            case PreferencesAttributeId:
                return GetPreferencesAttribute();

            case TriggerDecisionAttributeId:
                return new TriggerDecisionAttribute(this, arguments);

            case SplitAttributeId:
                return new SplitFactionAttribute(this, arguments);

            case GetRelationshipAttributeId:
                return new GetRelationshipAttribute(this, arguments);

            case SetRelationshipAttributeId:
                return new SetRelationshipAttribute(this, arguments);

            case LeaderAttributeId:
                return GetLeaderAttribute();

            case PolityAttributeId:
                return GetPolityAttribute();

            case CoreGroupAttributeId:
                return GetCoreGroupAttribute();
        }

        throw new System.ArgumentException("Faction: Unable to find attribute: " + attributeId);
    }

    protected override void ResetInternal()
    {
        if (_isReset)
        {
            return;
        }

        _leaderEntity?.Reset();
        _polityEntity?.Reset();
        _coreGroupEntity?.Reset();

        _preferencesEntity?.Reset();
    }

    public Agent GetLeader() => Faction.CurrentLeader;

    public Polity GetPolity() => Faction.Polity;

    public CellGroup GetCoreGroup() => Faction.CoreGroup;

    public Culture GetCulture() => Faction.Culture;
}
