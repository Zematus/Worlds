using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FactionEntity : Entity
{
    public const string AdministrativeLoadAttributeId = "administrative_load";
    public const string LeaderAttributeId = "leader";
    public const string PreferencesAttributeId = "preferences";
    public const string TriggerDecisionAttributeId = "trigger_decision";
    public const string TypeAttributeId = "type";

    public Faction Faction { get; private set; }

    private TypeAttribute _typeAttribute;

    private AdministrativeLoadAttribute _administrativeLoadAttribute;

    private AgentEntity _leaderEntity;
    private EntityAttribute _leaderEntityAttribute;

    private CulturalPreferencesEntity _preferencesEntity =
        new CulturalPreferencesEntity(PreferencesAttributeId);
    private EntityAttribute _preferencesAttribute;

    protected override object _reference => Faction;

    public class TypeAttribute : StringEntityAttribute
    {
        private FactionEntity _factionEntity;

        public TypeAttribute(FactionEntity factionEntity)
            : base(TypeAttributeId, factionEntity, null)
        {
            _factionEntity = factionEntity;
        }

        public override string Value => _factionEntity.Faction.Type;
    }

    public class AdministrativeLoadAttribute : NumericEntityAttribute
    {
        private FactionEntity _factionEntity;

        public AdministrativeLoadAttribute(FactionEntity factionEntity)
            : base(AdministrativeLoadAttributeId, factionEntity, null)
        {
            _factionEntity = factionEntity;
        }

        public override float Value => _factionEntity.Faction.AdministrativeLoad;
    }

    public class TriggerDecisionAttribute : EffectEntityAttribute
    {
        private FactionEntity _factionEntity;

        private Decision _decisionToTrigger = null;
        private bool _unfixedDecision = true;

        private IStringExpression _argumentExp;

        public TriggerDecisionAttribute(FactionEntity factionEntity, IExpression[] arguments)
            : base(TriggerDecisionAttributeId, factionEntity, arguments)
        {
            _factionEntity = factionEntity;

            if ((arguments == null) || (arguments.Length < 1))
            {
                throw new System.ArgumentException("Number of arguments less than 1");
            }

            _argumentExp = ExpressionBuilder.ValidateStringExpression(arguments[0]);

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
                    _typeAttribute ?? new TypeAttribute(this);
                return _typeAttribute;

            case AdministrativeLoadAttributeId:
                _administrativeLoadAttribute =
                    _administrativeLoadAttribute ?? new AdministrativeLoadAttribute(this);
                return _administrativeLoadAttribute;

            case PreferencesAttributeId:
                _preferencesAttribute =
                    _preferencesAttribute ??
                    new FixedEntityEntityAttribute(
                        _preferencesEntity, PreferencesAttributeId, this, arguments);
                return _preferencesAttribute;

            case TriggerDecisionAttributeId:
                return new TriggerDecisionAttribute(this, arguments);

            case LeaderAttributeId:
                _leaderEntityAttribute =
                    _leaderEntityAttribute ??
                    new FixedEntityEntityAttribute(
                        _leaderEntity, LeaderAttributeId, this, arguments);
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
