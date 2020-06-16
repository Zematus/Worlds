using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DelayedSetAgentEntity : AgentEntity
{
    private ValueGetterMethod<Agent> _getterMethod;

    private Agent _agent = null;

    public DelayedSetAgentEntity(ValueGetterMethod<Agent> getterMethod, Context c, string id)
        : base(c, id)
    {
        _getterMethod = getterMethod;
    }

    public void Reset()
    {
        _agent = null;
    }

    public override Agent Agent
    {
        get
        {
            if (_agent == null)
            {
                _agent = _getterMethod();

                Set(_agent);
            }

            return _agent;
        }
    }
}
