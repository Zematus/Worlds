using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AgentEntity : DelayedSetEntity<Agent>
{
    public const string CharismaAttributeId = "charisma";
    public const string WisdomAttributeId = "wisdom";

    private ValueGetterEntityAttribute<float> _charismaAttribute;
    private ValueGetterEntityAttribute<float> _wisdomAttribute;

    public virtual Agent Agent
    {
        get => Setable;
        private set => Setable = value;
    }

    protected override object _reference => Agent;

    public AgentEntity(Context c, string id, IEntity parent) : base(c, id, parent)
    {
    }

    public AgentEntity(
        ValueGetterMethod<Agent> getterMethod, Context c, string id, IEntity parent)
        : base(getterMethod, c, id, parent)
    {
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case CharismaAttributeId:
                _charismaAttribute =
                    _charismaAttribute ?? new ValueGetterEntityAttribute<float>(
                        CharismaAttributeId, this, () => Mathf.Clamp01(Agent.Charisma / 20f));
                return _charismaAttribute;

            case WisdomAttributeId:
                _wisdomAttribute =
                    _wisdomAttribute ?? new ValueGetterEntityAttribute<float>(
                        WisdomAttributeId, this, () => Mathf.Clamp01(Agent.Wisdom / 20f));
                return _wisdomAttribute;
        }

        throw new System.ArgumentException("Agent: Unable to find attribute: " + attributeId);
    }

    public override string GetDebugString()
    {
        return "agent:" + Agent.Name.Text;
    }

    public override string GetFormattedString()
    {
        return Agent.Name.BoldText;
    }
}
