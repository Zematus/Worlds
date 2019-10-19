using ProtoBuf;

// Cultural Preferences
// -- Authority
// -- Cohesion
// -- Isolation

[ProtoContract]
[ProtoInclude(100, typeof(CellCulturalPreference))]
public class CulturalPreference : CulturalPreferenceInfo
{
    public const string AuthorityPreferenceId = "AuthorityPreference";
    public const string CohesionPreferenceId = "CohesionPreference";
    public const string IsolationPreferenceId = "IsolationPreference";

    public const string AuthorityPreferenceName = "Authority";
    public const string CohesionPreferenceName = "Cohesion";
    public const string IsolationPreferenceName = "Isolation";

    public const int AuthorityPreferenceRngOffset = 0;
    public const int CohesionPreferenceRngOffset = 1;
    public const int IsolationPreferenceRngOffset = 2;

    [ProtoMember(1)]
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
