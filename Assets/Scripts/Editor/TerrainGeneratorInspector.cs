using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        TerrainGenerator terrainGenerator = target as TerrainGenerator;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("piecePrefab"));

        EditorGUILayout.LabelField("Terrain:");
        EditorGUI.indentLevel++;
        int newCount = EditorGUILayout.DelayedIntField("Size", terrainGenerator.terrainDefinition.Count);
        int diff = newCount - terrainGenerator.terrainDefinition.Count;
        if(diff != 0)
        {
            Undo.RecordObjects(new Object[] { target, terrainGenerator.spline }, "Change terrain piece size");
            for (int i = 0; i < System.Math.Abs(diff); i++)
            {
                if (diff > 0)
                    terrainGenerator.InsertPieceAfter(terrainGenerator.terrainDefinition.Count - 1);
                else
                    terrainGenerator.RemoveEndpointFromPiece(terrainGenerator.terrainDefinition.Count - 1);
            }
            serializedObject.Update(); // serializedObject is no longer in sync, we reupdate it
        }

        var terrainProp = serializedObject.FindProperty("terrainDefinition");
        for (int i = 0; i < terrainGenerator.terrainDefinition.Count; i++) {
            SerializedProperty elementProp = terrainProp.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginHorizontal();
            elementProp.isExpanded = EditorGUILayout.Foldout(elementProp.isExpanded, "Terrain Piece " + i);
            if (GUILayout.Button("Insert after", EditorStyles.miniButtonLeft, GUILayout.Width(70f)))
            {
                Undo.RecordObjects(new Object[] { target, terrainGenerator.spline }, "Insert terrain piece");
                terrainGenerator.InsertPieceAfter(i);
                break;
            }
            
            if (terrainGenerator.terrainDefinition.Count > 1 && 
                GUILayout.Button("Delete end point", EditorStyles.miniButtonRight, GUILayout.Width(100f)))
            {
                Undo.RecordObjects(new Object[]{target, terrainGenerator.spline}, "Delete terrain piece");
                terrainGenerator.RemoveEndpointFromPiece(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            if (elementProp.isExpanded)
            {
                EditorGUI.BeginChangeCheck();
                DrawChildProperties(elementProp);
                bool changed = EditorGUI.EndChangeCheck();

                if (GUILayout.Button("Rebuild segment " + i))
                {
                    terrainGenerator.dirty.Add(i);
                    EditorUtility.SetDirty(terrainGenerator);
                }

                if (changed)
                {
                    terrainGenerator.dirty.Add(i-1);
                    terrainGenerator.dirty.Add(i);
                    terrainGenerator.dirty.Add(i+1);
                    EditorUtility.SetDirty(terrainGenerator);
                }
            }
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pieces"), true);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("debugShowPlane"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("debugShowSegmentNumber"));

        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Rebuild"))
            terrainGenerator.Rebuild();
        if (GUILayout.Button("Destroy & Rebuild"))
            terrainGenerator.DestroyAndRebuild();
    }

    public void DrawChildProperties(SerializedProperty elementProp)
    {
        EditorGUI.indentLevel++;
        // there is no clean way to traverse only child property, so we need to find our sibbling manually...
        SerializedProperty nextProp = elementProp.Copy(); nextProp.NextVisible(false);
        elementProp.NextVisible(true);
        bool skipAdvance = false;
        do
        {
            if (SerializedProperty.EqualContents(elementProp, nextProp)) // and check for it everytime we do NextVisible...
                break;
            bool expanded = EditorGUILayout.PropertyField(elementProp, false);
            if (expanded)
                DrawChildProperties(elementProp);
            skipAdvance = expanded;
        }
        while (skipAdvance || elementProp.NextVisible(false));
        EditorGUI.indentLevel--;
    }
}
