using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

/// <summary>
/// Class used to load event mod entries from mod JSON files.
/// Class properties should match the root structure of the JSON file.
/// </summary>
[Serializable]
public class EventLoader
{
#pragma warning disable 0649 // Disable warning for unitialized properties...

    /// <summary>
    /// list of loaded event entries.
    /// </summary>
    public LoadedEvent[] events;

    /// <summary>
    /// Object defininf an event entry. Structure must match that of
    /// an event entry in the mod file
    /// </summary>
    [Serializable]
    public class LoadedEvent
    {
        public string id;
        public string name;
        public string target;
        public string[] assigners;
        public string[] assignmentConditions;
        public string[] triggerConditions;
        public string maxTimeToTrigger;
        public string[] effects;
        public bool repeatable;
    }

#pragma warning restore 0649

    /// <summary>
    /// Produce a set of Event Generator objects based on the contents an event mod file
    /// </summary>
    /// <param name="filename">Name of the JSON file with events to load</param>
    /// <returns>A set of Event Generator objects,
    /// each one relating to a single event entry on the file</returns>
    public static IEnumerable<EventGenerator> Load(string filename)
    {
        string jsonStr = File.ReadAllText(filename);

        // Load json object from file into intermediary object
        EventLoader loader = JsonUtility.FromJson<EventLoader>(jsonStr);

        for (int i = 0; i < loader.events.Length; i++)
        {
            EventGenerator generator;
            try
            {
                generator = CreateEventGenerator(loader.events[i]);
            }
            catch (Exception e)
            {
                // If theres a failure while loading an event entry. Report
                // the file from which the event came from and its index within
                // the file..
                throw new Exception(
                    "Failure loading event #" + i + " in " + filename + ": "
                    + e.Message, e);
            }

            yield return generator;
        }
    }

    /// <summary>
    /// Produces a generator object from a single event entry
    /// </summary>
    /// <param name="e">The event entry</param>
    /// <returns>The resulting event generator</returns>
    private static EventGenerator CreateEventGenerator(LoadedEvent e)
    {
        if (string.IsNullOrEmpty(e.id))
        {
            throw new ArgumentException("'id' can't be null or empty");
        }

        if (string.IsNullOrEmpty(e.name))
        {
            throw new ArgumentException("'name' can't be null or empty");
        }

        if (string.IsNullOrEmpty(e.target))
        {
            throw new ArgumentException("'target' can't be null or empty");
        }

        if (e.assigners == null)
        {
            throw new ArgumentException("'assigners' list can't be empty");
        }

        if (string.IsNullOrEmpty(e.maxTimeToTrigger))
        {
            throw new ArgumentException("'timeToTrigger' can't be null or empty");
        }

        if (e.effects == null)
        {
            throw new ArgumentException("'effects' list can't be empty");
        }

        // Generate a new event generator
        EventGenerator generator = EventGenerator.BuildGenerator(e.target);

        IBooleanExpression[] assignmentConditions = null;
        IBooleanExpression[] triggerConditions = null;

        if (e.assignmentConditions != null)
        {
            // Build the assignment condition expressions (must evaluate to bool values)
            assignmentConditions =
                ExpressionBuilder.BuildBooleanExpressions(generator, e.assignmentConditions);
        }

        if (e.triggerConditions != null)
        {
            // Build the trigger condition expressions (must evaluate to bool values)
            triggerConditions =
                ExpressionBuilder.BuildBooleanExpressions(generator, e.triggerConditions);
        }

        // Build the time-to-trigger expression (must evaluate to a number (int or float))
        INumericExpression maxTimeToTrigger = ExpressionBuilder.ValidateNumericExpression(
            ExpressionBuilder.BuildExpression(generator, e.maxTimeToTrigger));

        // Build the effect expressions (must produce side effects)
        IEffectExpression[] effects = ExpressionBuilder.BuildEffectExpressions(generator, e.effects);

        generator.Id = e.id;
        generator.IdHash = e.id.GetHashCode();
        generator.UId = EventGenerator.CurrentUId++;
        generator.Name = e.name;
        generator.Assigners = e.assigners;
        generator.AssignmentConditions = assignmentConditions;
        generator.TriggerConditions = triggerConditions;
        generator.MaxTimeToTrigger = maxTimeToTrigger;
        generator.Effects = effects;
        generator.Repeteable = e.repeatable;

        return generator;
    }
}
