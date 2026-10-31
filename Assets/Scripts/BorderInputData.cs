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

	public float treeDensity = 0f;

	public AnimationCurve curveNormalized = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
}
