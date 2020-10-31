
public class AssignablePreferenceAttribute : AssignableValueEntityAttribute<float>
{
    private AssignableCulturalPreferencesEntity _preferencesEntity;
    private string _preferenceId;

    public AssignablePreferenceAttribute(
        AssignableCulturalPreferencesEntity preferencesEntity,
        string preferenceId)
        : base(preferenceId, preferencesEntity, null)
    {
        _preferencesEntity = preferencesEntity;
        _preferenceId = preferenceId;
    }

    public override float Value
    {
        get => GetValue();
        set => SetValue(value);
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

    private void SetValue(float value)
    {
        if (!value.IsInsideRange(0, 1))
        {
            throw new System.ArgumentException(
                "Cultural preference can only be assigned values in the range [0,1]");
        }

        CulturalPreference preference =
            _preferencesEntity.Culture.GetPreference(_preferenceId);

        if (preference == null)
        {
            preference = CreateInstance();
        }

        preference.Value = value;
    }

    private CulturalPreference CreateInstance()
    {
        throw new System.NotImplementedException();
    }
}
