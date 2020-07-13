using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PolityEntity : Entity
{
    public const string GetRandomGroupAttributeId = "get_random_group";
    public const string DominantFactionAttributeId = "dominant_faction";
    public const string TransferInfluenceAttributeId = "transfer_influence";
    public const string TypeAttributeId = "type";

    public virtual Polity Polity { get; protected set; }

    public int RandomGroupIndex = 0;

    protected override object _reference => Polity;

    private ValueGetterEntityAttribute<string> _typeAttribute;

    private DelayedSetFactionEntity _dominantFactionEntity = null;

    private bool _alreadyReset = false;

    private readonly List<RandomGroupEntity>
        _randomGroupEntitiesToSet = new List<RandomGroupEntity>();

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

    private EntityAttribute GenerateRandomGroupEntityAttribute()
    {
        int groupIndex = RandomGroupIndex++;

        RandomGroupEntity entity = new RandomGroupEntity(groupIndex, this);

        _randomGroupEntitiesToSet.Add(entity);

        return entity.GetThisEntityAttribute(this);
    }

    public Faction GetDominantFaction() => Polity.DominantFaction;

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case TypeAttributeId:
                _typeAttribute =
                    _typeAttribute ?? new ValueGetterEntityAttribute<string>(
                        TypeAttributeId, this, () => Polity.Type);
                return _typeAttribute;

            case GetRandomGroupAttributeId:
                return GenerateRandomGroupEntityAttribute();

            case DominantFactionAttributeId:
                return GetDominantFactionAttribute();

            case TransferInfluenceAttributeId:
                return new TransferInfluenceAttribute(this, arguments);
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
