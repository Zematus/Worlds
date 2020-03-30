using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PolityEntity : Entity
{
    public const string GetRandomGroupAttributeId = "get_random_group";

    public Polity Polity { get; private set; }

    protected override object _reference => Polity;

    public int RandomGroupId = 0;

    public List<GroupEntity> RandomGroupEntitiesToSet = new List<GroupEntity>();

    public override string GetFormattedString()
    {
        return Polity.Name.BoldText;
    }

    public class GetRandomGroupAttribute : ValueEntityAttribute<Entity>
    {
        private PolityEntity _polityEntity;

        public GetRandomGroupAttribute(PolityEntity polityEntity, IExpression[] arguments)
            : base(GetRandomGroupAttributeId, polityEntity, arguments)
        {
            _polityEntity = polityEntity;
        }

        public override Entity Value
        {
            get
            {
                GroupEntity entity = new GroupEntity(
                    _polityEntity.Id + ".random_group_" + _polityEntity.RandomGroupId++);

                _polityEntity.RandomGroupEntitiesToSet.Add(entity);

                return entity;
            }
        }
    }

    public PolityEntity(string id) : base(id)
    {
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case GetRandomGroupAttributeId:
                return new GetRandomGroupAttribute(this, arguments);
        }

        throw new System.ArgumentException("Faction: Unable to find attribute: " + attributeId);
    }

    public void Set(Polity polity)
    {
        Polity = polity;

        int offset = (int)polity.Id;

        foreach (GroupEntity entity in RandomGroupEntitiesToSet)
        {
            entity.Set(Polity.GetRandomGroup(offset++));
        }
    }
}
