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

    public Dictionary<int, GroupEntity> RandomGroupEntitiesToSet =
        new Dictionary<int, GroupEntity>();

    public override string GetFormattedString()
    {
        return Polity.Name.BoldText;
    }

    public PolityEntity(string id) : base(id)
    {
    }

    private FixedValueEntityAttribute<Entity> GenerateRandomGroupEntity()
    {
        int groupIndex = RandomGroupIndex++;
        string groupId = "random_group_" + groupIndex;

        GroupEntity entity =
            new GroupEntity(BuildInternalEntityId(groupId));

        RandomGroupEntitiesToSet.Add(groupIndex, entity);

        return new FixedValueEntityAttribute<Entity>(entity, groupId, this);
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

        int offset = (int)Polity.Id;

        foreach (KeyValuePair<int, GroupEntity> pair in RandomGroupEntitiesToSet)
        {
            pair.Value.Set(Polity.GetRandomGroup(offset + pair.Key));
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
