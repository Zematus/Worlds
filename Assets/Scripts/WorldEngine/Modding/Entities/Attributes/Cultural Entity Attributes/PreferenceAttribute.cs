using UnityEngine;

public class PreferenceAttribute : ValueEntityAttribute<float>
{
    private ICulturalPreferencesEntity _preferencesEntity;
    private string _preferenceId;

    public PreferenceAttribute(
        ICulturalPreferencesEntity preferencesEntity,
        string preferenceId)
        : base(preferenceId, preferencesEntity, null)
    {
        _preferencesEntity = preferencesEntity;
        _preferenceId = preferenceId;
    }

    public override float Value
    {
        get => GetValue();
    }

    private float GetValue()
    {
        CulturalPreference preference =
            _preferencesEntity.Culture.GetPreference(_preferenceId);

        if (preference == null)
        {
            return 0;
        }

#if DEBUG
        if ((preference.Value <= 0) || (preference.Value >= 1))
        {
            Debug.LogWarning($"Preference value not between 0 and 1: {preference.Value}");
        }
#endif

        return preference.Value;
    }
}
