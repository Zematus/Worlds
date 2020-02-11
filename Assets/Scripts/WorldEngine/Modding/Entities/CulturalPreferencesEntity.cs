using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CulturalPreferencesEntity : Entity
{
    public Culture Culture;

    public class PreferenceAttribute : NumericEntityAttribute
    {
        private CulturalPreferencesEntity _preferencesEntity;
        private string _preferenceId;

        public PreferenceAttribute(CulturalPreferencesEntity preferencesEntity, string preferenceId)
        {
            _preferencesEntity = preferencesEntity;
            _preferenceId = preferenceId;
        }

        public override float GetValue()
        {
            CulturalPreference preference = _preferencesEntity.Culture.GetPreference(_preferenceId);

            if (preference == null)
            {
                return 0;
            }

            return preference.Value;
        }
    }

    public override EntityAttribute GetAttribute(string attributeId, Expression[] arguments = null)
    {
        if (!CulturalPreference.Preferences.Contains(attributeId))
        {
            throw new System.ArgumentException(
                "Unrecognized cultural preference in entity attribute: " + attributeId);
        }

        return new PreferenceAttribute(this, attributeId);
    }

    public void Set(Culture culture)
    {
        Culture = culture;
    }
}
