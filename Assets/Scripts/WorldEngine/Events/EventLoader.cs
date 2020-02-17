using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

[Serializable]
public class EventLoader
{
#pragma warning disable 0649

    public LoadedEvent[] events;

    [Serializable]
    public class LoadedEvent
    {
        public string id;
        public string name;
        public string target;
        public string[] assignmentConditions;
        public string[] triggerConditions;
        public string timeToTrigger;
        public string[] effects;
    }

#pragma warning restore 0649

    public static IEnumerable<EventGenerator> Load(string filename)
    {
        string jsonStr = File.ReadAllText(filename);

        EventLoader loader = JsonUtility.FromJson<EventLoader>(jsonStr);

        for (int i = 0; i < loader.events.Length; i++)
        {
            EventGenerator generator = null;

            try
            {
                generator = CreateEventGenerator(loader.events[i]);
            }
            catch (Exception e)
            {
                throw new Exception(
                    "Failure loading event #" + i + " in " + filename + ": "
                    + e.Message, e);
            }

            yield return generator;
        }
    }

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

        if (string.IsNullOrEmpty(e.timeToTrigger))
        {
            throw new ArgumentException("'timeToTrigger' can't be null or empty");
        }

        if (e.effects == null)
        {
            throw new ArgumentException("'effects' list can't be empty");
        }

        EventContext context = new EventContext(e.target);

        IBooleanExpression[] assignmentConditions = null;
        IBooleanExpression[] triggerConditions = null;
        INumericExpression timeToTrigger = null;
        IEffectExpression[] effects = null;

        if (e.assignmentConditions != null)
        {
            assignmentConditions =
                ExpressionBuilder.BuildBooleanExpressions(context, e.assignmentConditions);
        }

        if (e.triggerConditions != null)
        {
            triggerConditions =
                ExpressionBuilder.BuildBooleanExpressions(context, e.triggerConditions);
        }

        timeToTrigger = ExpressionBuilder.ValidateNumericExpression(
            ExpressionBuilder.BuildExpression(context, e.timeToTrigger));

        effects = ExpressionBuilder.BuildEffectExpressions(context, e.effects);

        EventGenerator generator = new EventGenerator()
        {
            Id = e.id,
            IdHash = e.id.GetHashCode(),
            UId = EventGenerator.CurrentUId++,
            Name = e.name,
            AssignmentConditions = assignmentConditions,
            TriggerConditions = triggerConditions,
            TimeToTrigger = timeToTrigger,
            Effects = effects
        };

        return generator;
    }
}
