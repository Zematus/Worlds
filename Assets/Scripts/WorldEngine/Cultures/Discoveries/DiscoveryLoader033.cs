using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;

[Serializable]
public class DiscoveryLoader033
{
#pragma warning disable 0649

    public LoadedDiscovery[] discoveries;

    [Serializable]
    public class LoadedDiscovery
    {
        public string id;
        public string name;
        public string[] gainConditions;
        public string[] holdConditions;
        public string[] gainEffects;
        public string[] lossEffects;
        public long eventTimeToTrigger;
        public string[] eventTimeToTriggerFactors;
    }

#pragma warning restore 0649

    public static IEnumerable<Discovery033> Load(string filename)
    {
        string jsonStr = File.ReadAllText(filename);

        DiscoveryLoader033 loader = JsonUtility.FromJson<DiscoveryLoader033>(jsonStr);

        for (int i = 0; i < loader.discoveries.Length; i++)
        {
            yield return CreateDiscovery(loader.discoveries[i]);
        }
    }

    private static Discovery033 CreateDiscovery(LoadedDiscovery d)
    {
        if (string.IsNullOrEmpty(d.id))
        {
            throw new ArgumentException("discovery id can't be null or empty");
        }

        if (string.IsNullOrEmpty(d.name))
        {
            throw new ArgumentException("discovery name can't be null or empty");
        }

        if (!d.eventTimeToTrigger.IsInsideRange(1, long.MaxValue))
        {
            throw new ArgumentException("discovery event time-to-trigger must be a value between 0 and 9,223,372,036,854,775,807 (inclusive)");
        }

        Condition[] gainConditions = null;
        Condition[] holdConditions = null;
        Effect[] gainEffects = null;
        Effect[] lossEffects = null;
        Factor[] eventTimeToTriggerFactors = null;

        if (d.gainConditions != null)
        {
            gainConditions = Condition.BuildConditions(d.gainConditions);
        }

        if (d.holdConditions != null)
        {
            holdConditions = Condition.BuildConditions(d.holdConditions);
        }

        string effectId = d.id + "_discovery";

        if (d.gainEffects != null)
        {
            gainEffects = Effect.BuildEffects(d.gainEffects, effectId);
        }

        if (d.lossEffects != null)
        {
            lossEffects = Effect.BuildEffects(d.lossEffects, effectId);
        }

        if (d.eventTimeToTriggerFactors != null)
        {
            eventTimeToTriggerFactors = Factor.BuildFactors(d.eventTimeToTriggerFactors);
        }

        Discovery033 discovery = new Discovery033()
        {
            Id = d.id,
            IdHash = d.id.GetHashCode(),
            UId = Manager.CurrentDiscoveryUid++,
            Name = d.name,
            GainConditions = gainConditions,
            HoldConditions = holdConditions,
            GainEffects = gainEffects,
            LossEffects = lossEffects,
            EventTimeToTrigger = d.eventTimeToTrigger,
            EventTimeToTriggerFactors = eventTimeToTriggerFactors,
        };

        return discovery;
    }
}
