using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FactionEntity : Entity
{
    public const string AdministrativeLoadAttributeId = "administrative_load";
    public const string InfluenceAttributeId = "influence";
    public const string LeaderAttributeId = "leader";
    public const string PolityAttributeId = "polity";
    public const string PreferencesAttributeId = "preferences";
    public const string TriggerDecisionAttributeId = "trigger_decision";
    public const string SplitFactionAttributeId = "split";
    public const string GroupCanBeCoreAttributeId = "group_can_be_core";
    public const string CoreGroupAttributeId = "core_group";
    public const string TypeAttributeId = "type";
    public const string RelationshipAttributeId = "relationship";
    public const string SetRelationshipAttributeId = "set_relationship";

    public virtual Faction Faction { get; private set; }

    private bool _alreadyReset = false;

    private ValueGetterEntityAttribute<string> _typeAttribute;
    private ValueGetterEntityAttribute<float> _administrativeLoadAttribute;
    private ValueGetterEntityAttribute<float> _influenceAttribute;

    private DelayedSetAgentEntity _leaderEntity = null;
    private DelayedSetPolityEntity _polityEntity = null;
    private DelayedSetGroupEntity _coreGroupEntity = null;

    private CulturalPreferencesEntity _preferencesEntity = null;

    protected override object _reference => Faction;

    public override string GetDebugString()
    {
        return "faction:" + Faction.GetName();
    }

    public override string GetFormattedString()
    {
        return Faction.GetNameBold();
    }

    public FactionEntity(string id) : base(id)
    {
    }

    public EntityAttribute GetPreferencesAttribute()
    {
        _preferencesEntity =
            _preferencesEntity ?? new CulturalPreferencesEntity(
                BuildAttributeId(PreferencesAttributeId));

        return _preferencesEntity.GetThisEntityAttribute(this);
    }

    public EntityAttribute GetLeaderAttribute()
    {
        _leaderEntity =
            _leaderEntity ?? new DelayedSetAgentEntity(
                GetLeader,
                BuildAttributeId(LeaderAttributeId));

        return _leaderEntity.GetThisEntityAttribute(this);
    }

    public EntityAttribute GetPolityAttribute()
    {
        _polityEntity =
            _polityEntity ?? new DelayedSetPolityEntity(
                GetPolity,
                BuildAttributeId(PolityAttributeId));

        return _polityEntity.GetThisEntityAttribute(this);
    }

    public EntityAttribute GetCoreGroupAttribute()
    {
        _coreGroupEntity =
            _coreGroupEntity ?? new DelayedSetGroupEntity(
                GetCoreGroup,
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

            case SplitFactionAttributeId:
                return new SplitFactionAttribute(this, arguments);

            case GroupCanBeCoreAttributeId:
                return new GroupCanBeCoreAttribute(this, arguments);

            case RelationshipAttributeId:
                return new RelationshipAttribute(this, arguments);

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

    protected void ResetInternal()
    {
        if (_alreadyReset)
        {
            return;
        }

        _leaderEntity?.Reset();
        _polityEntity?.Reset();
        _coreGroupEntity?.Reset();

        _alreadyReset = true;
    }

    public virtual void Set(Faction f)
    {
        f.PreUpdate();

        Faction = f;

        _preferencesEntity?.Set(Faction.Culture);

        ResetInternal();

        _alreadyReset = false;
    }

    public Agent GetLeader() => Faction.CurrentLeader;

    public Polity GetPolity() => Faction.Polity;

    public CellGroup GetCoreGroup() => Faction.CoreGroup;

    public override void Set(object o)
    {
        if (o is FactionEntity e)
        {
            Set(e.Faction);
        }
        else if (o is Faction f)
        {
            Set(f);
        }
        else
        {
            throw new System.ArgumentException("Unexpected type: " + o.GetType());
        }
    }
}
