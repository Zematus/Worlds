using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

// Cultural Preferences
// -- Authority
// -- Cohesion
// -- Isolation

[XmlInclude(typeof(CellCulturalPreference))]
public class CulturalPreference : CulturalPreferenceInfo
{
    public static HashSet<string> Preferences;

    public const string AuthorityPreferenceId = "authority";
    public const string CohesionPreferenceId = "cohesion";
    public const string IsolationPreferenceId = "isolation";

    public const string AuthorityPreferenceName = "Authority";
    public const string CohesionPreferenceName = "Cohesion";
    public const string IsolationPreferenceName = "Isolation";

    public const int AuthorityPreferenceRngOffset = 0;
    public const int CohesionPreferenceRngOffset = 1;
    public const int IsolationPreferenceRngOffset = 2;

    [XmlAttribute]
    public float Value;

    public static void InitializePreferences()
    {
        Preferences = new HashSet<string>
        {
            AuthorityPreferenceId,
            CohesionPreferenceId,
            IsolationPreferenceId
        };
    }

    public CulturalPreference()
    {
    }

    public CulturalPreference(string id, string name, int rngOffset, float value) : base(id, name, rngOffset)
    {
        Value = value;
    }

    public CulturalPreference(CulturalPreference basePreference) : base(basePreference)
    {
        Value = basePreference.Value;
    }

    public void Reset()
    {
        Value = 0;
    }
}
