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
        public string[] executeConditions;
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

    /// <summary>
    /// Produces an action object from a single action entry
    /// </summary>
    /// <param name="e">The action entry</param>
    /// <returns>The resulting action</returns>
    private static ModAction CreateAction(LoadedAction e)
    {
        if (string.IsNullOrEmpty(e.id))
        {
            throw new ArgumentException("action 'id' can't be null or empty");
        }

        if (string.IsNullOrEmpty(e.name))
        {
            throw new ArgumentException("action 'name' can't be null or empty");
        }

        if (string.IsNullOrEmpty(e.target))
        {
            throw new ArgumentException("action 'target' can't be null or empty");
        }

        if (string.IsNullOrEmpty(e.category))
        {
            throw new ArgumentException("action 'category' can't be null or empty");
        }

        if (!ModAction.CategoryIds.Contains(e.category))
        {
            throw new ArgumentException("event 'category' is not supported: " + e.category);
        }

        if (e.effects == null)
        {
            throw new ArgumentException("event 'effects' list can't be empty");
        }

        ModAction action = new ModAction();
        action.Initialize(e);

        IValueExpression<bool>[] accessConditions = null;
        IValueExpression<bool>[] executeConditions = null;

        if (e.accessConditions != null)
        {
            // Build the access condition expressions (must evaluate to bool values)
            accessConditions =
                ValueExpressionBuilder.BuildValueExpressions<bool>(action, e.accessConditions);
        }

        if (e.executeConditions != null)
        {
            // Build the execute condition expressions (must evaluate to bool values)
            executeConditions =
                ValueExpressionBuilder.BuildValueExpressions<bool>(action, e.executeConditions);
        }

        // Build the effect expressions (must produce side effects)
        IEffectExpression[] effects =
            ExpressionBuilder.BuildEffectExpressions(action, e.effects, true);

        action.IdHash = e.id.GetHashCode();
        action.UId = ModAction.CurrentUId++;
        action.Name = e.name;
        action.Category = e.category;
        action.AccessConditions = accessConditions;
        action.ExecuteConditions = executeConditions;
        action.Effects = effects;

        return action;
    }
}
