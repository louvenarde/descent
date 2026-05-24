using UnityEngine;

[CreateAssetMenu]
public class HumanoidParameters : ScriptableObject
{
    public float thighLength = 0.35f;

    public float ankleLength = 0.45f;

    public float hipWidth = 0.22f;


    [Range(0f, 1f)]
    public float minFlex01 = 0.5f;

    [Range(0f, 1f)]
    public float maxFlex01 = 0.95f;

    [Range(0f, 1f)]
    public float baseFlex01 = 0.74f;

    [Range(0f, 45f)]
    public float maxDegreesRotation = 5f;
}
