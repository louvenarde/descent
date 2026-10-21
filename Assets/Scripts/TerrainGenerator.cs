using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BezierSpline))]
[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    public TerrainPiece piecePrefab;

    [NonSerialized]
    public BezierSpline spline;

    [SerializeField]
    public List<TerrainPieceInputData> terrainDefinition = new List<TerrainPieceInputData>();

    [SerializeField]
    private List<TerrainPiece> pieces = new List<TerrainPiece>();

    [NonSerialized]
    public HashSet<int> dirty = new HashSet<int>();

    [SerializeField]
    private bool debugShowPlane = false;
    [SerializeField]
    private bool debugShowSegmentNumber = false;

    private void Reset()
    {
        terrainDefinition = new List<TerrainPieceInputData> {
            new TerrainPieceInputData()
        };
        Undo.RecordObject(spline, "Reset Spline");
        spline.Reset();
        Rebuild();
    }

    private void OnEnable()
    {
        spline = GetComponent<BezierSpline>();
        Rebuild();
        Undo.undoRedoPerformed += Rebuild;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= Rebuild;
    }

    private void OnDestroy()
    {
        DestroyAllPieces();
    }

    [ContextMenu("Destroy all pieces")]
    private void DestroyAllPieces()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
        pieces.Clear();
    }

    private void Build(int i)
    {
        if (i < 0 || i > terrainDefinition.Count) return;

        if (i >= pieces.Count)
            pieces.AddRange(Enumerable.Repeat<TerrainPiece>(null, i - pieces.Count + 1));
        if (pieces[i] == null)
            pieces[i] = (TerrainPiece)Instantiate(piecePrefab, transform);
        pieces[i].gameObject.SetActive(true);

        pieces[i].Build(i == 0 ? null : pieces[i - 1], terrainDefinition[i], spline, i);
    }
    
    [ContextMenu("Rebuild")]
    public void Rebuild()
    {
        DestroyAllPieces();

        if (!piecePrefab) return;

        for (int i = 0; i < terrainDefinition.Count; i++)
            Build(i);

        dirty.Clear();
        spline.dirty.Clear();
    }


    public void InsertPieceAfter(int pos)
    {
        TerrainPieceInputData data = new TerrainPieceInputData();
        data.exitWidthMeters = terrainDefinition[pos].exitWidthMeters;
        data.valleyCurveNormalized = new AnimationCurve(terrainDefinition[pos].valleyCurveNormalized.keys);
        terrainDefinition.Insert(pos + 1, data);
        spline.InsertCurve(pos + 1);
        Rebuild();
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
        Rebuild();
    }

    // Executed in edit mode
    private void Update()
    {
        dirty.UnionWith(spline.dirty);
        foreach(int index in dirty)
            Build(index);
        dirty.Clear();
        spline.dirty.Clear();
    }

    void OnDrawGizmos()
    {
        for(int i = 0; i < pieces.Count; i++)
        {
            var piece = pieces[i];

            piece.DrawPieceGizmo(debugShowPlane);

            if (debugShowSegmentNumber)
            {
                Handles.Label((piece.transform.position + piece.GetExitCenter()) / 2,
                i.ToString(),
                new GUIStyle() { fontSize = 20, normal = new GUIStyleState() { textColor = Color.red }, alignment = TextAnchor.MiddleCenter });
            }
        }
    }

#endif
}
