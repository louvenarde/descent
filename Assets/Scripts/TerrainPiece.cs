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
        public List<Vector2> exitRow;
    }

    private Mesh mesh;

    public void Build(Nullable<Connector> entryConnector, TerrainPieceInputData inputData, BezierSpline spline, int index, out Connector exitConnector)
    {
        exitConnector = new Connector();
        exitConnector.exitRow = new List<Vector2>();

        transform.position = spline.GetPoint(index / (float)spline.CurveCount);

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

        MeshCollider collider = gameObject.GetComponent<MeshCollider>();
        if (!collider)
        {
            collider = gameObject.AddComponent<MeshCollider>();
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

        List<Vector3> points = new List<Vector3>();
        List<Vector2> uv0 = new List<Vector2>();
        List<Color32> colors = new List<Color32>();

        List<Vector3> lastRow = new List<Vector3>();

        int nbQuadWidth = inputData.nbQuadWidth;
        int nbQuadLength = inputData.nbQuadLength;
        float valleyDepth = inputData.valleyDepth;
        AnimationCurve valleyCurveNormalized = inputData.valleyCurveNormalized;
        float exitWidthMeters = inputData.exitWidthMeters;
        int connectorInfluencePercent = inputData.connectorInfluencePercent;

        float connectorInfluence01 = connectorInfluencePercent / (100f);

        int nbVertWidth = nbQuadWidth + 1;
        int nbVertLength = nbQuadLength + 1;
        for (int vxX = 0; vxX < nbVertWidth; vxX++)
        {
            for (int vxZ = 0; vxZ < nbVertLength; vxZ++)
            {
                float normalizedX = (vxX / (float)nbQuadWidth);
                Assert.IsFalse(float.IsNaN(normalizedX), "normalizedX");
                float normalizedZ = (vxZ / (float)nbQuadLength);
                Assert.IsFalse(float.IsNaN(normalizedZ), "normalizedZ");

                float localConnectorInfluence01 = connectorInfluence01;
                if (vxZ == 0 && !inputData.detached)
                {
                    localConnectorInfluence01 = 1; // Always attach the first point
                }

                Vector2 localPos = new Vector2(
                    exitWidthMeters * normalizedX - exitWidthMeters * 0.5f, // Remove half width to make centered
                    valleyDepth * valleyCurveNormalized.Evaluate(normalizedX)
                );
                // We do not interpolate for connector influence in localPos because the width and depth are baked in the exitRow
                // and doing it twice lead to exponential behavior which feel unnatural 

                // Influence override to ensure connection between valleys of different curves
                if (entryConnector.HasValue)
                {
                    float linkInfluence01 = normalizedZ * localConnectorInfluence01 + 1f - localConnectorInfluence01;

                    int exitRowIndex =
                        Mathf.Clamp(
                            (int)(entryConnector.Value.exitRow.Count * normalizedX),
                            0,
                            entryConnector.Value.exitRow.Count - 1
                        );

                    Vector3 entryPoint = entryConnector.Value.exitRow[exitRowIndex];

                    localPos.x = Mathf.Lerp(entryPoint.x, localPos.x, linkInfluence01);
                    localPos.y = Mathf.Lerp(entryPoint.y, localPos.y, linkInfluence01);
                }

                float splineLocation = (index * nbQuadLength + vxZ) / (float)(spline.CurveCount * nbQuadLength);
                Vector3 splinePos = spline.GetPoint(splineLocation);
                Vector3 splineRight = spline.GetRight(splineLocation, transform.up);
                Vector3 splineUp = spline.GetUp(splineLocation, transform.up);

                Vector3 globalPos = splinePos + splineRight * localPos.x + splineUp * localPos.y;

                points.Add(transform.InverseTransformPoint(globalPos));
                uv0.Add(new Vector2(normalizedX, normalizedZ));
                colors.Add(new Color32(0xFF, 0xFF, 0xFF, 0xFF));

                if (vxZ == nbQuadLength)
                {
                    lastRow.Add(transform.InverseTransformPoint(globalPos));
                    exitConnector.exitRow.Add(localPos); // we keep them in localSpace
                }
            }
        }
        inMesh.SetVertices(inVertices: points);
        inMesh.SetUVs(0, uv0);
        inMesh.SetColors(colors);

        List<int> integerTris = new List<int>(nbQuadWidth * nbQuadLength * 3);
        for (int x = 0; x < nbQuadWidth; x++)
        {
            for (int y = 0; y < nbQuadLength; y++)
            {
                int n = x * nbVertLength + y;
                integerTris.Add(n);
                integerTris.Add(n + 1);
                integerTris.Add(n + 1 + nbVertLength);

                integerTris.Add(n);
                integerTris.Add(n + 1 + nbVertLength);
                integerTris.Add(n + nbVertLength);
            }
        }
        inMesh.SetTriangles(integerTris, 0);

        inMesh.RecalculateNormals();
        inMesh.RecalculateBounds();

        {
            Vector3 middlePoint = Vector3.zero;
            Vector3 perpendicular = Vector3.zero;
            for (int i = 0; i < lastRow.Count; i++)
            {
                middlePoint += lastRow[i];

                if (i < lastRow.Count - 1)
                    perpendicular += Vector3.Cross(Vector3.up, lastRow[i] - lastRow[i + 1]);
            }
            middlePoint /= lastRow.Count;
            perpendicular /= lastRow.Count - 1;

            exitConnector.exitPoint = transform.TransformPoint(middlePoint);
            exitConnector.exitDirection = perpendicular;
        }

        filter.sharedMesh = inMesh;
        collider.sharedMesh = inMesh;

        exitConnector.exitAngle = 0f;
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

    /*private void OnDrawGizmosSelected()
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
    }*/
#endif
}
