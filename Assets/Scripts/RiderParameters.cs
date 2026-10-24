using UnityEngine;

[CreateAssetMenu]
public class RiderParameters : ScriptableObject
{
	[System.Serializable]
	public class Punishment
	{
		public float backCrossPunishmentEffect = 300f;
		public float backFacingVelocityPunishmentEffect = 300f;
		public float impactPunishmentEffect = 1000f;
		public bool backwardAndStoppedPunish = true;
	}

    [Range(10, 300)]
    public int maxStableSpeedKph;


    [Range(1f, 120f)]
    public float legResponsiveness = 20f;

    [Range(80, 300f)]
	public float torqueEffect = 100f;

	[Range(1f, 30f)]
	public float crossTorqueMultipllier = 5f;

    public AnimationCurve torqueControlFromVelocityAmount = AnimationCurve.Linear(0f, 1f, 1f, 1f);

    [Range(0.1f, 1f)]
    public float tiltEffectMultiplier = 0.5f;

    [Range(0.1f, 1f)]
    public float weightSpeedBoostKmh = 30f;

	public Punishment punishment = new Punishment();
}