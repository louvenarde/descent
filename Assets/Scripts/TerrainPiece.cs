using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Reflection;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class TerrainPiece : MonoBehaviour
{
    [System.Serializable]
    public struct Connector
    {
        public Vector3 exitPoint;
        public float exitAngle;
        public Vector3 exitDirection;
        public float exitWidth;
        public float exitValleyDepth;
        public List<Vector2> exitRow;

        public List<Vector2> leftSide;
        public List<Vector2> leftSideExitRow;
        public List<Vector2> rightSide;
        public List<Vector2> rightSideExitRow;
    }

    [SerializeField]
    List<Mesh> lodMeshes;

    [SerializeField]
    LODGroup mainPieceLODGroup;

    [SerializeField]
    MeshFilter meshBarrierLeft;
    [SerializeField]
    LODGroup leftBorderLODGroup;
    [SerializeField]
    MeshFilter meshBarrierRight;
    [SerializeField]
    LODGroup rightBorderLODGroup;

    [SerializeField]
    List<Connector> outgoingConnectors;

#if UNITY_EDITOR
    public void Build(TerrainPiece previousPiece, TerrainPieceInputData inputData, BezierSpline spline, int index)
    {
        outgoingConnectors = new List<Connector>();

        var lods = mainPieceLODGroup.GetLODs();
        for (int lod = 0; lod < lods.Length; lod++)
        {
            if (lodMeshes.Count <= lod)
                lodMeshes.Add(new Mesh());
            CreateMesh(lodMeshes[lod], inputData, spline, previousPiece, index, lod);
            lods[lod].renderers[0].GetComponent<MeshFilter>().sharedMesh = lodMeshes[lod];
        }
        mainPieceLODGroup.RecalculateBounds();

        MeshCollider collider = gameObject.GetComponent<MeshCollider>();
        if (!collider)
            collider = gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = lodMeshes[lodMeshes.Count - 1];


        var lBorderLods = leftBorderLODGroup.GetLODs();
        for (int lod = 0; lod < lBorderLods.Length; lod++)
        {
            Mesh borderTest = new Mesh();
            CreateBorder(borderTest, inputData.leftBorder, spline, previousPiece, index, lod, true);
            lBorderLods[lod].renderers[0].GetComponent<MeshFilter>().sharedMesh = borderTest;
        }
        leftBorderLODGroup.RecalculateBounds();

        Mesh barrierLeft = new Mesh();
        CreateBarrier(barrierLeft, spline, index, true);
        meshBarrierLeft.sharedMesh = barrierLeft;
        meshBarrierLeft.GetComponent<MeshCollider>().sharedMesh = barrierLeft;


        var rBorderLods = rightBorderLODGroup.GetLODs();
        for (int lod = 0; lod < rBorderLods.Length; lod++)
        {
            Mesh borderTest = new Mesh();
            CreateBorder(borderTest, inputData.rightBorder, spline, previousPiece, index, lod, false);
            rBorderLods[lod].renderers[0].GetComponent<MeshFilter>().sharedMesh = borderTest;
        }
        rightBorderLODGroup.RecalculateBounds();

        Mesh barrierRight = new Mesh();
        CreateBarrier(barrierRight, spline, index, false);
        meshBarrierRight.sharedMesh = barrierRight;
        meshBarrierRight.GetComponent<MeshCollider>().sharedMesh = barrierRight;
    }

    private void CreateMesh(Mesh inMesh, TerrainPieceInputData inputData, BezierSpline spline, TerrainPiece prevPiece, int index, int lod)
    {
        inMesh.Clear();

        Connector exitConnector = new Connector();
        exitConnector.exitRow = new List<Vector2>();
        exitConnector.leftSide = new List<Vector2>();
        exitConnector.leftSideExitRow = new List<Vector2>();
        exitConnector.rightSide = new List<Vector2>();
        exitConnector.rightSideExitRow = new List<Vector2>();

        transform.position = spline.GetPoint(index / (float)spline.CurveCount);

        List<Vector3> points = new List<Vector3>();
        List<Vector2> uv0 = new List<Vector2>();

        List<Vector3> lastRow = new List<Vector3>();

        float subdivide = Mathf.Pow(2, lod);
        int nbQuadWidth = Math.Max(1, (int)(inputData.nbQuadWidth / subdivide));
        int nbQuadLength = Math.Max(1, (int)(inputData.nbQuadLength / subdivide));
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
                if (prevPiece)
                {
                    float linkInfluence01 = normalizedZ * localConnectorInfluence01 + 1f - localConnectorInfluence01;

                    int exitRowIndex =
                        Mathf.Clamp(
                            (int)(prevPiece.outgoingConnectors[lod].exitRow.Count * normalizedX),
                            0,
                            prevPiece.outgoingConnectors[lod].exitRow.Count - 1
                        );

                    Vector3 entryPoint = prevPiece.outgoingConnectors[lod].exitRow[exitRowIndex];

                    localPos.x = Mathf.Lerp(entryPoint.x, localPos.x, linkInfluence01);
                    localPos.y = Mathf.Lerp(entryPoint.y, localPos.y, linkInfluence01);
                }

                int vxZSpline = vxZ;
                // We extend a bit the start of the LOD1 mesh to create a little overlap, help filling the gaps for the outline shader during the transition between LOD0 & LOD1
                // No need for LOD2+ because they are far away enough to not be noticable and it can cause issue with the collision mesh (based on LOD2)
                if (vxZ == 0 && subdivide == 2)
                    vxZSpline--;
                float splineLocation = (index * nbQuadLength + vxZSpline) / (float)(spline.CurveCount * nbQuadLength);
                Vector3 globalPos = ApplySpline(spline, splineLocation, localPos);

                points.Add(transform.InverseTransformPoint(globalPos));
                uv0.Add(new Vector2(normalizedX, normalizedZ));

                if (vxZ == nbQuadLength)
                {
                    lastRow.Add(transform.InverseTransformPoint(globalPos));
                    exitConnector.exitRow.Add(localPos); // we keep them in localSpace
                }

                if (vxX == 0)
                    exitConnector.leftSide.Add(localPos);
                if (vxX == nbVertWidth - 1)
                    exitConnector.rightSide.Add(localPos);
            }
        }
        GenerateMesh(inMesh, points, uv0, nbQuadWidth, nbQuadLength, false);

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

        exitConnector.exitAngle = 0f;
        exitConnector.exitWidth = exitWidthMeters;
        exitConnector.exitValleyDepth = valleyDepth;

        if (outgoingConnectors.Count <= lod)
            outgoingConnectors.AddRange(Enumerable.Repeat<Connector>(new Connector(), lod - outgoingConnectors.Count + 1));
        outgoingConnectors[lod] = exitConnector;
    }

    private void CreateBorder(Mesh inMesh, BorderInputData inputData, BezierSpline spline, TerrainPiece prevPiece, int index, int lod, bool leftSide)
    {
        inMesh.Clear();

        List<Vector2> sidePos = leftSide ? outgoingConnectors[lod].leftSide : outgoingConnectors[lod].rightSide;
        List<Vector2> prevExitRow = null;
        if (prevPiece)
            prevExitRow = leftSide ? prevPiece.outgoingConnectors[lod].leftSideExitRow : prevPiece.outgoingConnectors[lod].rightSideExitRow;
        List<Vector3> points = new List<Vector3>();
        List<Vector2> uv0 = new List<Vector2>();

        int nbQuadWidth = Math.Max(1, (int)(inputData.nbQuadWidth));
        int nbVertLength = sidePos.Count;
        int nbQuadLength = nbVertLength - 1;
        float borderHeight = inputData.height;
        float borderWidth = inputData.width * (leftSide ? -1 : 1);
        AnimationCurve curveNormalized = inputData.curveNormalized;

        int nbVertWidth = nbQuadWidth + 1;
        for (int vxX = 0; vxX < nbVertWidth; vxX++)
        {
            for (int vxZ = 0; vxZ < nbVertLength; vxZ++)
            {
                float normalizedX = (vxX / (float)nbQuadWidth);
                Assert.IsFalse(float.IsNaN(normalizedX), "normalizedX");
                float normalizedZ = (vxZ / (float)nbQuadLength);
                Assert.IsFalse(float.IsNaN(normalizedZ), "normalizedZ");

                Vector2 localPos = new Vector2(
                    borderWidth * normalizedX,
                    curveNormalized.Evaluate(normalizedX) * borderHeight
                );

                if (prevPiece)
                {
                    int exitRowIndex =
                        Mathf.Clamp(
                            (int)(prevExitRow.Count * normalizedX),
                            0,
                            prevExitRow.Count - 1
                        );

                    localPos.x = Mathf.Lerp(prevExitRow[exitRowIndex].x, localPos.x, normalizedZ);
                    localPos.y = Mathf.Lerp(prevExitRow[exitRowIndex].y, localPos.y, normalizedZ);
                }

                int vxZSpline = vxZ;
                float splineLocation = (index * nbQuadLength + vxZSpline) / (float)(spline.CurveCount * nbQuadLength);
                // sidePos is added there to avoid any interpolation with prevPiece
                Vector3 globalPos = ApplySpline(spline, splineLocation, sidePos[vxZ] + localPos);

                points.Add(transform.InverseTransformPoint(globalPos));
                uv0.Add(new Vector2(normalizedX, normalizedZ));

                if (vxZ == nbQuadLength)
                {
                    // we keep them in localSpace
                    if (leftSide)
                        outgoingConnectors[lod].leftSideExitRow.Add(localPos);
                    else
                        outgoingConnectors[lod].rightSideExitRow.Add(localPos);
                }
            }
        }
        GenerateMesh(inMesh, points, uv0, nbQuadWidth, nbQuadLength, leftSide);
    }

    private Vector3 ApplySpline(BezierSpline spline, float splineLocation, Vector2 localPos)
    {
        Vector3 splinePos = spline.GetPoint(splineLocation);
        Vector3 splineRight = spline.GetRight(splineLocation, transform.up);
        Vector3 splineUp = spline.GetUp(splineLocation, transform.up);

        return splinePos + splineRight * localPos.x + splineUp * localPos.y;
    }

    private void CreateBarrier(Mesh inMesh, BezierSpline spline, int index, bool leftSide)
    {
        inMesh.Clear();

        List<Vector2> sidePos = leftSide ? outgoingConnectors.Last().leftSide : outgoingConnectors.Last().rightSide;
        List<Vector3> points = new List<Vector3>();
        List<Vector2> uv0 = new List<Vector2>();

        int nbVertLength = sidePos.Count;
        int nbQuadLength = nbVertLength - 1;

        for (int vxX = 0; vxX < 2; vxX++)
        {
            for (int vxZ = 0; vxZ < nbVertLength; vxZ++)
            {
                float normalizedZ = (vxZ / (float)nbQuadLength);
                Assert.IsFalse(float.IsNaN(normalizedZ), "normalizedZ");

                Vector2 localPos = sidePos[vxZ] + new Vector2(0, vxX * 2.0f);

                int vxZSpline = vxZ;
                float splineLocation = (index * nbQuadLength + vxZSpline) / (float)(spline.CurveCount * nbQuadLength);
                Vector3 globalPos = ApplySpline(spline, splineLocation, localPos);

                points.Add(transform.InverseTransformPoint(globalPos));
                uv0.Add(new Vector2(vxX, normalizedZ * 10.0f));
            }
        }
        GenerateMesh(inMesh, points, uv0, 1, nbQuadLength, leftSide);
    }
    
    private void GenerateMesh(Mesh inMesh, List<Vector3> points, List<Vector2> uv0, int nbQuadWidth, int nbQuadLength, bool flip)
    {
        inMesh.SetVertices(inVertices: points);
        inMesh.SetUVs(0, uv0);

        int nbVertLength = nbQuadLength + 1;
        List<int> integerTris = new List<int>(nbQuadWidth * nbQuadLength * 3);
        for (int x = 0; x < nbQuadWidth; x++)
        {
            for (int y = 0; y < nbQuadLength; y++)
            {
                int n = x * nbVertLength + y;

                if ((x + y) % 2 == 0)
                {
                    integerTris.Add(n);
                    integerTris.Add(n + 1 + (flip ? nbVertLength : 0));
                    integerTris.Add(n + 1 + (flip ? 0 : nbVertLength));

                    integerTris.Add(n);
                    integerTris.Add(n + nbVertLength + (flip ? 0 : 1));
                    integerTris.Add(n + nbVertLength + (flip ? 1 : 0));
                }
                else
                {
                    integerTris.Add(n);
                    integerTris.Add(n + (flip ? nbVertLength : 1));
                    integerTris.Add(n + (flip ? 1 : nbVertLength));

                    integerTris.Add(n + 1);
                    integerTris.Add(n + nbVertLength + (flip ? 0 : 1));
                    integerTris.Add(n + nbVertLength + (flip ? 1 : 0));
                }
            }
        }
        inMesh.SetTriangles(integerTris, 0);

        inMesh.RecalculateNormals();
        inMesh.RecalculateBounds();
    }

    void OnDestroy()
    {
        // I don't think it is necessary, the resource tracking should cover this but I don't know how well it works in editor so just in case:
        for (int i = 0; i < lodMeshes.Count; i++)
            Destroy(lodMeshes[i]);
        lodMeshes.Clear();
    }

    public Vector3 GetExitCenter()
    {
        return outgoingConnectors[0].exitPoint;
    }

    public void DrawPieceGizmo(bool showPlane)
    {
        Handles.color = Color.Lerp(Color.yellow, Color.red, 0.5f);
        Handles.ArrowCap(0, outgoingConnectors[0].exitPoint, Quaternion.LookRotation(outgoingConnectors[0].exitDirection, Vector3.up), 1f);

        Vector3 exitCenter = outgoingConnectors[0].exitPoint;
        if (showPlane)
        {
            Vector3 normal = outgoingConnectors[0].exitDirection;
            Vector3 right = Vector3.Cross(Vector3.up, normal).normalized;
            Vector3 up = Vector3.Cross(right, normal).normalized;

            Vector3[] verts = new Vector3[]
            {
                    exitCenter - right * outgoingConnectors[0].exitWidth - up * outgoingConnectors[0].exitValleyDepth,
                    exitCenter + right * outgoingConnectors[0].exitWidth - up * outgoingConnectors[0].exitValleyDepth,
                    exitCenter + right * outgoingConnectors[0].exitWidth + up * outgoingConnectors[0].exitValleyDepth,
                    exitCenter - right * outgoingConnectors[0].exitWidth + up * outgoingConnectors[0].exitValleyDepth
            };

            Color c = new Color(1, 0, 1, 0.1f);
            Handles.DrawSolidRectangleWithOutline(verts, c, Color.white); // no zTest for handles in this version of Unity... need to find an alternative
        }
    }

    /*private void OnDrawGizmosSelected()
    {
        if (lodMeshes.Count > 0)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Handles.matrix = transform.localToWorldMatrix;

            // Display
            Gizmos.color = Color.magenta;

            Mesh mesh = lodMeshes[lodMeshes.Count - 1];
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
