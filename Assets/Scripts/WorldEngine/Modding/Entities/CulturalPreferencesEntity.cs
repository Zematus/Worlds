using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CulturalPreferencesEntity : Entity
{
    public Culture Culture;

    protected override object _reference => Culture;

    public class PreferenceAttribute : AssignableValueEntityAttribute<float>
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
                CulturalPreference preference =
                    _preferencesEntity.Culture.GetPreference(_preferenceId);

                if (preference == null)
                {
                    return 0;
                }

                return preference.Value;
            }
            set
            {
                if (!value.IsInsideRange(0,1))
                {
                    throw new System.ArgumentException(
                        "Cultural preference can only be assigned values in the range [0,1]");
                }

                CulturalPreference preference =
                    _preferencesEntity.Culture.GetPreference(_preferenceId);

                if (preference == null)
                {
                    throw new System.NotImplementedException();
                }

                // TODO: Make sure this value is set only during or after UpdatePreferences
                preference.Value = value;
            }
        }
    }

    public CulturalPreferencesEntity(Context c, string id) : base(c, id)
    {
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        if (!PreferenceGenerator.Generators.ContainsKey(attributeId))
        {
            throw new System.ArgumentException(
                "Unrecognized cultural preference in entity attribute: " + attributeId);
        }

        return new PreferenceAttribute(this, attributeId, arguments);
    }

    public override string GetDebugString()
    {
        return "cultural_preferences";
    }

    public override string GetFormattedString()
    {
        return "<i>cultural preferences</i>";
    }

    public void Set(Culture c) => Culture = c;

    public override void Set(object o)
    {
        if (o is CulturalPreferencesEntity e)
        {
            Set(e.Culture);
        }
        else if (o is Culture c)
        {
            Set(c);
        }
        else
        {
            throw new System.ArgumentException("Unexpected type: " + o.GetType());
        }
    }
}
