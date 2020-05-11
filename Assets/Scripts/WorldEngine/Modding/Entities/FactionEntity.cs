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
    public const string CoreGroupId = "core_group";
    public const string TypeAttributeId = "type";

    public Faction Faction { get; private set; }

    private ValueGetterEntityAttribute<string> _typeAttribute;
    private ValueGetterEntityAttribute<float> _administrativeLoadAttribute;
    private ValueGetterEntityAttribute<float> _influenceAttribute;

    private readonly DelayedSetAgentEntity _leaderEntity;
    private EntityAttribute _leaderEntityAttribute;

    private readonly DelayedSetPolityEntity _polityEntity;
    private EntityAttribute _polityEntityAttribute;

    private readonly DelayedSetGroupEntity _coreGroupEntity;
    private EntityAttribute _coreGroupEntityAttribute;

    private readonly CulturalPreferencesEntity _preferencesEntity =
        new CulturalPreferencesEntity(PreferencesAttributeId);
    private EntityAttribute _preferencesAttribute;

    protected override object _reference => Faction;

    public override string GetFormattedString()
    {
        return Faction.Name.BoldText;
    }

    public FactionEntity(string id) : base(id)
    {
        _leaderEntity = new DelayedSetAgentEntity(
            GetLeader,
            BuildAttributeId(LeaderAttributeId));

        _polityEntity = new DelayedSetPolityEntity(
            GetPolity,
            BuildAttributeId(PolityAttributeId));

        _coreGroupEntity = new DelayedSetGroupEntity(
            GetCoreGroup,
            BuildAttributeId(CoreGroupId));
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
                _preferencesAttribute =
                    _preferencesAttribute ?? new FixedValueEntityAttribute<Entity>(
                        _preferencesEntity, PreferencesAttributeId, this);
                return _preferencesAttribute;

            case TriggerDecisionAttributeId:
                return new TriggerDecisionAttribute(this, arguments);

            case SplitFactionAttributeId:
                return new SplitFactionAttribute(this, arguments);

            case GroupCanBeCoreAttributeId:
                return new GroupCanBeCoreAttribute(this, arguments);

            case LeaderAttributeId:
                _leaderEntityAttribute =
                    _leaderEntityAttribute ?? new FixedValueEntityAttribute<Entity>(
                        _leaderEntity, LeaderAttributeId, this);
                return _leaderEntityAttribute;

            case PolityAttributeId:
                _polityEntityAttribute =
                    _polityEntityAttribute ?? new FixedValueEntityAttribute<Entity>(
                        _polityEntity, PolityAttributeId, this);
                return _polityEntityAttribute;

            case CoreGroupId:
                _coreGroupEntityAttribute =
                    _coreGroupEntityAttribute ?? new FixedValueEntityAttribute<Entity>(
                        _coreGroupEntity, PolityAttributeId, this);
                return _coreGroupEntityAttribute;
        }

        throw new System.ArgumentException("Faction: Unable to find attribute: " + attributeId);
    }

    public void Set(Faction f, bool noReset = false)
    {
        if (noReset && (Faction == f))
        {
            return;
        }

        Faction = f;

        _preferencesEntity.Set(Faction.Culture);

        _leaderEntity.Reset();
        _polityEntity.Reset();
        _coreGroupEntity.Reset();
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
