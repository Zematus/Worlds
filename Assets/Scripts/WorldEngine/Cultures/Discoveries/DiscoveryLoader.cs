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
        public string[] gainEffects;
        public string[] lossEffects;
    }

#pragma warning restore 0649

    public static IEnumerable<Discovery> Load(string filename)
    {
        string jsonStr = File.ReadAllText(filename);

        var loader = JsonUtility.FromJson<DiscoveryLoader>(jsonStr);

        for (int i = 0; i < loader.discoveries.Length; i++)
        {
            yield return CreateDiscovery(loader.discoveries[i]);
        }
    }

    private static Discovery CreateDiscovery(LoadedDiscovery d)
    {
        if (string.IsNullOrEmpty(d.id))
        {
            throw new ArgumentException("discovery id can't be null or empty");
        }

        if (string.IsNullOrEmpty(d.name))
        {
            throw new ArgumentException("discovery name can't be null or empty");
        }

        Discovery discovery = new Discovery()
        {
            Id = d.id,
            UId = Manager.CurrentDiscoveryUid++,
            Name = d.name
        };

        // Build the gain effect expressions (must produce side effects)
        IEffectExpression[] gainEffects =
            ExpressionBuilder.BuildEffectExpressions(discovery, d.gainEffects);

        // Build the loss effect expressions (must produce side effects)
        IEffectExpression[] lossEffects =
            ExpressionBuilder.BuildEffectExpressions(discovery, d.lossEffects);

        discovery.GainEffects = gainEffects;
        discovery.LossEffects = lossEffects;

        return discovery;
    }
}
