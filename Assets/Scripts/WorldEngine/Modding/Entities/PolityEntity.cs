using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PolityEntity : Entity
{
    public const string GetRandomGroupAttributeId = "get_random_group";

    public Polity Polity { get; private set; }

    protected override object _reference => Polity;

    public int RandomGroupIndex = 0;

    private readonly List<RandomGroupEntity>
        _randomGroupEntitiesToSet = new List<RandomGroupEntity>();

    public override string GetFormattedString()
    {
        return Polity.Name.BoldText;
    }

    public PolityEntity(string id) : base(id)
    {
    }

    private class RandomGroupEntity : GroupEntity
    {
        private int _index;
        private PolityEntity _polityEntity;

        private CellGroup _group = null;

        public RandomGroupEntity(int index, PolityEntity entity)
            : base(entity.Id + ".random_group_" + index)
        {
            _index = index;
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

                    int offset = (int)polity.Id;

                    _group = polity.GetRandomGroup(offset + _index);

                    Set(_group);
                }

                return _group;
            }
        }
    }

    private FixedValueEntityAttribute<Entity> GenerateRandomGroupEntity()
    {
        int groupIndex = RandomGroupIndex++;

        RandomGroupEntity entity = new RandomGroupEntity(groupIndex, this);

        _randomGroupEntitiesToSet.Add(entity);

        return new FixedValueEntityAttribute<Entity>(entity, entity.Id, this);
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case GetRandomGroupAttributeId:
                return GenerateRandomGroupEntity();
        }

        throw new System.ArgumentException("Polity: Unable to find attribute: " + attributeId);
    }

    public void Set(Polity p)
    {
        Polity = p;

        foreach (RandomGroupEntity groupEntity in _randomGroupEntitiesToSet)
        {
            groupEntity.Reset();
        }
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
