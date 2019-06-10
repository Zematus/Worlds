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
        public string isPresentAtStart;
    }

#pragma warning restore 0649

    public static IEnumerable<DiscoveryClass> Load(string filename)
    {
        string jsonStr = File.ReadAllText(filename);

        DiscoveryLoader loader = JsonUtility.FromJson<DiscoveryLoader>(jsonStr);

        for (int i = 0; i < loader.discoveries.Length; i++)
        {
            yield return CreateDiscoveryClass(loader.discoveries[i]);
        }
    }

    private static DiscoveryClass CreateDiscoveryClass(LoadedDiscovery d)
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

        string[] gainConditions = null;

        if (!string.IsNullOrEmpty(d.gainConditions))
        {
            //Cleanup and split list of conditions
            string c = Regex.Replace(d.gainConditions, ModUtility.FirstAndLastSingleQuoteRegex, "");
            gainConditions = Regex.Split(c, ModUtility.SeparatorSingleQuoteRegex);
        }

        DiscoveryClass discoveryClass = new DiscoveryClass()
        {
            Id = d.id,
            IdHash = d.id.GetHashCode(),
            Name = d.name,
            EventTimeToTrigger = d.eventTimeToTrigger
        };

        return discoveryClass;
    }
}
