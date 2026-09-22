using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;



#if UNITY_EDITOR
using UnityEditor;

#endif


[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    public TerrainPiece pieceExample;

    [System.Serializable]
    private class DescentTerrain
    {
        [System.Serializable]
        public class TerrainConnection
        {
            public TerrainPiece piece;
            public TerrainPiece.Connector outgoingConnector;
        }

        public List<TerrainConnection> connections = new List<TerrainConnection>();
    }

    public List<TerrainPieceInputData> terrainDefinition = new List<TerrainPieceInputData>();

    [SerializeField]
    //[HideInInspector]
    private DescentTerrain terrain = new DescentTerrain();

    void Awake()
    {
        Clear();
        pieceExample.gameObject.SetActive(false);
    }

    [ContextMenu("Clear")]
    private void Clear()
    {

        foreach (var connx in terrain.connections)
        {
            if (connx.piece)
            {
                DestroyImmediate(connx.piece.gameObject);
            }
        }

        terrain.connections.Clear();
    }

    [ContextMenu("Build")]
    private void Build()
    {
        Clear();

        TerrainPiece.Connector? previousConnector = null;
        for (int i = 0; i < terrainDefinition.Count; i++)
        {
            TerrainPiece pieceInst = (TerrainPiece)Instantiate(pieceExample, pieceExample.transform.parent);
            pieceInst.gameObject.SetActive(true);

            var connx = new DescentTerrain.TerrainConnection();
            connx.piece = pieceInst;

            TerrainPiece.Connector connector;
            connx.piece.Build(previousConnector, terrainDefinition[i], out connector);
            connx.outgoingConnector = connector;
            previousConnector = connector;

            terrain.connections.Add(connx);
        }
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
            Handles.ArrowCap(0, connx.outgoingConnector.exitPoint, Quaternion.LookRotation(connx.outgoingConnector.exitDirection, Vector3.up), 1f);
        }

    }

#endif
}
