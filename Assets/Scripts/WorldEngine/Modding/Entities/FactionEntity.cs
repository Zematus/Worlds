using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FactionEntity : Entity
{
    public const string AdministrativeLoadAttributeId = "administrative_load";
    public const string InfluenceAttributeId = "influence";
    public const string LeaderAttributeId = "leader";
    public const string PreferencesAttributeId = "preferences";
    public const string TriggerDecisionAttributeId = "trigger_decision";
    public const string TypeAttributeId = "type";

    public Faction Faction { get; private set; }

    private ValueGetterEntityAttribute<string> _typeAttribute;
    private ValueGetterEntityAttribute<float> _administrativeLoadAttribute;
    private ValueGetterEntityAttribute<float> _influenceAttribute;

    private readonly AgentEntity _leaderEntity;
    private EntityAttribute _leaderEntityAttribute;

    private readonly CulturalPreferencesEntity _preferencesEntity =
        new CulturalPreferencesEntity(PreferencesAttributeId);
    private EntityAttribute _preferencesAttribute;

    protected override object _reference => Faction;

    public override string GetFormattedString()
    {
        return Faction.Name.BoldText;
    }

    public class TriggerDecisionAttribute : EffectEntityAttribute
    {
        private FactionEntity _factionEntity;

        private Decision _decisionToTrigger = null;
        private bool _unfixedDecision = true;

        private readonly IValueExpression<string> _argumentExp;

        public TriggerDecisionAttribute(FactionEntity factionEntity, IExpression[] arguments)
            : base(TriggerDecisionAttributeId, factionEntity, arguments)
        {
            _factionEntity = factionEntity;

            if ((arguments == null) || (arguments.Length < 1))
            {
                throw new System.ArgumentException("Number of arguments less than 1");
            }

            _argumentExp = ExpressionBuilder.ValidateValueExpression<string>(arguments[0]);

            if (_argumentExp is FixedStringValueExpression)
            {
                // The decision to trigger won't change in the future
                // so we can set it now
                SetDecision();
                _unfixedDecision = false;
            }
        }

        private void SetDecision()
        {
            //_decisionToTrigger = Decision.Decisions[_argumentExp.Value];
        }

        public override void Apply()
        {
            if (_unfixedDecision)
            {
                SetDecision();
            }

            //_decisionToTrigger.Trigger();
        }
    }

    public FactionEntity(string id) : base(id)
    {
        _leaderEntity = new AgentEntity(BuildInternalEntityId(LeaderAttributeId));
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

            case LeaderAttributeId:
                _leaderEntityAttribute =
                    _leaderEntityAttribute ?? new FixedValueEntityAttribute<Entity>(
                        _leaderEntity, LeaderAttributeId, this);
                return _leaderEntityAttribute;
        }

        throw new System.ArgumentException("Faction: Unable to find attribute: " + attributeId);
    }

    public void Set(Faction faction)
    {
        Faction = faction;

        _preferencesEntity.Set(faction.Culture);

        _leaderEntity.Set(faction.CurrentLeader);
    }
}
