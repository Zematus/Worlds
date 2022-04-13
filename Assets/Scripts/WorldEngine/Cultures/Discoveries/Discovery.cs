using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class Discovery : Context, IDiscovery, IEffectTrigger
{
    public const string TargetEntityId = "target";

    public string Name { get; set; }
    public int UId { get; set; }

    /// <summary>
    /// Effects to occur when the discovery is gained
    /// </summary>
    public IEffectExpression[] GainEffects;

    /// <summary>
    /// Effects to occur when the discovery is lost
    /// </summary>
    public IEffectExpression[] LossEffects;

    public static Dictionary<string, Discovery> Discoveries;

    private readonly GroupEntity _target;

    public static void LoadDiscoveriesFile(string filename)
    {
        foreach (Discovery discovery in DiscoveryLoader.Load(filename))
        {
            if (Discoveries.ContainsKey(discovery.Id))
            {
                Discoveries[discovery.Id] = discovery;
            }
            else
            {
                Discoveries.Add(discovery.Id, discovery);
            }
        }
    }

    public static void ResetDiscoveries()
    {
        Discoveries = new Dictionary<string, Discovery>();
    }

    public Discovery()
    {
        DebugType = "Discovery";

        _target = new GroupEntity(this, TargetEntityId, null);

        // Add the target to the context's entity map
        AddEntity(_target);
    }

    private void SetTarget(CellGroup group)
    {
        Reset();

        _target.Set(group);
    }

    private void ApplyEffects(CellGroup group, IEffectExpression[] effects)
    {
        SetTarget(group);

        OpenDebugOutput($"Applying {Name} Discovery Gain Effects:");

        foreach (IEffectExpression exp in effects)
        {
            AddExpDebugOutput("Effect", exp);

            exp.Trigger = this;
            exp.Apply();
        }

        CloseDebugOutput();
    }

    public void OnGain(CellGroup group) => ApplyEffects(group, GainEffects);
    public void OnLoss(CellGroup group) => ApplyEffects(group, LossEffects);

    public override float GetNextRandomFloat(int iterOffset) =>
        _target.Group.GetNextLocalRandomFloat(iterOffset);

    public override int GetNextRandomInt(int iterOffset, int maxValue) =>
        _target.Group.GetNextLocalRandomInt(iterOffset, maxValue);

    public override int GetBaseOffset() =>
        _target.Group.GetHashCode();

#if DEBUG
    private Dictionary<IEffectExpression, long> _lastUseDates = new Dictionary<IEffectExpression, long>();

    public long GetLastUseDate(IEffectExpression expression)
    {
        if (_lastUseDates.ContainsKey(expression))
        {
            return _lastUseDates[expression];
        }

        return -1;
    }

    public void SetLastUseDate(IEffectExpression expression, long date)
    {
        _lastUseDates[expression] = date;
    }
#endif
}
