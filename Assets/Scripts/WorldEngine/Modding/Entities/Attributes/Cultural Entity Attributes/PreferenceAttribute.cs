
public class PreferenceAttribute : ValueEntityAttribute<float>
{
    private CulturalPreferencesEntity _preferencesEntity;
    private string _preferenceId;

    public PreferenceAttribute(
        CulturalPreferencesEntity preferencesEntity,
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

        return preference.Value;
    }
}
