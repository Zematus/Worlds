using UnityEngine;

public class SkillAttribute : ValueEntityAttribute<float>
{
    private readonly ICulturalSkillsEntity _skillsEntity;
    private readonly string _skillId;

    public SkillAttribute(
        ICulturalSkillsEntity skillsEntity,
        string skillId)
        : base(skillId, skillsEntity, null)
    {
        _skillsEntity = skillsEntity;
        _skillId = skillId;
    }

    public override float Value => GetValue();

    private float GetValue()
    {
        CulturalSkill skill =
            _skillsEntity.Culture.GetSkill(_skillId);

        if (skill == null)
        {
            return 0;
        }

#if DEBUG
        if ((skill.Value <= 0) || (skill.Value >= 1))
        {
            Debug.LogWarning($"Skill value not between 0 and 1: {skill.Value}");
        }
#endif

        return skill.Value;
    }
}
