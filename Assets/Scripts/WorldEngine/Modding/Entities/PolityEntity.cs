using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PolityEntity : DelayedSetEntity<Polity>
{
    public const string GetRandomGroupAttributeId = "get_random_group";
    public const string GetRandomContactAttributeId = "get_random_contact";
    public const string DominantFactionAttributeId = "dominant_faction";
    public const string TransferInfluenceAttributeId = "transfer_influence";
    public const string TypeAttributeId = "type";
    public const string LeaderAttributeId = "leader";
    public const string ContactCountAttributeId = "contact_count";
    public const string FactionCountAttributeId = "faction_count";
    public const string SplitAttributeId = "split";

    public virtual Polity Polity
    {
        get => Setable;
        private set => Setable = value;
    }

    public int RandomGroupIndex = 0;
    public int RandomContactIndex = 0;

    protected override object _reference => Polity;

    private ValueGetterEntityAttribute<string> _typeAttribute;
    private ValueGetterEntityAttribute<float> _contactCountAttribute;
    private ValueGetterEntityAttribute<float> _factionCountAttribute;

    private AgentEntity _leaderEntity = null;
    private FactionEntity _dominantFactionEntity = null;

    private readonly List<GroupEntity>
        _randomGroupEntitiesToSet = new List<GroupEntity>();

    private readonly List<ContactEntity>
        _randomContactEntitiesToSet = new List<ContactEntity>();

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

    public EntityAttribute GetLeaderAttribute()
    {
        _leaderEntity =
            _leaderEntity ?? new AgentEntity(
                GetLeader,
                Context,
                BuildAttributeId(LeaderAttributeId));

        return _leaderEntity.GetThisEntityAttribute(this);
    }

    private EntityAttribute GenerateRandomGroupEntityAttribute()
    {
        int index = RandomGroupIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        GroupEntity entity = new GroupEntity(
            () => {
                int offset = Polity.GetHashCode() + iterOffset + Context.GetBaseOffset();
                return Polity.GetRandomGroup(offset);
            },
            Context,
            BuildAttributeId("random_group_" + index));

        _randomGroupEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    private EntityAttribute GenerateRandomContactEntityAttribute()
    {
        int index = RandomContactIndex++;
        int iterOffset = Context.GetNextIterOffset() + index;

        ContactEntity entity = new ContactEntity(
            () => {
                int offset = Polity.GetHashCode() + iterOffset + Context.GetBaseOffset();
                return Polity.GetRandomPolityContact(offset);
            },
            Context,
            BuildAttributeId("random_contact_" + index));

        _randomContactEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    public Faction GetDominantFaction() => Polity.DominantFaction;

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
                        ContactCountAttributeId, this, () => Polity.GetPolityContacts().Count);
                return _contactCountAttribute;

            case FactionCountAttributeId:
                _factionCountAttribute =
                    _factionCountAttribute ?? new ValueGetterEntityAttribute<float>(
                        FactionCountAttributeId, this, () => Polity.FactionCount);
                return _factionCountAttribute;

            case LeaderAttributeId:
                return GetLeaderAttribute();

            case GetRandomGroupAttributeId:
                return GenerateRandomGroupEntityAttribute();

            case GetRandomContactAttributeId:
                return GenerateRandomContactEntityAttribute();

            case DominantFactionAttributeId:
                return GetDominantFactionAttribute();

            case TransferInfluenceAttributeId:
                return new TransferInfluenceAttribute(this, arguments);

            case SplitAttributeId:
                return new SplitPolityAttribute(this, arguments);
        }

        throw new System.ArgumentException("Polity: Unable to find attribute: " + attributeId);
    }

    protected override void ResetInternal()
    {
        if (_isReset) return;

        foreach (GroupEntity groupEntity in _randomGroupEntitiesToSet)
        {
            groupEntity.Reset();
        }

        foreach (ContactEntity contactEntity in _randomContactEntitiesToSet)
        {
            contactEntity.Reset();
        }

        _leaderEntity?.Reset();
        _dominantFactionEntity?.Reset();
    }
}
