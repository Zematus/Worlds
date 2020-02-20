using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Defines a context for event associated expressions to use
/// </summary>
public class EventContext : Context
{
    public const string TargetId = "target";

    /// <summary>
    /// The target of the event to trigger
    /// </summary>
    public Entity Target;
    /// <summary>
    /// The type of the event target
    /// </summary>
    public string TargetType;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="targetStr">String identifying the type of target</param>
    public EventContext(string targetStr)
    {
        SetTarget(targetStr);
    }

    /// <summary>
    /// Sets the target entity for the event based in the type of target
    /// </summary>
    /// <param name="targetStr"></param>
    private void SetTarget(string targetStr)
    {
        switch (targetStr)
        {
            case EventGenerator.FactionTargetType:
                Target = new FactionEntity(TargetId);
                break;
            case EventGenerator.GroupTargetType:
                Target = new GroupEntity(TargetId);
                break;
            default:
                throw new System.ArgumentException("Invalid target type: " + targetStr);
        }

        TargetType = targetStr;

        // Add the target to the context's entity map
        Entities.Add(TargetId, Target);
    }
}
