using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PolityEntity : Entity
{
    public const string GetRandomGroupAttributeId = "get_random_group";
    public const string GetRandomContactAttributeId = "get_random_contact";
    public const string DominantFactionAttributeId = "dominant_faction";
    public const string TransferInfluenceAttributeId = "transfer_influence";
    public const string ContactStrengthId = "contact_strength";
    public const string TypeAttributeId = "type";
    public const string LeaderAttributeId = "leader";
    public const string ContactsAttributeId = "contacts";

    public virtual Polity Polity { get; protected set; }

    public int RandomGroupIndex = 0;
    public int RandomContactIndex = 0;

    protected override object _reference => Polity;

    private ValueGetterEntityAttribute<string> _typeAttribute;

    private DelayedSetAgentEntity _leaderEntity = null;
    private DelayedSetFactionEntity _dominantFactionEntity = null;

    private bool _alreadyReset = false;

    private readonly List<RandomGroupEntity>
        _randomGroupEntitiesToSet = new List<RandomGroupEntity>();

    private readonly List<RandomContactEntity>
        _randomContactEntitiesToSet = new List<RandomContactEntity>();

    public override string GetDebugString()
    {
        return "polity:" + Polity.Name.Text;
    }

    public override string GetFormattedString()
    {
        return Polity.Name.BoldText;
    }

    public PolityEntity(Context c, string id) : base(c, id)
    {
    }

    public EntityAttribute GetDominantFactionAttribute()
    {
        _dominantFactionEntity =
            _dominantFactionEntity ?? new DelayedSetFactionEntity(
            GetDominantFaction,
            Context,
            BuildAttributeId(DominantFactionAttributeId));

        return _dominantFactionEntity.GetThisEntityAttribute(this);
    }

    public EntityAttribute GetLeaderAttribute()
    {
        _leaderEntity =
            _leaderEntity ?? new DelayedSetAgentEntity(
                GetLeader,
                Context,
                BuildAttributeId(LeaderAttributeId));

        return _leaderEntity.GetThisEntityAttribute(this);
    }

    private class RandomGroupEntity : GroupEntity
    {
        private int _index;
        private int _iterOffset;
        private PolityEntity _polityEntity;

        private CellGroup _group = null;

        public RandomGroupEntity(int index, PolityEntity entity)
            : base(entity.Context, entity.Id + ".random_group_" + index)
        {
            _index = index;
            _iterOffset = entity.Context.GetNextIterOffset() + index;
            _polityEntity = entity;
        }

        public void Reset()
        {
            _group = null;
        }

        public override CellGroup Group
        {
            get
            {
                if (_group == null)
                {
                    Polity polity = _polityEntity.Polity;

                    int offset = polity.GetHashCode() + _iterOffset + Context.GetBaseOffset();

                    _group = polity.GetRandomGroup(offset);

                    Set(_group);
                }

                return _group;
            }
        }
    }

    private class RandomContactEntity : ContactEntity
    {
        private int _index;
        private int _iterOffset;
        private PolityEntity _polityEntity;

        private PolityContact _contact = null;

        public RandomContactEntity(int index, PolityEntity entity)
            : base(entity.Context, entity.Id + ".random_contact_" + index)
        {
            _index = index;
            _iterOffset = entity.Context.GetNextIterOffset() + index;
            _polityEntity = entity;
        }

        public void Reset()
        {
            _contact = null;
        }

        public override PolityContact Contact
        {
            get
            {
                if (_contact == null)
                {
                    Polity polity = _polityEntity.Polity;

                    int offset = polity.GetHashCode() + _iterOffset + Context.GetBaseOffset();

                    _contact = polity.GetRandomPolityContact(offset);

                    Set(_contact);
                }

                return _contact;
            }
        }
    }

    private EntityAttribute GenerateRandomGroupEntityAttribute()
    {
        int groupIndex = RandomGroupIndex++;

        RandomGroupEntity entity = new RandomGroupEntity(groupIndex, this);

        _randomGroupEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    private EntityAttribute GenerateRandomContactEntityAttribute()
    {
        int contactIndex = RandomContactIndex++;

        RandomContactEntity entity = new RandomContactEntity(contactIndex, this);

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

            case ContactStrengthId:
                return new ContactStrengthAttribute(this, arguments);
        }

        throw new System.ArgumentException("Polity: Unable to find attribute: " + attributeId);
    }

    protected void ResetInternal()
    {
        if (_alreadyReset)
        {
            return;
        }

        foreach (RandomGroupEntity groupEntity in _randomGroupEntitiesToSet)
        {
            groupEntity.Reset();
        }

        foreach (RandomContactEntity contactEntity in _randomContactEntitiesToSet)
        {
            contactEntity.Reset();
        }

        _leaderEntity?.Reset();
        _dominantFactionEntity?.Reset();

        _alreadyReset = true;
    }

    public void Set(Polity p)
    {
        Polity = p;

        ResetInternal();

        _alreadyReset = false;
    }

    public override void Set(object o)
    {
        if (o is PolityEntity e)
        {
            Set(e.Polity);
        }
        else if (o is Polity p)
        {
            Set(p);
        }
        else
        {
            throw new System.ArgumentException("Unexpected type: " + o.GetType());
        }
    }
}
