using System.Collections.Generic;
using UnityEngine;

public class FactionEntity : CulturalEntity<Faction>
{
    public const string AdministrativeLoadAttributeId = "administrative_load";
    public const string InfluenceAttributeId = "influence";
    public const string LeaderAttributeId = "leader";
    public const string PolityAttributeId = "polity";
    public const string TriggerDecisionAttributeId = "trigger_decision";
    public const string SplitAttributeId = "split";
    public const string RemoveAttributeId = "remove";
    public const string MigrateCoreToGroupAttributeId = "migrate_core_to_group";
    public const string CoreGroupAttributeId = "core_group";
    public const string TypeAttributeId = "type";
    public const string GuideAttributeId = "guide";
    public const string GetRelationshipAttributeId = "get_relationship";
    public const string SetRelationshipAttributeId = "set_relationship";
    public const string GroupsAttributeId = "groups";
    public const string HasContactWithAttributeId = "has_contact_with";
    public const string ChangePolityAttributeId = "change_polity";

    public virtual Faction Faction
    {
        get => Setable;
        private set => Setable = value;
    }

    private ValueGetterEntityAttribute<string> _typeAttribute;
    private ValueGetterEntityAttribute<string> _guideAttribute;
    private ValueGetterEntityAttribute<float> _administrativeLoadAttribute;
    private ValueGetterEntityAttribute<float> _influenceAttribute;

    private AgentEntity _leaderEntity = null;
    private PolityEntity _polityEntity = null;
    private GroupEntity _coreGroupEntity = null;
    private GroupCollectionEntity _groupsEntity = null;

    protected override object _reference => Faction;

    public override string GetDebugString() => $"faction:{Faction.GetName()}";

    public override string GetFormattedString() => Faction.GetNameBold();

    public FactionEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public FactionEntity(
        ValueGetterMethod<Faction> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public FactionEntity(
        TryRequestGenMethod<Faction> tryRequestGenMethod, Context c, string id, IEntity parent)
        : base(tryRequestGenMethod, c, id, parent)
    {
    }

    public ICollection<CellGroup> GetGroups() => Faction.Groups;

    public EntityAttribute GetLeaderAttribute()
    {
        _leaderEntity =
            _leaderEntity ?? new AgentEntity(
                GetLeader,
                Context,
                BuildAttributeId(LeaderAttributeId),
                this);

        return _leaderEntity.GetThisEntityAttribute();
    }

    public EntityAttribute GetPolityAttribute()
    {
        _polityEntity =
            _polityEntity ?? new PolityEntity(
                GetPolity,
                Context,
                BuildAttributeId(PolityAttributeId),
                this);

        return _polityEntity.GetThisEntityAttribute();
    }

    public EntityAttribute GetCoreGroupAttribute()
    {
        _coreGroupEntity =
            _coreGroupEntity ?? new GroupEntity(
                GetCoreGroup,
                Context,
                BuildAttributeId(CoreGroupAttributeId),
                this);

        return _coreGroupEntity.GetThisEntityAttribute();
    }

    private EffectEntityAttribute GenerateRemoveAttribute()
    {
        EffectEntityAttribute attribute =
            new EffectApplierEntityAttribute(
                RemoveAttributeId,
                this,
                () => Faction.SetToRemove());

        return attribute;
    }

    public EffectEntityAttribute GetMigrateCoreToGroupAttribute(IExpression[] arguments)
    {
        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException(
                $"{MigrateCoreToGroupAttributeId}: expected one argument");
        }

        var entityExp = 
            ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[0]);

        var attribute =
            new EffectApplierEntityAttribute(
                MigrateCoreToGroupAttributeId,
                this,
                () => {
                    GroupEntity groupEntity = entityExp.Value as GroupEntity;

                    if (groupEntity == null)
                    {
                        throw new System.ArgumentException(
                            $"{MigrateCoreToGroupAttributeId}: invalid group:" +
                            $"\n - expression: {ToString()}" +
                            $"\n - group: {entityExp.ToPartiallyEvaluatedString()}");
                    }

                    Faction.MigrateCoreToGroup(groupEntity.Group);
                });

        return attribute;
    }

    public EffectEntityAttribute GenerateChangePolityAttribute(IExpression[] arguments)
    {
        if ((arguments == null) || (arguments.Length < 2))
        {
            throw new System.ArgumentException(
                $"{ChangePolityAttributeId}: expected two arguments");
        }

        var polityEntityExp =
            ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[0]);
        var influenceValExp =
            ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);

        var attribute =
            new EffectApplierEntityAttribute(
                ChangePolityAttributeId,
                this,
                () => {
                    PolityEntity polityEntity = polityEntityExp.Value as PolityEntity;

                    if (polityEntity == null)
                    {
                        throw new System.ArgumentException(
                            $"{ChangePolityAttributeId}: invalid polity:" +
                            $"\n - expression: {ToString()}" +
                            $"\n - polity: {polityEntityExp.ToPartiallyEvaluatedString()}");
                    }

                    Faction.ChangePolity(polityEntity.Polity, influenceValExp.Value);
                });

        return attribute;
    }

    private ValueGetterEntityAttribute<bool> GenerateHasContactWithAttribute(IExpression[] arguments)
    {
        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException(
                $"{HasContactWithAttributeId}: expected one argument");
        }

        var entityExp =
            ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[0]);

        var attribute =
            new ValueGetterEntityAttribute<bool>(
                HasContactWithAttributeId,
                this,
                () => {
                    PolityEntity polityEntity = entityExp.Value as PolityEntity;

                    if (polityEntity == null)
                    {
                        throw new System.ArgumentException(
                            $"{MigrateCoreToGroupAttributeId}: invalid polity:" +
                            $"\n - expression: {ToString()}" +
                            $"\n - polity: {entityExp.ToPartiallyEvaluatedString()}");
                    }

                    return Faction.HasContactWithPolity(polityEntity.Polity);
                });

        return attribute;
    }

    public string GetGuide() =>
        Faction.IsUnderPlayerGuidance ? Context.Guide_Player : Context.Guide_Simulation;

    public EntityAttribute GetGroupsAttribute()
    {
        _groupsEntity =
            _groupsEntity ?? new GroupCollectionEntity(
            GetGroups,
            Context,
            BuildAttributeId(GroupsAttributeId),
            this);

        return _groupsEntity.GetThisEntityAttribute();
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

            case GuideAttributeId:
                _guideAttribute =
                    _guideAttribute ?? new ValueGetterEntityAttribute<string>(
                        GuideAttributeId, this, GetGuide);
                return _guideAttribute;

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

            case TriggerDecisionAttributeId:
                return new TriggerDecisionAttribute(this, arguments);

            case SplitAttributeId:
                return new SplitFactionAttribute(this, arguments);

            case RemoveAttributeId:
                return GenerateRemoveAttribute();

            case ChangePolityAttributeId:
                return GenerateChangePolityAttribute(arguments);

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

            case MigrateCoreToGroupAttributeId:
                return GetMigrateCoreToGroupAttribute(arguments);

            case HasContactWithAttributeId:
                return GenerateHasContactWithAttribute(arguments);

            case GroupsAttributeId:
                return GetGroupsAttribute();
        }

        return base.GetAttribute(attributeId, arguments);
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
        _groupsEntity?.Reset();

        base.ResetInternal();
    }

    public Agent GetLeader() => Faction.CurrentLeader;

    public Polity GetPolity() => Faction.Polity;

    public CellGroup GetCoreGroup() => Faction.CoreGroup;

    public override Culture GetCulture() => Faction.Culture;
}
