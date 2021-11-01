using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PolityEntity : DelayedSetEntity<Polity>
{
    public const string GetRandomGroupAttributeId = "get_random_group";
    public const string GetRandomContactAttributeId = "get_random_contact";
    public const string GetContactAttributeId = "get_contact";
    public const string ContactsAttributeId = "contacts";
    public const string DominantFactionAttributeId = "dominant_faction";
    public const string TransferInfluenceAttributeId = "transfer_influence";
    public const string TypeAttributeId = "type";
    public const string LeaderAttributeId = "leader";
    public const string ContactCountAttributeId = "contact_count";
    public const string FactionCountAttributeId = "faction_count";
    public const string SplitAttributeId = "split";
    public const string MergeAttributeId = "merge";
    public const string NeighborRegionsAttributeId = "neighbor_regions";
    public const string AddCoreRegionAttributeId = "add_core_region";
    public const string CoreRegionSaturationAttributeId = "core_region_saturation";
    public const string GetFactionsAttributeId = "get_factions";

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
    private ValueGetterEntityAttribute<float> _coreRegionSaturationAttribute;

    private AgentEntity _leaderEntity = null;
    private FactionEntity _dominantFactionEntity = null;

    private RegionCollectionEntity _neighborRegionsEntity = null;

    private ContactCollectionEntity _contactsEntity = null;

    private int _factionCollectionIndex = 0;

    private List<FactionCollectionEntity> _factionCollectionEntitiesToSet =
        new List<FactionCollectionEntity>();

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

    public EntityAttribute GetNeighborRegionsAttribute()
    {
        _neighborRegionsEntity =
            _neighborRegionsEntity ?? new RegionCollectionEntity(
            GetNeighborRegions,
            Context,
            BuildAttributeId(NeighborRegionsAttributeId));

        return _neighborRegionsEntity.GetThisEntityAttribute(this);
    }

    public EntityAttribute GetContactsAttribute()
    {
        _contactsEntity =
            _contactsEntity ?? new ContactCollectionEntity(
            GetContacts,
            Context,
            BuildAttributeId(ContactsAttributeId));

        return _contactsEntity.GetThisEntityAttribute(this);
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

    public static PolityType ConvertToType(string typeStr)
    {
        typeStr = typeStr.ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(typeStr))
        {
            return PolityType.Any;
        }

        switch (typeStr)
        {
            case "tribe":
                return PolityType.Tribe;
            default:
                throw new System.Exception($"Unhandled polity type: {typeStr}");
        }
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

        var polityArgument =
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

    public ICollection<Region> GetNeighborRegions() => Polity.NeighborRegions;

    public ICollection<PolityContact> GetContacts() => Polity.GetContacts();

    public Agent GetLeader() => Polity.CurrentLeader;

    public ParametricSubcontext BuildGetFactionsAttributeSubcontext(
        Context parentContext,
        string[] paramIds)
    {
        int index = _factionCollectionIndex;

        if ((paramIds == null) || (paramIds.Length < 1))
        {
            throw new System.ArgumentException(
                $"{GetFactionsAttributeId}: expected at least one parameter identifier");
        }

        var subcontext =
            new ParametricSubcontext(
                $"{GetFactionsAttributeId}_{index}",
                parentContext);

        var factionEntity = new FactionEntity(subcontext, paramIds[0]);
        subcontext.AddEntity(factionEntity);

        return subcontext;
    }

    public override ParametricSubcontext BuildParametricSubcontext(
        Context parentContext,
        string attributeId,
        string[] paramIds)
    {
        switch (attributeId)
        {
            case GetFactionsAttributeId:
                return BuildGetFactionsAttributeSubcontext(parentContext, paramIds);
        }

        return base.BuildParametricSubcontext(parentContext, attributeId, paramIds);
    }

    public EntityAttribute GetFactionAttribute(
        ParametricSubcontext subcontext,
        string[] paramIds,
        IExpression[] arguments)
    {
        int index = _factionCollectionIndex++;

        if ((paramIds == null) || (paramIds.Length < 1))
        {
            throw new System.ArgumentException(
                GetFactionsAttributeId + ": expected one parameter identifier");
        }

        var paramEntity = subcontext.GetEntity(paramIds[0]) as FactionEntity;

        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException(
                GetFactionsAttributeId + ": expected one condition argument");
        }

        var conditionExp = ValueExpressionBuilder.ValidateValueExpression<bool>(arguments[0]);

        var collectionEntity = new FactionCollectionEntity(
            () =>
            {
                var selectedFactions = new HashSet<Faction>();

                foreach (var faction in Polity.GetFactions())
                {
                    paramEntity.Set(faction);

                    if (conditionExp.Value)
                    {
                        selectedFactions.Add(faction);
                    }
                }

                return selectedFactions;
            },
            Context,
            BuildAttributeId($"factions_collection_{index}"));

        _factionCollectionEntitiesToSet.Add(collectionEntity);

        return collectionEntity.GetThisEntityAttribute(this);
    }

    public override EntityAttribute GetParametricAttribute(
        string attributeId,
        ParametricSubcontext subcontext,
        string[] paramIds,
        IExpression[] arguments)
    {
        switch (attributeId)
        {
            case GetFactionsAttributeId:
                return GetFactionAttribute(subcontext, paramIds, arguments);
        }

        return base.GetParametricAttribute(attributeId, subcontext, paramIds, arguments);
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case TypeAttributeId:
                _typeAttribute =
                    _typeAttribute ?? new ValueGetterEntityAttribute<string>(
                        TypeAttributeId, this, () => Polity.TypeStr);
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

            case NeighborRegionsAttributeId:
                return GetNeighborRegionsAttribute();

            case ContactsAttributeId:
                return GetContactsAttribute();

            case AddCoreRegionAttributeId:
                return new AddCoreRegionAttribute(this, arguments);

            case CoreRegionSaturationAttributeId:
                _coreRegionSaturationAttribute =
                    _coreRegionSaturationAttribute ?? new ValueGetterEntityAttribute<float>(
                        CoreRegionSaturationAttributeId, this, () => Polity.CoreRegionSaturation);
                return _coreRegionSaturationAttribute;
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

        foreach (var entity in _factionCollectionEntitiesToSet)
        {
            entity.Reset();
        }

        _leaderEntity?.Reset();
        _dominantFactionEntity?.Reset();
        _neighborRegionsEntity?.Reset();
        _contactsEntity?.Reset();
    }
}
