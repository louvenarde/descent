using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Linq;





#if UNITY_EDITOR
using UnityEditor;

#endif

public class TerrainPiece : MonoBehaviour
{
#if UNITY_EDITOR
    [System.Serializable]
    public struct Connector
    {
        public Vector3 exitPoint;
        public float exitAngle;
        public Vector3 exitDirection;
        public float exitWidth;
        public float exitValleyDepth;
        public List<Vector3> exitRow;
    }


    private Mesh mesh;

    public void Build(Nullable<Connector> entryConnector, TerrainPieceInputData inputData, out Connector exitConnector)
    {
        exitConnector = new Connector();
        exitConnector.exitRow = new List<Vector3>();

        float entryWidthMeters = inputData.exitWidthMeters;

        transform.position = Vector3.zero;

        if (entryConnector.HasValue)
        {
            entryWidthMeters = entryConnector.Value.exitWidth;
            transform.position = entryConnector.Value.exitPoint;
        }

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

        Mesh inMesh;
        {
            if (!mesh)
            {
                mesh = new Mesh();
            }

            inMesh = mesh;
            mesh.Clear();
        }

        List<Vector3Int> tris = new List<Vector3Int>();
        List<Vector3> points = new List<Vector3>();

        List<Vector3> lastRow = new List<Vector3>();

        int vxWidth = inputData.vxWidth;
        float valleyDepth = inputData.valleyDepth;
        int vxLength = inputData.vxLength;
        AnimationCurve valleyCurveNormalized = inputData.valleyCurveNormalized;
        float exitWidthMeters = inputData.exitWidthMeters;
        int connectorInfluencePercent = inputData.connectorInfluencePercent;
        float lengthMeters = inputData.lengthMeters;
        int steepnessPercent = inputData.steepnessPercent;

        float connectorInfluence01 = connectorInfluencePercent / (100f);

        for (int vxX = 0; vxX < vxWidth + 1; vxX++)
        {
            for (int vxZ = 0; vxZ < vxLength + 1; vxZ++)
            {
                float normalizedX = (vxX / (float)vxWidth);
                Assert.IsFalse(float.IsNaN(normalizedX), "normalizedX");

                float localConnectorInfluence01 = connectorInfluence01;
                if (vxZ == 0 && !inputData.detached)
                {
                    localConnectorInfluence01 = 1; // Always attach the first point
                }

                float normalizedZ = (vxZ / (float)vxLength);
                Assert.IsFalse(float.IsNaN(normalizedZ), "normalizedZ");

                float width = Mathf.Lerp(entryWidthMeters, exitWidthMeters, normalizedZ * localConnectorInfluence01 + 1f - localConnectorInfluence01);
                Assert.IsFalse(float.IsNaN(width), "width");

                float x = width * normalizedX - width * 0.5f; // Remove half width to make centered
                Assert.IsFalse(float.IsNaN(x), "x");

                float valleyDepthAtThisPoint = valleyDepth;
                if (entryConnector.HasValue)
                {
                    valleyDepthAtThisPoint = Mathf.Lerp(
                        entryConnector.Value.exitValleyDepth,
                        valleyDepth,
                        normalizedZ * localConnectorInfluence01 + 1f - localConnectorInfluence01
                    );

                    Assert.IsFalse(float.IsNaN(valleyDepthAtThisPoint), "valleyDepthAtThisPoint");
                }

                float heightFromValley = valleyDepthAtThisPoint * valleyCurveNormalized.Evaluate(normalizedX);
                Assert.IsFalse(float.IsNaN(heightFromValley), "heightFromValley");


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

                // Final influence override to ensure connection between valleys of different curves
                if (entryConnector.HasValue)
                {
                    float linkInfluence01 = normalizedZ * localConnectorInfluence01 + 1f - localConnectorInfluence01;

                    int exitRowIndex =
                        Mathf.Clamp(
                            (int)(entryConnector.Value.exitRow.Count * normalizedX),
                            0,
                            entryConnector.Value.exitRow.Count-1
                        );

                    Vector3 entryPoint = transform.InverseTransformPoint(entryConnector.Value.exitRow[exitRowIndex]);

                    y = Mathf.Lerp(entryPoint.y, y, linkInfluence01);
                    x = Mathf.Lerp(entryPoint.x, x, linkInfluence01);
                }

                Vector3 point = new Vector3(x, y, z);
                points.Add(point);

                if (vxZ == vxLength)
                {
                    lastRow.Add(new Vector3(x, heightFromSteepness, z));
                    exitConnector.exitRow.Add(transform.TransformPoint(point));
                }
            }
        }

        for (int a = 0; a <= vxWidth * vxLength + 1; a += vxLength + 1)
        {
            for (int b = 0; b < vxLength; b++)
            {
                int n = a + b;
                tris.Add(new Vector3Int(n, n + 1, n + vxLength + 1 + 1));
                tris.Add(new Vector3Int(n, n + vxLength + 1 + 1, n + vxLength + 1));
            }
        }

        List<int> integerTris = new List<int>(tris.Count * 3);
        for (int i = 0; i < tris.Count; i++)
        {
            integerTris.Add(tris[i].x);
            integerTris.Add(tris[i].y);
            integerTris.Add(tris[i].z);
        }

        points.AddRange(inMesh.vertices);
        inMesh.SetVertices(inVertices: points);

        integerTris.AddRange(inMesh.triangles);
        inMesh.SetTriangles(integerTris, 0);

        {
            Vector3 middlePoint = Vector3.zero;
            for (int i = 0; i < lastRow.Count; i++)
            {
                middlePoint += lastRow[i];
            }

            middlePoint /= lastRow.Count;

            exitConnector.exitPoint = transform.TransformPoint(middlePoint);
        }

        filter.mesh = inMesh;

        var perpendicular = Vector3.Cross(Vector3.up, lastRow[0] - lastRow[1]);
        exitConnector.exitAngle = 0f;
        exitConnector.exitDirection = perpendicular;
        exitConnector.exitWidth = exitWidthMeters;
        exitConnector.exitValleyDepth = valleyDepth;
    }

    void OnDestroy()
    {
        if (mesh)
        {
            Destroy(mesh);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (mesh)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Handles.matrix = transform.localToWorldMatrix;

            // Display
            Gizmos.color = Color.magenta;

            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                Vector3 a = mesh.vertices[mesh.triangles[i]];
                Vector3 b = mesh.vertices[mesh.triangles[i + 1]];
                Vector3 c = mesh.vertices[mesh.triangles[i + 2]];
                Gizmos.DrawLine(a, b);
                Gizmos.DrawLine(b, c);
                Gizmos.DrawLine(a, c);
            }

            Gizmos.color = Color.blue;
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                Gizmos.DrawSphere(mesh.vertices[i], 0.1f);
                Handles.Label(mesh.vertices[i] + Vector3.up * 0.5f, i.ToString());
            }

            Gizmos.matrix = Matrix4x4.identity;
            Handles.matrix = Matrix4x4.identity;
        }
    }
#endif
}
