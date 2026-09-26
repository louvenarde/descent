using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor
{

    private const int stepsPerCurve = 10;
    private const float directionScale = 0.5f;
    private const float handleSize = 0.04f;
    private const float pickSize = 0.06f;

    private static Color[] modeColors = {
        Color.white,
        Color.yellow,
        Color.cyan
    };

    private BezierSpline spline;
    private Transform handleTransform;
    private int selectedIndex = -1;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        spline = target as BezierSpline;
        if (GUILayout.Button("Append point at the end"))
        {
            Undo.RecordObject(spline, "Append point");
            spline.AddCurve();
            EditorUtility.SetDirty(spline);
        }

        if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount)
        {
            EditorGUILayout.LabelField("Selected Point:");
            EditorGUI.indentLevel++;
            DrawSelectedPointInspector();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Button("Remove selected point"))
            {
                Undo.RecordObject(spline, "Remove Point");
                spline.RemoveCurve((selectedIndex + 1) / 3);
                EditorUtility.SetDirty(spline);
            }
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

    }

    private void DrawSelectedPointInspector()
    {
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Move Point");
            EditorUtility.SetDirty(spline);
            spline.SetControlPoint(selectedIndex, point);
        }
        EditorGUI.BeginChangeCheck();
        BezierSpline.PointMode mode = (BezierSpline.PointMode)EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Change Point Mode");
            spline.SetControlPointMode(selectedIndex, mode);
            EditorUtility.SetDirty(spline);
        }
    }

    private void OnSceneGUI()
    {
        spline = target as BezierSpline;
        handleTransform = spline.transform;

        Vector3 p0 = ShowPoint(0);
        for (int i = 1; i < spline.ControlPointCount; i += 3)
        {
            Vector3 p1 = ShowPoint(i);
            Vector3 p2 = ShowPoint(i + 1);
            Vector3 p3 = ShowPoint(i + 2);

            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
            p0 = p3;
        }

        if (spline.debugShowDirection)
        {
            ShowDirections();
        }
    }

    private void ShowDirections()
    {
        int steps = stepsPerCurve * spline.CurveCount;
        for (int i = 0; i <= steps; i++)
        {
            Vector3 point = spline.GetPoint(i / (float)steps);
            Vector3 forward = spline.GetForward(i / (float)steps);
            Vector3 right = spline.GetRight(i / (float)steps);
            Vector3 up = spline.GetUp(i / (float)steps);
            Handles.color = Color.red;
            Handles.DrawLine(point, point + right * directionScale);
            Handles.color = Color.green;
            Handles.DrawLine(point, point + up * directionScale);
            Handles.color = Color.blue;
            Handles.DrawLine(point, point + forward * directionScale);
        }
    }

    private Vector3 ShowPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
        float size = HandleUtility.GetHandleSize(point);
        Handles.color = modeColors[(int)spline.GetControlPointMode(index)];
        if (index % 3 == 0)
            Handles.color = Color.red;

        if (Handles.Button(point, Quaternion.identity, size * handleSize, size * pickSize, Handles.DotCap))
        {
            selectedIndex = index;
            Repaint();
        }
        if (selectedIndex == index)
        {
            EditorGUI.BeginChangeCheck();

            Quaternion handleRotation = Quaternion.identity;
            if(Tools.pivotRotation == PivotRotation.Local)
            {
                float splinePos = ((index + 1) / 3) / (float)spline.CurveCount;
                handleRotation = Quaternion.LookRotation(spline.GetForward(splinePos), spline.GetUp(splinePos));
            }

            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
            }
        }
        return point;
    }
}