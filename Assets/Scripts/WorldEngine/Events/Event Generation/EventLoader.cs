using UnityEngine;
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
    /// Object defining an event entry. Structure must match that of
    /// an event entry in the mod file
    /// </summary>
    [Serializable]
    public class LoadedEvent : Context.LoadedContext
    {
        public string name;
        public string target;
        public string[] assignOn;
        public string[] assignmentConditions;
        public string[] triggerConditions;
        public string timeToTrigger;
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
                // If there's a failure while loading an event entry, report
                // the file from which the event came from and its index within
                // the file...
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
            throw new ArgumentException("event 'id' can't be null or empty");
        }

        if (string.IsNullOrEmpty(e.name))
        {
            throw new ArgumentException("event 'name' can't be null or empty");
        }

        if (string.IsNullOrEmpty(e.target))
        {
            throw new ArgumentException("event 'target' can't be null or empty");
        }

        if (e.assignOn == null)
        {
            throw new ArgumentException("event 'assignOn' list can't be empty");
        }

        if (string.IsNullOrEmpty(e.timeToTrigger))
        {
            throw new ArgumentException("event 'timeToTrigger' can't be null or empty");
        }

        if (e.effects == null)
        {
            throw new ArgumentException("event 'effects' list can't be empty");
        }

        EventGenerator generator = EventGenerator.BuildGenerator(e.target);
        generator.Initialize(e);

        IValueExpression<bool>[] assignmentConditions = null;
        IValueExpression<bool>[] triggerConditions = null;

        if (e.assignmentConditions != null)
        {
            // Build the assignment condition expressions (must evaluate to bool values)
            assignmentConditions =
                ValueExpressionBuilder.BuildValueExpressions<bool>(generator, e.assignmentConditions);
        }

        if (e.triggerConditions != null)
        {
            // Build the trigger condition expressions (must evaluate to bool values)
            triggerConditions =
                ValueExpressionBuilder.BuildValueExpressions<bool>(generator, e.triggerConditions);
        }

        // Build the time-to-trigger expression (must evaluate to a number (int or float))
        IValueExpression<float> timeToTrigger =
            ValueExpressionBuilder.BuildValueExpression<float>(generator, e.timeToTrigger);

        // Build the effect expressions (must produce side effects)
        IEffectExpression[] effects =
            ExpressionBuilder.BuildEffectExpressions(generator, e.effects);

        generator.IdHash = e.id.GetHashCode();
        generator.UId = EventGenerator.CurrentUId++;
        generator.Name = e.name;
        generator.AssignOn = e.assignOn;
        generator.AssignmentConditions = assignmentConditions;
        generator.TriggerConditions = triggerConditions;
        generator.TimeToTrigger = timeToTrigger;
        generator.Effects = effects;
        generator.Repeteable = e.repeatable;

        return generator;
    }
}
