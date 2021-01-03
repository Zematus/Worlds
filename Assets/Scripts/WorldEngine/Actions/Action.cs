using System.Collections.Generic;
using UnityEngine.Profiling;

/// <summary>
/// Player accessible scriptable action
/// </summary>
public class Action : Context, IDebugLogger
{
    public const string FactionTargetType = "faction";

    public const string TargetEntityId = "target";

    public static Dictionary<string, Action> Actions;
    public readonly FactionEntity Target;

    /// <summary>
    /// Global UId counter
    /// </summary>
    public static long CurrentUId = StartUId;

    /// <summary>
    /// The UId to use for this action type
    /// </summary>
    public long UId;

    /// <summary>
    /// Name to use in the UI for this action type
    /// </summary>
    public string Name;

    /// <summary>
    /// Hash to use for RNGs that use this action type
    /// </summary>
    public int IdHash;

    /// <summary>
    /// Conditions that decide if this action can be accessed with the target
    /// </summary>
    public IValueExpression<bool>[] AccessConditions;

    /// <summary>
    /// Conditions that decide if this action can be used with the target
    /// </summary>
    public IValueExpression<bool>[] UseConditions;

    /// <summary>
    /// Effects to occur after the action triggers
    /// </summary>
    public IEffectExpression[] Effects;

    /// <summary>
    /// First UId to use for actions loaded from mods
    /// </summary>
    protected const long StartUId = 0;

    public static void ResetActions()
    {
        Actions = new Dictionary<string, Action>();
    }

    public static void LoadActionFile(string filename)
    {
        foreach (Action action in ActionLoader.Load(filename))
        {
            if (Actions.ContainsKey(action.Id))
            {
                Actions[action.Id] = action;
            }
            else
            {
                Actions.Add(action.Id, action);
            }
        }
    }

    public Action()
    {
        Target = new FactionEntity(this, TargetEntityId);

        // Add the target to the context's entity map
        AddEntity(Target);
    }

    public static void InitializeActions()
    {
        foreach (Action action in Actions.Values)
        {
            action.Initialize();
        }
    }

    public virtual void Initialize()
    {
        World.Actions.Add(Id, this);
    }

    public static Action GetAction(string id)
    {
        return !Actions.TryGetValue(id, out Action a) ? null : a;
    }

    protected bool CanAccess()
    {
        OpenDebugOutput("Evaluating Access Conditions:");

        if (AccessConditions != null)
        {
            foreach (IValueExpression<bool> exp in AccessConditions)
            {
                bool value = exp.Value;

                if (DebugEnabled)
                {
                    string expStr = exp.ToString();
                    string expPartialStr = exp.ToPartiallyEvaluatedString(true);

                    AddDebugOutput("  Condition: " + expStr +
                     "\n   - Partial eval: " + expPartialStr +
                     "\n   - Result: " + value);
                }

                if (!value)
                {
                    CloseDebugOutput("Access Result: False");
                    return false;
                }
            }
        }

        CloseDebugOutput("Access Result: True");
        return true;
    }

    public bool CanUse()
    {
        OpenDebugOutput("Evaluating Use Conditions:");

        // Always check that the target is still valid
        if (!CanAccess())
        {
            CloseDebugOutput("Use Result: False");
            return false;
        }

        if (UseConditions != null)
        {
            foreach (IValueExpression<bool> exp in UseConditions)
            {
                bool value = exp.Value;

                if (DebugEnabled)
                {
                    string expStr = exp.ToString();
                    string expPartialStr = exp.ToPartiallyEvaluatedString(true);

                    AddDebugOutput("  Condition: " + expStr +
                     "\n   - Partial eval: " + expPartialStr +
                     "\n   - Result: " + value);
                }

                if (!value)
                {
                    CloseDebugOutput("Use Result: False");
                    return false;
                }
            }
        }

        CloseDebugOutput("Use Result: True");
        return true;
    }

    public void Trigger()
    {
        foreach (IEffectExpression exp in Effects)
        {
            exp.Apply();
        }
    }

    public override int GetNextRandomInt(int iterOffset, int maxValue) =>
        Target.Faction.GetNextLocalRandomInt(iterOffset, maxValue);

    public override float GetNextRandomFloat(int iterOffset) =>
        Target.Faction.GetNextLocalRandomFloat(iterOffset);

    public override int GetBaseOffset() => Target.Faction.GetHashCode();
}
