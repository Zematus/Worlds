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
    public const string GroupCanBeCoreAttributeId = "group_can_be_core";
    public const string TypeAttributeId = "type";

    public Faction Faction { get; private set; }

    private ValueGetterEntityAttribute<string> _typeAttribute;
    private ValueGetterEntityAttribute<float> _administrativeLoadAttribute;
    private ValueGetterEntityAttribute<float> _influenceAttribute;

    private readonly AgentEntity _leaderEntity;
    private EntityAttribute _leaderEntityAttribute;

    private readonly PolityEntity _polityEntity;
    private EntityAttribute _polityEntityAttribute;

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
        _leaderEntity = new AgentEntity(BuildInternalEntityId(LeaderAttributeId));
        _polityEntity = new PolityEntity(BuildInternalEntityId(PolityAttributeId));
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
        }

        throw new System.ArgumentException("Faction: Unable to find attribute: " + attributeId);
    }

    public void Set(Faction f)
    {
        Faction = f;

        _preferencesEntity.Set(Faction.Culture);

        _leaderEntity.Set(Faction.CurrentLeader);

        _polityEntity.Set(Faction.Polity);
    }

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
