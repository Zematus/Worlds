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

    public Dictionary<int, GroupEntity> RandomGroupEntitiesToSet =
        new Dictionary<int, GroupEntity>();

    public override string GetFormattedString()
    {
        return Polity.Name.BoldText;
    }

    public Entity GetRandomGroupEntity()
    {
        int groupId = RandomGroupId++;

        GroupEntity entity =
            new GroupEntity(BuildInternalEntityId("random_group_" + groupId));

        RandomGroupEntitiesToSet.Add(groupId, entity);

        return entity;
    }

    public PolityEntity(string id) : base(id)
    {
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case GetRandomGroupAttributeId:
                return new ValueGetterEntityAttribute<Entity>(
                    GetRandomGroupAttributeId, this, GetRandomGroupEntity);
        }

        throw new System.ArgumentException("Faction: Unable to find attribute: " + attributeId);
    }

    public override void Set(object o)
    {
        if ((Polity = o as Polity) == null)
        {
            throw new System.Exception("Entity reference is not of type " + typeof(Polity));
        }

        int offset = (int)Polity.Id;

        foreach (KeyValuePair<int, GroupEntity> pair in RandomGroupEntitiesToSet)
        {
            pair.Value.Set(Polity.GetRandomGroup(offset + pair.Key));
        }
    }
}
