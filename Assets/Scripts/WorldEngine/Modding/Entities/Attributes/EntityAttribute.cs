﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class EntityAttribute : IInputRequester
{
    public string Id;
    public IEntity Entity;
    public IExpression[] Arguments;

    private IExpression _attrExpression = null;

    public virtual bool RequiresInput
    {
        get
        {
            if (Arguments != null)
            {
                foreach (IExpression e in Arguments)
                {
                    if (e.RequiresInput)
                        return true;
                }
            }

            return false;
        }
    }

    public EntityAttribute(string id, IEntity entity, IExpression[] arguments)
    {
        Id = id;
        Entity = entity;
        Arguments = arguments;
    }

    protected abstract IExpression BuildExpression();

    public IExpression GetExpression()
    {
        _attrExpression = _attrExpression ?? BuildExpression();

        return _attrExpression;
    }

    public override string ToString()
    {
        return Entity.Id + "." + Id;
    }

    public virtual string ToPartiallyEvaluatedString(bool evaluate) => ToString();

    public virtual bool TryGetRequest(out InputRequest request)
    {
        if (Arguments != null)
        {
            foreach (IExpression e in Arguments)
            {
                if (e.TryGetRequest(out request))
                    return true;
            }
        }

        request = null;

        return false;
    }
}
