using System.Collections.Generic;
using System;

public interface IEntity : IComparable<object>, IInputRequester
{
    string Id { get; }

    EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null);

    string GetFormattedString();

    string GetDebugString();

    string BuildAttributeId(string attrId);

    IValueExpression<IEntity> Expression { get; }

    void Set(object o);

    void Set(object o, PartiallyEvaluatedStringConverter converter);

    EntityAttribute GetThisEntityAttribute(Entity parent);

    string ToPartiallyEvaluatedString(bool evaluate);
}
