using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AgentEntity : Entity
{
    public const string CharismaAttributeId = "charisma";
    public const string WisdomAttributeId = "wisdom";

    private ValueGetterEntityAttribute<float> _charismaAttribute;
    private ValueGetterEntityAttribute<float> _wisdomAttribute;

    public Agent Agent;

    protected override object _reference => Agent;

    public AgentEntity(string id) : base(id)
    {
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case CharismaAttributeId:
                _charismaAttribute =
                    _charismaAttribute ?? new ValueGetterEntityAttribute<float>(
                        CharismaAttributeId, this, () => Agent.Charisma);
                return _charismaAttribute;

            case WisdomAttributeId:
                _wisdomAttribute =
                    _wisdomAttribute ?? new ValueGetterEntityAttribute<float>(
                        WisdomAttributeId, this, () => Agent.Wisdom);
                return _wisdomAttribute;
        }

        throw new System.ArgumentException("Agent: Unable to find attribute: " + attributeId);
    }

    public void Set(Agent agent)
    {
        Agent = agent;
    }

    public override string GetFormattedString()
    {
        return Agent.Name.BoldText;
    }
}
