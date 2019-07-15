using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class Effect
{
    public string Id;

    public Effect(string id)
    {
        Id = id;
    }

    public static Effect BuildEffect(string effectStr, string id)
    {
        Match match = Regex.Match(effectStr, AddGroupKnowledgeEffect.Regex);
        if (match.Success == true)
        {
            return new AddGroupKnowledgeEffect(match, id);
        }

        match = Regex.Match(effectStr, RemoveGroupKnowledgeEffect.Regex);
        if (match.Success == true)
        {
            return new RemoveGroupKnowledgeEffect(match, id);
        }

        match = Regex.Match(effectStr, ModifyGroupKnowledgeLimitEffect.Regex);
        if (match.Success == true)
        {
            return new ModifyGroupKnowledgeLimitEffect(match, id);
        }

        match = Regex.Match(effectStr, AddGroupActivityEffect.Regex);
        if (match.Success == true)
        {
            return new AddGroupActivityEffect(match, id);
        }

        match = Regex.Match(effectStr, RemoveGroupActivityEffect.Regex);
        if (match.Success == true)
        {
            return new RemoveGroupActivityEffect(match, id);
        }

        throw new System.ArgumentException("Not a recognized effect: " + effectStr);
    }

    public static Effect[] BuildEffects(ICollection<string> effectStrs, string id)
    {
        Effect[] effects = new Effect[effectStrs.Count];

        int i = 0;
        foreach (string effectStr in effectStrs)
        {
            effects[i++] = BuildEffect(effectStr, id);
        }

        return effects;
    }

    public abstract void Apply(CellGroup group);
}
