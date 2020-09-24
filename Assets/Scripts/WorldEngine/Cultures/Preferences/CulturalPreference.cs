using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[XmlInclude(typeof(CellCulturalPreference))]
public class CulturalPreference : CulturalPreferenceInfo
{
    // NOTE: Some preference value Ids might need to remain hardcoded
    public const string AuthorityPreferenceId = "authority";
    public const string CohesionPreferenceId = "cohesion";
    public const string IsolationPreferenceId = "isolation";
    public const string AggressionPreferenceId = "aggression";

    [System.Obsolete]
    public const string AuthorityPreferenceName = "Authority";
    [System.Obsolete]
    public const string CohesionPreferenceName = "Cohesion";
    [System.Obsolete]
    public const string IsolationPreferenceName = "Isolation";

    [System.Obsolete]
    public const int AuthorityPreferenceRngOffset = 0;
    [System.Obsolete]
    public const int CohesionPreferenceRngOffset = 1;
    [System.Obsolete]
    public const int IsolationPreferenceRngOffset = 2;

    [XmlAttribute]
    public float Value;

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
