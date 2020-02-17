using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class EventContext : Context
{
    public const string TargetId = "target";

    public Entity Target;
    public string TargetType;

    public EventContext(string targetStr)
    {
        SetTarget(targetStr);
    }

    private void SetTarget(string targetStr)
    {
        switch (targetStr)
        {
            case Event.FactionTargetType:
                Target = new FactionEntity(TargetId);
                break;
            case Event.GroupTargetType:
                Target = new GroupEntity(TargetId);
                break;
            default:
                throw new System.ArgumentException("Invalid target type: " + targetStr);
        }

        TargetType = targetStr;

        Entities.Add(TargetId, Target);
    }
}
