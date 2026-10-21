using UnityEngine;

[CreateAssetMenu]
public class BoardProfile : ScriptableObject
{
    public float sympatheticCorrectionForce = 4f;

    public float maxEdgeFriction = 30f;

    [Range(1f, 10f)]
    public float minimumControlVelocityKmh = 4f;

    [Range(0f, 45f)]
    public float maxVisualTiltDegrees = 15f;

    [Range(0f, 1f)]
	public float grip = 0.05f;

	[Range(0f, 3f)]
	public float gripRecoveryTimeSeconds = 1.5f;
}
