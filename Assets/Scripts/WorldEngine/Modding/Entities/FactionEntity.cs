using System.Collections.Generic;

public class FactionEntity : DelayedSetEntity<Faction>
{
    public const string AdministrativeLoadAttributeId = "administrative_load";
    public const string InfluenceAttributeId = "influence";
    public const string LeaderAttributeId = "leader";
    public const string PolityAttributeId = "polity";
    public const string PreferencesAttributeId = "preferences";
    public const string TriggerDecisionAttributeId = "trigger_decision";
    public const string SplitAttributeId = "split";
    public const string RemoveAttributeId = "remove";
    public const string CoreGroupAttributeId = "core_group";
    public const string TypeAttributeId = "type";
    public const string GuideAttributeId = "guide";
    public const string GetRelationshipAttributeId = "get_relationship";
    public const string SetRelationshipAttributeId = "set_relationship";
    public const string GetGroupsWithConditionAttributeId = "get_groups_with_condition";

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

    private AssignableCulturalPreferencesEntity _preferencesEntity = null;

    private int _groupCollectionIndex = 0;

    private List<GroupCollectionEntity> _groupCollectionEntitiesToSet = 
        new List<GroupCollectionEntity>();

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

    private EffectEntityAttribute GenerateRemoveAttribute()
    {
        EffectEntityAttribute attribute =
            new EffectApplierEntityAttribute(
                RemoveAttributeId,
                this,
                () => Faction.SetToRemove());

        return attribute;
    }

    public string GetGuide() =>
        Faction.IsUnderPlayerGuidance ? "player" : "simulation";

    public ParametricSubcontext BuildGroupsWithConditionAttributeSubcontext(
        Context parentContext,
        string[] paramIds)
    {
        int index = _groupCollectionIndex;

        if ((paramIds == null) || (paramIds.Length < 1))
        {
            throw new System.ArgumentException(
                $"{GetGroupsWithConditionAttributeId}: expected at least one parameter identifier");
        }

        var groupEntity = new GroupEntity(Context, paramIds[0]);

        var subcontext = 
            new ParametricSubcontext(
                $"{GetGroupsWithConditionAttributeId}_{index}", 
                parentContext);
        subcontext.AddEntity(groupEntity);

        return subcontext;
    }

    public EntityAttribute GetGroupsWithConditionAttribute(
        ParametricSubcontext subcontext, 
        string[] paramIds, 
        IExpression[] arguments)
    {
        int index = _groupCollectionIndex++;

        if ((paramIds == null) || (paramIds.Length < 1))
        {
            throw new System.ArgumentException(
                GetGroupsWithConditionAttributeId + ": expected at least one parameter identifier");
        }

        GroupEntity paramGroupEntity = subcontext.GetEntity(paramIds[0]) as GroupEntity;

        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException(
                GetGroupsWithConditionAttributeId + ": expected at least one condition argument");
        }

        var conditionExp = ValueExpressionBuilder.ValidateValueExpression<bool>(arguments[0]);

        var collectionEntity = new GroupCollectionEntity(
            () =>
            {
                var selectedGroups = new HashSet<CellGroup>();

                foreach (var group in Faction.InnerGroups)
                {
                    paramGroupEntity.Set(group);

                    if (conditionExp.Value)
                    {
                        selectedGroups.Add(group);
                    }
                }

                return selectedGroups;
            },
            Context,
            BuildAttributeId($"groups_collection_{index}"));

        _groupCollectionEntitiesToSet.Add(collectionEntity);

        return collectionEntity.GetThisEntityAttribute(this);
    }

    public override ParametricSubcontext BuildParametricSubcontext(
        Context parentContext,
        string attributeId, 
        string[] paramIds)
    {
        switch (attributeId)
        {
            case GetGroupsWithConditionAttributeId:
                return BuildGroupsWithConditionAttributeSubcontext(parentContext, paramIds);
        }

        throw new System.ArgumentException(
            $"Faction: Unable to build parametric subcontext for attribute: {attributeId}");
    }

    public override EntityAttribute GetParametricAttribute(
        string attributeId,
        ParametricSubcontext subcontext,
        string[] paramIds,
        IExpression[] arguments)
    {
        switch (attributeId)
        {
            case GetGroupsWithConditionAttributeId:
                return GetGroupsWithConditionAttribute(subcontext, paramIds, arguments);
        }

        throw new System.ArgumentException("Faction: Unable to find parametric attribute: " + attributeId);
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

            case PreferencesAttributeId:
                return GetPreferencesAttribute();

            case TriggerDecisionAttributeId:
                return new TriggerDecisionAttribute(this, arguments);

            case SplitAttributeId:
                return new SplitFactionAttribute(this, arguments);

            case RemoveAttributeId:
                return GenerateRemoveAttribute();

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

            case GetGroupsWithConditionAttributeId:
                throw new System.ArgumentException($"Faction: '{attributeId}' is a parametric attribute");
        }

        throw new System.ArgumentException($"Faction: Unable to find attribute: {attributeId}");
    }

    protected override void ResetInternal()
    {
        if (_isReset)
        {
            return;
        }

        foreach (var entity in _groupCollectionEntitiesToSet)
        {
            entity.Reset();
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
