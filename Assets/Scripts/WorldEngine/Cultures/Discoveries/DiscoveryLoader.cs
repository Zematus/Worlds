using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;

[Serializable]
public class DiscoveryLoader
{
#pragma warning disable 0649

    public LoadedDiscovery[] discoveries;

    [Serializable]
    public class LoadedDiscovery
    {
        public string id;
        public string name;
        public string gainConditions;
        public string holdConditions;
        public string gainEffects;
        public string lossEffects;
        public int eventTimeToTrigger;
        public string eventTimeToTriggerFactors;
    }

#pragma warning restore 0649

    public static IEnumerable<Discovery> Load(string filename)
    {
        string jsonStr = File.ReadAllText(filename);

        DiscoveryLoader loader = JsonUtility.FromJson<DiscoveryLoader>(jsonStr);

        for (int i = 0; i < loader.discoveries.Length; i++)
        {
            yield return CreateDiscoveryClass(loader.discoveries[i]);
        }
    }

    private static Discovery CreateDiscoveryClass(LoadedDiscovery d)
    {
        if (string.IsNullOrEmpty(d.id))
        {
            throw new ArgumentException("discovery id can't be null or empty");
        }

        if (string.IsNullOrEmpty(d.name))
        {
            throw new ArgumentException("discovery name can't be null or empty");
        }

        if (!d.eventTimeToTrigger.IsInsideRange(1, int.MaxValue))
        {
            throw new ArgumentException("discovery event time-to-trigger must be a value between 0 and 2,147,483,647 (inclusive)");
        }

        Condition[] gainConditions = null;
        Condition[] holdConditions = null;
        Effect[] gainEffects = null;
        Effect[] lossEffects = null;
        Factor[] eventTimeToTriggerFactors = null;

        if (!string.IsNullOrEmpty(d.gainConditions))
        {
            //Cleanup and split list of conditions
            string c = Regex.Replace(d.gainConditions, ModUtility.FirstAndLastSingleQuoteRegex, "");
            string[] gainConditionsStr = Regex.Split(c, ModUtility.SeparatorSingleQuoteRegex);

            gainConditions = Condition.BuildConditions(gainConditionsStr);
        }

        if (!string.IsNullOrEmpty(d.holdConditions))
        {
            //Cleanup and split list of conditions
            string c = Regex.Replace(d.holdConditions, ModUtility.FirstAndLastSingleQuoteRegex, "");
            string[] holdConditionsStr = Regex.Split(c, ModUtility.SeparatorSingleQuoteRegex);

            holdConditions = Condition.BuildConditions(holdConditionsStr);
        }

        string effectId = d.id + "_discovery";

        if (!string.IsNullOrEmpty(d.gainEffects))
        {
            //Cleanup and split list of effects
            string e = Regex.Replace(d.gainEffects, ModUtility.FirstAndLastSingleQuoteRegex, "");
            string[] gainEffectsStr = Regex.Split(e, ModUtility.SeparatorSingleQuoteRegex);

            gainEffects = Effect.BuildEffects(gainEffectsStr, effectId);
        }

        if (!string.IsNullOrEmpty(d.lossEffects))
        {
            //Cleanup and split list of effects
            string e = Regex.Replace(d.lossEffects, ModUtility.FirstAndLastSingleQuoteRegex, "");
            string[] lossEffectsStr = Regex.Split(e, ModUtility.SeparatorSingleQuoteRegex);

            lossEffects = Effect.BuildEffects(lossEffectsStr, effectId);
        }

        if (!string.IsNullOrEmpty(d.eventTimeToTriggerFactors))
        {
            //Cleanup and split list of factors
            string f = Regex.Replace(d.eventTimeToTriggerFactors, ModUtility.FirstAndLastSingleQuoteRegex, "");
            string[] timeToTriggerFactorsStr = Regex.Split(f, ModUtility.SeparatorSingleQuoteRegex);

            eventTimeToTriggerFactors = Factor.BuildFactors(timeToTriggerFactorsStr);
        }

        Discovery discoveryClass = new Discovery()
        {
            Id = d.id,
            IdHash = d.id.GetHashCode(),
            UId = Discovery.CurrentUId++,
            Name = d.name,
            GainConditions = gainConditions,
            HoldConditions = holdConditions,
            GainEffects = gainEffects,
            LossEffects = lossEffects,
            EventTimeToTrigger = d.eventTimeToTrigger,
            EventTimeToTriggerFactors = eventTimeToTriggerFactors,
        };

        return discoveryClass;
    }
}
