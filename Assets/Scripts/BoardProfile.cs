using UnityEngine;

[CreateAssetMenu]
public class BoardProfile : ScriptableObject
{
    public float sympatheticCorrectionForce = 4f;

    public float maxEdgeFriction = 30f;

    public float minimumControlVelocity = 1f;

    [Range(0f, 45f)]
    public float maxVisualTiltDegrees = 15f;
}
