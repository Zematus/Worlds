using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class GroupEffect : Effect
{
    public enum GroupType
    {
        This
    }

    public GroupType TargetType;

    protected GroupEffect(string typeStr, string id) : base(id)
    {
        switch (typeStr.Trim().ToLower())
        {
            case "this":
                TargetType = GroupType.This;
                break;

            default:
                throw new System.ArgumentException("Unhandled target type: " + typeStr);
        }
    }

    public override void Apply(CellGroup group)
    {
        switch (TargetType)
        {
            case GroupType.This:
                ApplyToTarget(group);
                break;

            default:
                throw new System.Exception("Unhandled GroupType: " + TargetType);
        }
    }

    public abstract void ApplyToTarget(CellGroup targetGroup);
}
