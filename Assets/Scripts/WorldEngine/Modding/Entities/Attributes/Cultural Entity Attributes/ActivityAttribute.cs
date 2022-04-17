using UnityEngine;

public class ActivityAttribute : ValueEntityAttribute<float>
{
    private readonly ICulturalActivitiesEntity _activitiesEntity;
    private readonly string _activityId;

    public ActivityAttribute(
        ICulturalActivitiesEntity activitiesEntity,
        string activityId)
        : base(activityId, activitiesEntity, null)
    {
        _activitiesEntity = activitiesEntity;
        _activityId = activityId;
    }

    public override float Value => GetValue();

    private float GetValue()
    {
        CulturalActivity activity =
            _activitiesEntity.Culture.GetActivity(_activityId);

        if (activity == null)
        {
            return 0;
        }

#if DEBUG
        if ((activity.Value <= 0) || (activity.Value >= 1))
        {
            Debug.LogWarning($"Activity value not between 0 and 1: {activity.Value}");
        }
#endif

        return activity.Value;
    }
}
