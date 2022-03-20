using UnityEngine;

/// <summary>
/// A temporary culture object used to hold and transfer cultural property values
/// </summary>
public class BufferCulture : Culture
{
    /// <summary>
    /// Seserialization constructor.
    /// NOTE: this object shouldn't be serialized
    /// </summary>
    public BufferCulture()
    {
        throw new System.InvalidOperationException(
            "BufferCulture objects shouldn't be serialized");
    }

    /// <summary>
    /// Constructs a new buffer culture from a base culture
    /// </summary>
    /// <param name="baseCulture">the culture to use as reference</param>
    public BufferCulture(Culture baseCulture) : base(baseCulture)
    {
    }

    /// <summary>
    /// Merge another culture's properties by a proportion
    /// </summary>
    /// <param name="sourceCulture">the culture to merge properties from</param>
    /// <param name="percentage">the percentage or merging</param>
    public void MergeCulture(Culture sourceCulture, float percentage)
    {
        foreach (CulturalPreference p in sourceCulture.GetPreferences())
        {
            CulturalPreference preference;

            if (_preferences.TryGetValue(p.Id, out preference))
            {
                preference.Value = Mathf.Lerp(preference.Value, p.Value, percentage);
            }
            else
            {
                preference = new CulturalPreference(p);
                AddPreference(preference);
                preference.Value *= percentage;
            }
        }

        foreach (CulturalActivity a in sourceCulture.GetActivities())
        {
            CulturalActivity activity;

            if (_activities.TryGetValue(a.Id, out activity))
            {
                activity.Value = Mathf.Lerp(activity.Value, a.Value, percentage);
            }
            else
            {
                activity = new CulturalActivity(a);
                AddActivity(activity);
                activity.Value *= percentage;
            }
        }

        foreach (CulturalSkill s in sourceCulture.GetSkills())
        {
            CulturalSkill skill;

            if (_skills.TryGetValue(s.Id, out skill))
            {
                skill.Value = Mathf.Lerp(skill.Value, s.Value, percentage);
            }
            else
            {
                skill = new CulturalSkill(s);
                AddSkill(skill);
                skill.Value *= percentage;
            }
        }

        foreach (CulturalKnowledge k in sourceCulture.GetKnowledges())
        {
            CulturalKnowledge knowledge;

            if (_knowledges.TryGetValue(k.Id, out knowledge))
            {
                knowledge.Value =
                    MathUtility.LerpToIntAndGetDecimals(
                        knowledge.Value,
                        k.Value,
                        percentage,
                        out _);
            }
            else
            {
                knowledge = new CulturalKnowledge(k);
                AddKnowledge(knowledge);
                knowledge.Value =
                    MathUtility.LerpToIntAndGetDecimals(
                        0,
                        k.Value,
                        percentage,
                        out _);
            }
        }

        foreach (var d in sourceCulture.Discoveries.Values)
        {
            AddDiscovery(d);
        }
    }
}
