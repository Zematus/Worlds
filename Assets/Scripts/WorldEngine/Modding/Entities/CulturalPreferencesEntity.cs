using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CulturalPreferencesEntity : Entity
{
    public Culture Culture;

    protected override object _reference => Culture;

    public class PreferenceAttribute : ValueEntityAttribute<float>
    {
        private CulturalPreferencesEntity _preferencesEntity;
        private string _preferenceId;

        public PreferenceAttribute(
            CulturalPreferencesEntity preferencesEntity,
            string preferenceId,
            IExpression[] arguments)
            : base(preferenceId, preferencesEntity, arguments)
        {
            _preferencesEntity = preferencesEntity;
            _preferenceId = preferenceId;
        }

        public override float Value
        {
            get
            {
                CulturalPreference preference = _preferencesEntity.Culture.GetPreference(_preferenceId);

                if (preference == null)
                {
                    return 0;
                }

                return preference.Value;
            }
        }
    }

    public CulturalPreferencesEntity(string id) : base(id)
    {
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        if (!CulturalPreference.Preferences.Contains(attributeId))
        {
            throw new System.ArgumentException(
                "Unrecognized cultural preference in entity attribute: " + attributeId);
        }

        return new PreferenceAttribute(this, attributeId, arguments);
    }

    public void Set(Culture culture)
    {
        Culture = culture;
    }
}
