using System.Collections.Generic;
using UnityEngine.Profiling;

/// <summary>
/// Player accessible scriptable action
/// </summary>
public class ModAction : Context, IDebugLogger
{
    public const string TerritoryCategoryId = "territory";
    public const string DiplomacyCategoryId = "diplomacy";
    //public const string CultureCategoryId = "culture";

    public static HashSet<string> CategoryIds;

    public const string FactionTargetType = "faction";

    public const string TargetEntityId = "target";

    public static Dictionary<string, ModAction> Actions;
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
    /// Category to use when sorting this into the action toolbar sections
    /// </summary>
    public string Category;

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
    public IValueExpression<bool>[] ExecuteConditions;

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
        Actions = new Dictionary<string, ModAction>();
        CategoryIds = new HashSet<string>();
    }

    public static void LoadActionFile(string filename)
    {
        foreach (ModAction action in ActionLoader.Load(filename))
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

    public ModAction()
    {
        Target = new FactionEntity(this, TargetEntityId);

        // Add the target to the context's entity map
        AddEntity(Target);
    }

    public static ModAction GetAction(string id)
    {
        return !Actions.TryGetValue(id, out ModAction a) ? null : a;
    }

    public bool CanAccess()
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

    public bool CanExecute()
    {
        OpenDebugOutput("Evaluating Use Conditions:");

        // Always check that the target is still valid
        if (!CanAccess())
        {
            CloseDebugOutput("Use Result: False");
            return false;
        }

        if (ExecuteConditions != null)
        {
            foreach (IValueExpression<bool> exp in ExecuteConditions)
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

    public bool Execute()
    {
        foreach (IEffectExpression exp in Effects)
        {
            exp.Apply();
        }

        return true;
    }

    public void SetTarget(Faction faction)
    {
        if (faction == null)
        {
            throw new System.ArgumentNullException("faction is set to null");
        }

        Reset();

        Target.Set(faction);
    }

    public override int GetNextRandomInt(int iterOffset, int maxValue) =>
        Target.Faction.GetNextLocalRandomInt(iterOffset, maxValue);

    public override float GetNextRandomFloat(int iterOffset) =>
        Target.Faction.GetNextLocalRandomFloat(iterOffset);

    public override int GetBaseOffset() => Target.Faction.GetHashCode();
}
