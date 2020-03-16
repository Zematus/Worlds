using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AgentEntity : Entity
{
    public const string CharismaAttributeId = "charisma";
    public const string WisdomAttributeId = "wisdom";

    private CharismaAttribute _charismaAttribute;
    private WisdomAttribute _wisdomAttribute;

    public Agent Agent;

    protected override object _reference => Agent;

    public class CharismaAttribute : NumericEntityAttribute
    {
        private AgentEntity _agentEntity;

        public CharismaAttribute(AgentEntity agentEntity)
            : base(CharismaAttributeId, agentEntity, null)
        {
            _agentEntity = agentEntity;
        }

        public override float Value => _agentEntity.Agent.Charisma;
    }

    public class WisdomAttribute : NumericEntityAttribute
    {
        private AgentEntity _agentEntity;

        public WisdomAttribute(AgentEntity agentEntity)
            : base(CharismaAttributeId, agentEntity, null)
        {
            _agentEntity = agentEntity;
        }

        public override float Value => _agentEntity.Agent.Wisdom;
    }

    public AgentEntity(string id) : base(id)
    {
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case CharismaAttributeId:
                _charismaAttribute =
                    _charismaAttribute ?? new CharismaAttribute(this);
                return _charismaAttribute;

            case WisdomAttributeId:
                _wisdomAttribute =
                    _wisdomAttribute ?? new WisdomAttribute(this);
                return _wisdomAttribute;
        }

        throw new System.ArgumentException("Agent: Unable to find attribute: " + attributeId);
    }

    public void Set(Agent agent)
    {
        Agent = agent;
    }
}
