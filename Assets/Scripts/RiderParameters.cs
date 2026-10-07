using UnityEngine;

[CreateAssetMenu]
public class RiderParameters : ScriptableObject
{
    [Range(10, 300)]
    public int maxStableSpeedKph;

    public AnimationCurve frontFootWeightCurve = AnimationCurve.Linear(0f, 1f, 0f, 1f);

    public AnimationCurve backfootPositiveSpeedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1.5f);

    public AnimationCurve backfootNegativeSpeedCurve = AnimationCurve.Linear(0f, 0.5f, 1f, 1f);

    [SerializeField]
    private float impactEffectMultiplier = 1f;

    [SerializeField]
    private float frontStandaloneMovementPunishmentMultiplier = 1f;
}