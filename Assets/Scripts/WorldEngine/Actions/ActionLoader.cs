using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

/// <summary>
/// Class used to load action mod entries from mod JSON files.
/// Class properties should match the root structure of the JSON file.
/// </summary>
[Serializable]
public class ActionLoader
{
#pragma warning disable 0649 // Disable warning for unitialized properties...

    /// <summary>
    /// list of loaded action entries.
    /// </summary>
    public LoadedAction[] actions;

    [Serializable]
    public class LoadedJustifiedCondition
    {
        public string condition;
        public string info;
    }

    /// <summary>
    /// Object defining an action entry. Structure must match that of
    /// an action entry in the mod file
    /// </summary>
    [Serializable]
    public class LoadedAction : Context.LoadedContext
    {
        public string name;
        public string target;
        public string category;
        public string[] accessConditions;
        public LoadedJustifiedCondition[] executeConditions;
        public string[] effects;
    }

#pragma warning restore 0649

    /// <summary>
    /// Produce a set of Action objects based on the contents an event mod file
    /// </summary>
    /// <param name="filename">Name of the JSON file with actions to load</param>
    /// <returns>A set of Action objects, each one relating to a single action entry
    /// on the file</returns>
    public static IEnumerable<ModAction> Load(string filename)
    {
        string jsonStr = File.ReadAllText(filename);

        // Load json object from file into intermediary object
        ActionLoader loader = JsonUtility.FromJson<ActionLoader>(jsonStr);

        for (int i = 0; i < loader.actions.Length; i++)
        {
            ModAction action;
            try
            {
                action = CreateAction(loader.actions[i]);
            }
            catch (Exception e)
            {
                // If there's a failure while loading an action entry, report
                // the file from which the action came from and its index within
                // the file...
                throw new Exception(
                    "Failure loading action #" + i + " in " + filename + ": "
                    + e.Message, e);
            }

            yield return action;
        }
    }

    private static JustifiedCondition CreateExecuteCondition(
        ModAction action,
        LoadedJustifiedCondition js)
    {
        var condition = new JustifiedCondition(action);

        condition.Info = new ModText(action, js.info);
        condition.Condition = ValueExpressionBuilder.BuildValueExpression<bool>(action, js.condition);

        return condition;
    }

    /// <summary>
    /// Produces an action object from a single action entry
    /// </summary>
    /// <param name="a">The action entry</param>
    /// <returns>The resulting action</returns>
    private static ModAction CreateAction(LoadedAction a)
    {
        if (string.IsNullOrEmpty(a.id))
        {
            throw new ArgumentException("action 'id' can't be null or empty");
        }

        if (string.IsNullOrEmpty(a.name))
        {
            throw new ArgumentException("action 'name' can't be null or empty");
        }

        if (string.IsNullOrEmpty(a.target))
        {
            throw new ArgumentException("action 'target' can't be null or empty");
        }

        if (string.IsNullOrEmpty(a.category))
        {
            throw new ArgumentException("action 'category' can't be null or empty");
        }

        if (!ModAction.CategoryIds.Contains(a.category))
        {
            throw new ArgumentException("event 'category' is not supported: " + a.category);
        }

        if (a.effects == null)
        {
            throw new ArgumentException("event 'effects' list can't be empty");
        }

        ModAction action = new ModAction();
        action.Initialize(a);

        IValueExpression<bool>[] accessConditions = null;
        JustifiedCondition[] executeConditions = null;

        if (a.accessConditions != null)
        {
            // Build the access condition expressions (must evaluate to bool values)
            accessConditions =
                ValueExpressionBuilder.BuildValueExpressions<bool>(action, a.accessConditions);
        }

        if (a.executeConditions != null)
        {
            executeConditions = new JustifiedCondition[a.executeConditions.Length];

            for (int i = 0; i < a.executeConditions.Length; i++)
            {
                try
                {
                    executeConditions[i] = CreateExecuteCondition(action, a.executeConditions[i]);
                }
                catch (Exception ex)
                {
                    // If there's a failure while loading a execute condition entry,
                    // report the index within the decision...
                    throw new Exception(
                        $"Failure loading justified condition #{i} in action '{a.id}': {ex.Message}", ex);
                }
            }
        }

        // Build the effect expressions (must produce side effects)
        IEffectExpression[] effects =
            ExpressionBuilder.BuildEffectExpressions(action, a.effects, true);

        action.IdHash = a.id.GetHashCode();
        action.UId = ModAction.CurrentUId++;
        action.Name = a.name;
        action.Category = a.category;
        action.AccessConditions = accessConditions;
        action.ExecuteConditions = executeConditions;
        action.Effects = effects;

        return action;
    }
}
