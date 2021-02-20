using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PolityEntity : DelayedSetEntity<Polity>
{
    public const string GetRandomGroupAttributeId = "get_random_group";
    public const string GetRandomContactAttributeId = "get_random_contact";
    public const string GetContactAttributeId = "get_contact";
    public const string DominantFactionAttributeId = "dominant_faction";
    public const string TransferInfluenceAttributeId = "transfer_influence";
    public const string TypeAttributeId = "type";
    public const string LeaderAttributeId = "leader";
    public const string ContactCountAttributeId = "contact_count";
    public const string FactionCountAttributeId = "faction_count";
    public const string SplitAttributeId = "split";
    public const string MergeAttributeId = "merge";
    public const string AccessibleNeighborRegionsAttributeId = "accessible_neighbor_regions";
    public const string AddCoreRegionAttributeId = "add_core_region";

    public virtual Polity Polity
    {
        get => Setable;
        private set => Setable = value;
    }

    protected override object _reference => Polity;

    private int _groupIndex = 0;
    private int _contactIndex = 0;

    private ValueGetterEntityAttribute<string> _typeAttribute;
    private ValueGetterEntityAttribute<float> _contactCountAttribute;
    private ValueGetterEntityAttribute<float> _factionCountAttribute;

    private AgentEntity _leaderEntity = null;
    private FactionEntity _dominantFactionEntity = null;

    private RegionCollectionEntity _accessibleNeighborRegionsEntity = null;

    private readonly List<GroupEntity>
        _groupEntitiesToSet = new List<GroupEntity>();

    private readonly List<ContactEntity>
        _contactEntitiesToSet = new List<ContactEntity>();

    public override string GetDebugString()
    {
        return "polity:" + Polity.GetName();
    }

    public override string GetFormattedString()
    {
        return Polity.Name.BoldText;
    }

    public PolityEntity(Context c, string id) : base(c, id)
    {
    }

    public PolityEntity(
        ValueGetterMethod<Polity> getterMethod, Context c, string id)
        : base(getterMethod, c, id)
    {
    }

    public EntityAttribute GetDominantFactionAttribute()
    {
        _dominantFactionEntity =
            _dominantFactionEntity ?? new FactionEntity(
            GetDominantFaction,
            Context,
            BuildAttributeId(DominantFactionAttributeId));

        return _dominantFactionEntity.GetThisEntityAttribute(this);
    }

    public EntityAttribute GetAccessibleNeighborRegionsAttribute()
    {
        _accessibleNeighborRegionsEntity =
            _accessibleNeighborRegionsEntity ?? new RegionCollectionEntity(
            GetAccessibleNeighborRegions,
            Context,
            BuildAttributeId(AccessibleNeighborRegionsAttributeId));

        return _accessibleNeighborRegionsEntity.GetThisEntityAttribute(this);
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

    private EntityAttribute GenerateGetRandomGroupEntityAttribute()
    {
        int index = _groupIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        GroupEntity entity = new GroupEntity(
            () => {
                int offset = Polity.GetHashCode() + iterOffset + Context.GetBaseOffset();
                return Polity.GetRandomGroup(offset);
            },
            Context,
            BuildAttributeId("random_group_" + index));

        _groupEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    private EntityAttribute GenerateGetRandomContactEntityAttribute()
    {
        int index = _contactIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        ContactEntity entity = new ContactEntity(
            () => {
                int offset = Polity.GetHashCode() + iterOffset + Context.GetBaseOffset();
                return Polity.GetRandomPolityContact(offset);
            },
            Context,
            BuildAttributeId("random_contact_" + index));

        _contactEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    private EntityAttribute GenerateGetContactEntityAttribute(IExpression[] arguments)
    {
        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException(
                GetContactAttributeId + ": number of arguments given less than 1");
        }

        IValueExpression<IEntity> polityArgument =
            ValueExpressionBuilder.ValidateValueExpression<IEntity>(arguments[0]);

        int index = _contactIndex++;

        ContactEntity entity = new ContactEntity(
            () => {
                PolityEntity polityEntity = polityArgument.Value as PolityEntity;

                if (polityEntity == null)
                {
                    throw new System.ArgumentException(
                        "split: invalid contact polity: " +
                        "\n - expression: " + ToString() +
                        "\n - contact polity: " + polityArgument.ToPartiallyEvaluatedString());
                }

                return Polity.GetContact(polityEntity.Polity);
            },
            Context,
            BuildAttributeId("contact_" + index));

        _contactEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    public Faction GetDominantFaction() => Polity.DominantFaction;

    public ICollection<Region> GetAccessibleNeighborRegions() => Polity.AccessibleNeighborRegions;

    public Agent GetLeader() => Polity.CurrentLeader;

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case TypeAttributeId:
                _typeAttribute =
                    _typeAttribute ?? new ValueGetterEntityAttribute<string>(
                        TypeAttributeId, this, () => Polity.Type);
                return _typeAttribute;

            case ContactCountAttributeId:
                _contactCountAttribute =
                    _contactCountAttribute ?? new ValueGetterEntityAttribute<float>(
                        ContactCountAttributeId, this, () => Polity.GetContacts().Count);
                return _contactCountAttribute;

            case FactionCountAttributeId:
                _factionCountAttribute =
                    _factionCountAttribute ?? new ValueGetterEntityAttribute<float>(
                        FactionCountAttributeId, this, () => Polity.FactionCount);
                return _factionCountAttribute;

            case LeaderAttributeId:
                return GetLeaderAttribute();

            case GetRandomGroupAttributeId:
                return GenerateGetRandomGroupEntityAttribute();

            case GetRandomContactAttributeId:
                return GenerateGetRandomContactEntityAttribute();

            case DominantFactionAttributeId:
                return GetDominantFactionAttribute();

            case TransferInfluenceAttributeId:
                return new TransferInfluenceAttribute(this, arguments);

            case GetContactAttributeId:
                return GenerateGetContactEntityAttribute(arguments);

            case SplitAttributeId:
                return new SplitPolityAttribute(this, arguments);

            case MergeAttributeId:
                return new MergePolityAttribute(this, arguments);

            case AccessibleNeighborRegionsAttributeId:
                return GetAccessibleNeighborRegionsAttribute();

            case AddCoreRegionAttributeId:
                return new AddCoreRegionAttribute(this, arguments);
        }

        throw new System.ArgumentException("Polity: Unable to find attribute: " + attributeId);
    }

    protected override void ResetInternal()
    {
        if (_isReset) return;

        foreach (GroupEntity groupEntity in _groupEntitiesToSet)
        {
            groupEntity.Reset();
        }

        foreach (ContactEntity contactEntity in _contactEntitiesToSet)
        {
            contactEntity.Reset();
        }

        _leaderEntity?.Reset();
        _dominantFactionEntity?.Reset();
        _accessibleNeighborRegionsEntity?.Reset();
    }
}
