using System;
using UnityEngine;

[Serializable]
public class TerrainPieceInputData
{
    [Range(2, 5)]
    public int vxWidth = 5;

    [Range(2, 5)]
    public int vxLength = 3;

    [Range(3, 50)]
    public float exitWidthMeters = 10;

    [Range(5, 50)]
    public float lengthMeters = 10;

    [Range(0, 100)]
    public int connectorInfluencePercent = 100;

    [Range(0, 100)]
    public int steepnessPercent = 10;

    [Range(0, 4)]
    public float valleyDepth = 1;

    public bool detached = false;

    public AnimationCurve valleyCurveNormalized = AnimationCurve.EaseInOut(0f, 0f, 0f, 0f);
}
