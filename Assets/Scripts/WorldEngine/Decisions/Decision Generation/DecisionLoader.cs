using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Class used to load decision mod entries from mod JSON files.
/// Class properties should match the root structure of the JSON file.
/// </summary>
[Serializable]
public class DecisionLoader
{
#pragma warning disable 0649 // Disable warning for unitialized properties...

    public LoadedDecision[] decisions;

    [Serializable]
    public class LoadedDecision
    {
        [Serializable]
        public class LoadedDescriptionSegment
        {
            public string id;
            public string text;
            public string[] conditions;
        }

        [Serializable]
        public class LoadedOption
        {
            [Serializable]
            public class LoadedEffect
            {
                public string id;
                public string text;
                public string result;
            }

            public string id;
            public string text;
            public string[] conditions;
            public string weight;
            public LoadedEffect[] effects;
        }

        public string id;
        public string name;
        public string target;
        public Context.LoadedProperty[] properties;
        public LoadedDescriptionSegment[] description;
        public LoadedOption[] options;
    }

#pragma warning restore 0649

    public static IEnumerable<ModDecision> Load(string filename)
    {
        string jsonStr = File.ReadAllText(filename);

        // Load json object from file into intermediary object
        DecisionLoader loader = JsonUtility.FromJson<DecisionLoader>(jsonStr);

        for (int i = 0; i < loader.decisions.Length; i++)
        {
            ModDecision decision;
            try
            {
                decision = CreateDecision(loader.decisions[i]);
            }
            catch (Exception e)
            {
                // If theres a failure while loading a decision entry. Report
                // the file from which the event came from and its index within
                // the file...
                throw new Exception(
                    "Failure loading decision #" + i + " in " + filename + ": "
                    + e.Message, e);
            }

            yield return decision;
        }
    }

    private static DescriptionSegment CreateDescriptionSegment(
        ModDecision decision,
        LoadedDecision.LoadedDescriptionSegment ds)
    {
        if (string.IsNullOrEmpty(ds.id))
        {
            throw new ArgumentException("description 'id' can't be null or empty");
        }

        if (string.IsNullOrEmpty(ds.text))
        {
            throw new ArgumentException("description 'text' can't be null or empty");
        }

        DescriptionSegment segment = new DescriptionSegment(decision);

        IBooleanExpression[] conditions = null;

        if (ds.conditions != null)
        {
            // Build the condition expressions (must evaluate to bool values)
            conditions =
                ExpressionBuilder.BuildBooleanExpressions(segment, ds.conditions);
        }

        segment.Id = ds.id;
        segment.Text = ds.text;
        segment.Conditions = conditions;

        return segment;
    }

    private static ModDecision CreateDecision(LoadedDecision d)
    {
        if (string.IsNullOrEmpty(d.id))
        {
            throw new ArgumentException("decision 'id' can't be null or empty");
        }

        if (string.IsNullOrEmpty(d.name))
        {
            throw new ArgumentException("decision 'name' can't be null or empty");
        }

        if (string.IsNullOrEmpty(d.target))
        {
            throw new ArgumentException("decision 'target' can't be null or empty");
        }

        ModDecision decision = new ModDecision();

        if (d.properties != null)
        {
            foreach (Context.LoadedProperty lp in d.properties)
            {
                decision.AddPropertyEntity(lp);
            }
        }

        DescriptionSegment[] segments = null;

        decision.Id = d.id;
        decision.IdHash = d.id.GetHashCode();
        decision.Name = d.name;

        return decision;
    }
}
