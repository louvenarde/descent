using UnityEngine;

[CreateAssetMenu]
public class RiderParameters : ScriptableObject
{
    [Range(10, 300)]
    public int maxStableSpeedKph;

    public AnimationCurve frontFootWeightCurve = AnimationCurve.Linear(0f, 1f, 0f, 1f);

    public AnimationCurve backfootPositiveSpeedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1.5f);

    public AnimationCurve backfootNegativeSpeedCurve = AnimationCurve.Linear(0f, 0.5f, 1f, 1f);

    [Range(0.1f, 3f)]
    public float impactEffectMultiplier = 1f;

    [Range(0.0f, 3f)]
    public float frontStandaloneMovementPunishmentMultiplier = 1f;

    [Range(1f, 120f)]
    public float legResponsiveness = 20f;
}