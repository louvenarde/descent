using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BezierSpline))]
[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    public TerrainPiece pieceExample;

    [NonSerialized]
    public BezierSpline spline;

    [System.Serializable]
    public class DescentTerrain
    {
        [System.Serializable]
        public class TerrainConnection
        {
            public TerrainPiece piece;
            public List<TerrainPiece.Connector> outgoingConnectors;
        }

        public List<TerrainConnection> connections = new List<TerrainConnection>();
    }

    public List<TerrainPieceInputData> terrainDefinition = new List<TerrainPieceInputData>();

    [SerializeField]
    //[HideInInspector]
    private DescentTerrain terrain = new DescentTerrain();

    [SerializeField]
    private bool debugShowPlane = false;
    [SerializeField]
    private bool debugShowSegmentNumber = false;

    private void OnEnable()
    {
        spline = GetComponent<BezierSpline>();
        pieceExample.gameObject.SetActive(false);
        Build();
    }

    [ContextMenu("Clear")]
    private void Clear()
    {
        // FIXME:
        // Unfortunatly we cannot just use terrain.connections here to remove pieces because it will cause issue with Ctrl+Z
        // I didn't find any means to clear the previous pieces before the undo if the object is affected
        // Maybe with OnBeforeSerialize? but it will likely cause issue on terrainpiece serialization down the line? - Togi

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject childObject = transform.GetChild(i).gameObject;
            if (childObject == pieceExample.gameObject) continue;
            DestroyImmediate(childObject);
        }

        terrain.connections.Clear();
    }

    [ContextMenu("Build")]
    private void Build()
    {
        Clear();

        DescentTerrain.TerrainConnection previousConnector = null;
        for (int i = 0; i < terrainDefinition.Count; i++)
        {
            TerrainPiece pieceInst = (TerrainPiece)Instantiate(pieceExample, pieceExample.transform.parent);
            pieceInst.gameObject.SetActive(true);

            var connx = new DescentTerrain.TerrainConnection();
            connx.piece = pieceInst;

            List<TerrainPiece.Connector> connectors;
            connx.piece.Build(previousConnector, terrainDefinition[i], spline, i, out connectors);
            connx.outgoingConnectors = connectors;
            previousConnector = connx;

            terrain.connections.Add(connx);
        }
    }


    public void InsertPieceAfter(int pos)
    {
        TerrainPieceInputData data = new TerrainPieceInputData();
        data.exitWidthMeters = terrainDefinition[pos].exitWidthMeters;
        data.valleyCurveNormalized = new AnimationCurve(terrainDefinition[pos].valleyCurveNormalized.keys);
        terrainDefinition.Insert(pos + 1, data);
        spline.InsertCurve(pos + 1);
        Build();
    }

    /// <summary>
    /// When we remove a piece we need to know which part of the piece should be destroyed<br />
    /// This function remove the piece at index <c>pos</c> and the endpoint of this piece<br />
    /// For example this curve : |   0   |   1   |   2   |<br />
    /// Become after Remove(1) : |   0   |       X   1   |
    /// </summary>
    /// <param name="pos">Piece index</param>
    public void RemoveEndpointFromPiece(int pos)
    {
        terrainDefinition.RemoveAt(pos);
        spline.RemoveCurve(pos + 1);
        Build();
    }

    // Executed in edit mode
    private void Update()
    {
        if (Selection.Contains(gameObject.GetInstanceID()))
        {
            Build();
        }
    }

    private void OnDestroy()
    {
        Clear();
    }

    void OnDrawGizmos()
    {
        for(int i = 0; i < terrain.connections.Count; i++)
        {
            var connx = terrain.connections[i];

            Handles.color = Color.Lerp(Color.yellow, Color.red, 0.5f);
            Handles.ArrowCap(0, connx.outgoingConnectors[0].exitPoint, Quaternion.LookRotation(connx.outgoingConnectors[0].exitDirection, Vector3.up), 1f);

            Vector3 exitCenter = connx.outgoingConnectors[0].exitPoint;
            if(debugShowPlane)
            {
                Vector3 normal = connx.outgoingConnectors[0].exitDirection;
                Vector3 right = Vector3.Cross(Vector3.up, normal).normalized;
                Vector3 up = Vector3.Cross(right, normal).normalized;
            
                Vector3[] verts = new Vector3[]
                {
                    exitCenter - right * connx.outgoingConnectors[0].exitWidth - up * connx.outgoingConnectors[0].exitValleyDepth,
                    exitCenter + right * connx.outgoingConnectors[0].exitWidth - up * connx.outgoingConnectors[0].exitValleyDepth,
                    exitCenter + right * connx.outgoingConnectors[0].exitWidth + up * connx.outgoingConnectors[0].exitValleyDepth,
                    exitCenter - right * connx.outgoingConnectors[0].exitWidth + up * connx.outgoingConnectors[0].exitValleyDepth
                };

                Color c = new Color(1, 0, 1, 0.1f);
                Handles.DrawSolidRectangleWithOutline(verts, c, Color.white); // no zTest for handles in this version of Unity... need to find an alternative
            }

            if (debugShowSegmentNumber)
            {
                Handles.Label((connx.piece.transform.position + exitCenter) / 2,
                i.ToString(),
                new GUIStyle() { fontSize = 20, normal = new GUIStyleState() { textColor = Color.red }, alignment = TextAnchor.MiddleCenter });
            }
        }
    }

#endif
}
