using System;
using UnityEngine;

[Serializable]
public class BorderInputData
{
    [Range(2, 40)]
    public int nbQuadWidth = 5;

    [Range(0, 100)]
    public float width = 25;

    [Range(0, 20)]
    public float height = 1;

    public AnimationCurve curveNormalized = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
}

[Serializable]
public class TerrainPieceInputData
{
    [Range(2, 200)]
    public int nbQuadWidth = 5;

    [Range(2, 200)]
    public int nbQuadLength = 3;

    [Range(3, 50)]
    public float exitWidthMeters = 10;

    [Range(0, 100)]
    public int connectorInfluencePercent = 100;

    [Range(0, 4)]
    public float valleyDepth = 1;

    public bool detached = false;

    public AnimationCurve valleyCurveNormalized = AnimationCurve.EaseInOut(0f, 0f, 0f, 0f);

    public BorderInputData leftBorder;
    public BorderInputData rightBorder;
}
