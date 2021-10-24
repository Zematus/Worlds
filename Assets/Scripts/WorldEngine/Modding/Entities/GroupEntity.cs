using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GroupEntity : DelayedSetEntity<CellGroup>
{
    public const string CellAttributeId = "cell";
    public const string ProminenceValueAttributeId = "prominence_value";
    public const string FactionCoresCountAttributeId = "faction_cores_count";
    public const string HasFactionAttributeId = "has_faction";
    public const string GetFactionAttributeId = "get_faction";
    public const string GetFactionCoreDistanceAttributeId = "get_faction_core_distance";
    public const string PreferencesAttributeId = "preferences";
    public const string KnowledgesAttributeId = "knowledges";
    public const string PolityWithHighestProminenceValueAttributeId = "polity_with_highest_prominence_value";
    public const string GetRandomPolityAttributeId = "get_random_polity";
    public const string HasPolityOfTypeAttributeId = "has_polity_of_type";

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

    private int _polityIndex = 0;
    private int _factionIndex = 0;

    private readonly List<PolityEntity>
        _polityEntitiesToSet = new List<PolityEntity>();
    private readonly List<FactionEntity>
        _factionEntitiesToSet = new List<FactionEntity>();

    public GroupEntity(Context c, string id) : base(c, id)
    {
    }

    public GroupEntity(
        ValueGetterMethod<CellGroup> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    public GroupEntity(
        TryRequestGenMethod<CellGroup> tryRequestGenMethod, Context c, string id)
        : base(tryRequestGenMethod, c, id)
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

    public EntityAttribute GetPolityWithHighestProminenceValueAttribute()
    {
        _polityWithHighestProminenceEntity =
            _polityWithHighestProminenceEntity ?? new PolityEntity(
                GetPolityWithHighestProminenceValue,
                Context,
                BuildAttributeId(PolityWithHighestProminenceValueAttributeId));

        return _polityWithHighestProminenceEntity.GetThisEntityAttribute(this);
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

    public EntityAttribute GetKnowledgesAttribute()
    {
        _knowledgesEntity =
            _knowledgesEntity ?? new CulturalKnowledgesEntity(
                GetCulture,
                Context,
                BuildAttributeId(KnowledgesAttributeId));

        return _knowledgesEntity.GetThisEntityAttribute(this);
    }

    private EntityAttribute GenerateGetRandomPolityEntityAttribute(IExpression[] arguments)
    {
        int index = _polityIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        IValueExpression<string> argumentExp = null;
        if (arguments.Length > 0)
        {
            argumentExp = ValueExpressionBuilder.ValidateValueExpression<string>(arguments[0]);
        }

        PolityEntity entity = new PolityEntity(
            () => {
                PolityType type = PolityEntity.ConvertToType(argumentExp?.Value);

                int offset = Group.GetHashCode() + iterOffset + Context.GetBaseOffset();
                return Group.GetRandomPolity(offset, type);
            },
            Context,
            BuildAttributeId($"random_polity_{index}"));

        _polityEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    private ValueGetterEntityAttribute<bool> GenerateHasPolityOfTypeAttribute(IExpression[] arguments)
    {
        IValueExpression<string> argumentExp = null;
        if (arguments.Length > 0)
        {
            argumentExp = ValueExpressionBuilder.ValidateValueExpression<string>(arguments[0]);
        }

        var attribute =
            new ValueGetterEntityAttribute<bool>(
                HasPolityOfTypeAttributeId, 
                this, 
                () => {
                    PolityType type = PolityEntity.ConvertToType(argumentExp?.Value);

                    return Group.HasPolityOfType(type);
                });

        return attribute;
    }

    private ValueGetterEntityAttribute<bool> GenerateHasFactionAttribute(IExpression[] arguments)
    {
        if (arguments.Length < 1)
        {
            throw new System.ArgumentException("get_faction: missing 'polity' argument");
        }

        var argumentExp =
            ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[0]);

        var attribute =
            new ValueGetterEntityAttribute<bool>(
                HasFactionAttributeId,
                this,
                () => {
                    if (argumentExp.Value is PolityEntity pEntity)
                    {
                        return Group.GetFaction(pEntity.Polity) != null;
                    }

                    throw new System.Exception(
                        $"Input parameter is not of a valid polity entity: {argumentExp.Value.GetType()}" +
                        $"\n - expression: {argumentExp}" +
                        $"\n - value: {argumentExp.ToPartiallyEvaluatedString()}");
                });

        return attribute;
    }

    private EntityAttribute GenerateGetFactionEntityAttribute(IExpression[] arguments)
    {
        int index = _factionIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        if (arguments.Length < 1)
        {
            throw new System.ArgumentException("get_faction: missing 'polity' argument");
        }

        var argumentExp = 
            ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[0]);

        FactionEntity entity = new FactionEntity(
            () => {
                if (argumentExp.Value is PolityEntity pEntity)
                {
                    Faction faction = Group.GetFaction(pEntity.Polity);

                    if (faction == null)
                    {
                        throw new System.Exception(
                            $"Faction not found. Validate if polity '{pEntity.Polity.Name.Text}' " +
                            $"is present in Group first, and then validate if the group is part of " +
                            $"a faction using 'has_faction' property." +
                            $"\n - {Context.DebugType}: {Context.Id}");
                    }

                    return faction;
                }

                throw new System.Exception(
                    $"Input parameter is not of a valid polity entity: {argumentExp.Value.GetType()}" +
                    $"\n - expression: {argumentExp}" +
                    $"\n - value: {argumentExp.ToPartiallyEvaluatedString()}");
            },
            Context,
            BuildAttributeId($"faction_{index}"));

        _factionEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    protected override object _reference => Group;

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case CellAttributeId:
                return GetCellAttribute();

            case ProminenceValueAttributeId:
                return new ProminenceValueAttribute(this, arguments);

            case FactionCoresCountAttributeId:
                _factionCoresCountAttribute =
                    _factionCoresCountAttribute ?? new ValueGetterEntityAttribute<float>(
                        FactionCoresCountAttributeId, this, () => Group.GetFactionCores().Count);
                return _factionCoresCountAttribute;

            case GetFactionCoreDistanceAttributeId:
                return new GetFactionCoreDistanceAttribute(this, arguments);

            case PreferencesAttributeId:
                return GetPreferencesAttribute();

            case KnowledgesAttributeId:
                return GetKnowledgesAttribute();

            case PolityWithHighestProminenceValueAttributeId:
                return GetPolityWithHighestProminenceValueAttribute();

            case GetRandomPolityAttributeId:
                return GenerateGetRandomPolityEntityAttribute(arguments);

            case HasPolityOfTypeAttributeId:
                return GenerateHasPolityOfTypeAttribute(arguments);

            case HasFactionAttributeId:
                return GenerateHasFactionAttribute(arguments);

            case GetFactionAttributeId:
                return GenerateGetFactionEntityAttribute(arguments);
        }

        throw new System.ArgumentException("Group: Unable to find attribute: " + attributeId);
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

        foreach (var polityEntity in _polityEntitiesToSet)
        {
            polityEntity.Reset();
        }

        foreach (var factionEntity in _factionEntitiesToSet)
        {
            factionEntity.Reset();
        }

        _cellEntity?.Reset();
        _polityWithHighestProminenceEntity?.Reset();

        _preferencesEntity?.Reset();
        _knowledgesEntity?.Reset();
    }

    public TerrainCell GetCell() => Group.Cell;

    public Polity GetPolityWithHighestProminenceValue() => Group.HighestPolityProminence?.Polity;

    public Culture GetCulture() => Group.Culture;
}
