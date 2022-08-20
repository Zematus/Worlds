using System.Collections.Generic;
using System;

public interface IEntity : IComparable<object>, IInputRequester, IFormattedStringGenerator
{
    string Id { get; }

    IEntity Parent { get; }

    EntityAttribute GetParametricAttribute(
        string attributeId,
        ParametricSubcontext subcontext,
        string[] paramIds,
        IExpression[] arguments);

    EntityAttribute GetAttribute(
        string attributeId, 
        IExpression[] arguments = null);

    ParametricSubcontext BuildParametricSubcontext(
        Context parentContext, 
        string attributeId, 
        string[] paramIds);

    string GetDebugString();

    string BuildAttributeId(string attrId);

    Context Context { get; }

    IValueExpression<IEntity> Expression { get; }

    void Set(object o);

    void Set(object o, PartiallyEvaluatedStringConverter converter);

    EntityAttribute GetThisEntityAttribute();

    string ToPartiallyEvaluatedString(int depth = -1);
}
