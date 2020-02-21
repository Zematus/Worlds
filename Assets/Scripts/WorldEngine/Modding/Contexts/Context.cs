using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Context objects contain data used by sets of expressions to resolve references and receive input
/// from the simulation
/// </summary>
public abstract class Context
{
    /// <summary>
    /// List of expressions that can be referenced by other expressions using this context
    /// </summary>
    readonly public Dictionary<string, IExpression> Expressions = new Dictionary<string, IExpression>();
    /// <summary>
    /// List of entities that can be referenced by expressions using this context
    /// </summary>
    readonly public Dictionary<string, Entity> Entities = new Dictionary<string, Entity>();
}
