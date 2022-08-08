using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[XmlInclude(typeof(CellCulturalPreference))]
public class CulturalPreference : CulturalPreferenceInfo
{
    [System.Obsolete]
    public const string AuthorityPreferenceId = "authority";
    [System.Obsolete]
    public const string CohesionPreferenceId = "cohesion";

    public const string IsolationPreferenceId = "isolation"; // Needs to remain hardcoded for now
    public const string AggressionPreferenceId = "aggression"; // Needs to remain hardcoded for now

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

    [XmlAttribute("V")]
    public float ValueInternal;

    [XmlIgnore]
    public virtual float Value
    {
        get => ValueInternal;
        set => ValueInternal = value;
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
