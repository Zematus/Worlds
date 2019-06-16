using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Effect
{
    public static Effect BuildEffect(string effectStr)
    {
        Match match = Regex.Match(effectStr, GroupGainsKnowledgeEffect.Regex);
        if (match.Success == true)
        {
            return new GroupGainsKnowledgeEffect(match);
        }

        throw new System.ArgumentException("Not a recognized effect: " + effectStr);
    }

    public static Effect[] BuildEffects(ICollection<string> effectStrs)
    {
        Effect[] effects = new Effect[effectStrs.Count];

        int i = 0;
        foreach (string effectStr in effectStrs)
        {
            effects[i++] = BuildEffect(effectStr);
        }

        return effects;
    }

    public abstract void Apply(CellGroup group);
}
