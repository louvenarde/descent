using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;



#if UNITY_EDITOR
using UnityEditor;
#endif


public class TerrainGenerator : MonoBehaviour
{

#if UNITY_EDITOR
    private struct Vector3Int
    {
        public int x, y, z;

        public Vector3Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
        {
            return string.Format("{0}; {1}; {2}", x, y, z);
        }
    };

    //private struct Quad
    //{
    //    public Vector3 northWest;
    //    public Vector3 northEast;
    //    public Vector3 southEast;
    //    public Vector3 southWest;
    //}

    [Range(2, 5)]
    public int vxWidth = 5;

    [Range(2, 5)]
    public int vxLength = 3;

    [Range(3, 10)]
    public float widthMeters = 10;

    [Range(5, 50)]
    public float lengthMeters = 10;

    [Range(0, 100)]
    public float steepnessPercent = 10;

    [Range(0, 4)]
    public float valleyDepth = 1;

    public AnimationCurve valleyCurveNormalized;

    private Mesh mesh;

    void OnDrawGizmos()
    {
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        if (!filter)
        {
            filter = gameObject.AddComponent<MeshFilter>();
        }

        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        if (!renderer)
        {
            gameObject.AddComponent<MeshRenderer>();
        }

        if (!mesh)
        {
            mesh = new Mesh();
        }

        List<Vector3Int> tris = new List<Vector3Int>();
        List<Vector3> points = new List<Vector3>();
        for (int vxX = 0; vxX < vxWidth + 1; vxX++)
        {
            float normalizedX = (vxX / (float)vxWidth);
            Assert.IsFalse(float.IsNaN(normalizedX), "normalizedX");

            float x = widthMeters * normalizedX;
            Assert.IsFalse(float.IsNaN(x), "x");

            float heightFromValley = valleyDepth * valleyCurveNormalized.Evaluate(normalizedX);
            Assert.IsFalse(float.IsNaN(heightFromValley), "heightFromValley");

            for (int vxZ = 0; vxZ < vxLength + 1; vxZ++)
            {
                float normalizedZ = (vxZ / (float)vxLength);
                Assert.IsFalse(float.IsNaN(normalizedZ), "normalizedZ");


                float zSpacingWanted = lengthMeters * normalizedZ;
                Assert.IsFalse(float.IsNaN(zSpacingWanted), "zSpacingWanted");


                float heightFromSteepness = -lengthMeters * normalizedZ * (steepnessPercent / 100f);
                Assert.IsFalse(float.IsNaN(heightFromSteepness), "heightFromSteepness");

                float y = heightFromSteepness + heightFromValley;
                Assert.IsFalse(float.IsNaN(y), "y");

                // Math approved by Togi of TogiMaro
                float z;
                {
                    z = Mathf.Sqrt(Mathf.Max(0f, zSpacingWanted * zSpacingWanted - y * y));
                    Assert.IsFalse(float.IsNaN(z), "z");
                }

                //Debug.Log(string.Format("{0}: {1}\t{2}\t{3}", points.Count, x, y, z));

                Vector3 point = new Vector3(x, y, z);
                points.Add(point);
            }
        }

        for(int a = 0; a <= vxWidth*vxLength+1; a += vxLength+1)
        {
            for (int b = 0; b < vxLength; b++)
            {
                int n = a + b;
                tris.Add(new Vector3Int(n, n + 1, n + vxLength + 1 + 1));
                tris.Add(new Vector3Int(n, n + vxLength + 1 + 1, n + vxLength + 1));
            }
        }

        // Display
        Gizmos.color = Color.magenta;
        for (int i = 0; i < tris.Count; i++)
        {
            Vector3 a = points[tris[i].x];
            Vector3 b = points[tris[i].y];
            Vector3 c = points[tris[i].z];
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(a, c);
        }

        Gizmos.color = Color.blue;
        for (int i = 0; i < points.Count; i++)
        {
            Gizmos.DrawSphere(points[i], 0.1f);
            Handles.Label(points[i] + Vector3.up * 0.5f, i.ToString());
        }

        filter.mesh = mesh;
    }
#endif
}
