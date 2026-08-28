using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour {

    [SerializeField]
    private PlayerController controller;

    [SerializeField]
    private Material debugMaterial;

    [SerializeField]
    private float cameraDistance = 3f;

    [SerializeField]
    private float cameraHeight = 2f;

    private void FixedUpdate()
    {
        Vector3 target = controller.Placement.Board.position;
        transform.LookAt(target);

        Vector3 direction = (transform.position - target);
        direction.y = 0f;
        transform.position = direction.normalized * cameraDistance + target + cameraHeight * Vector3.up;
    }

    private void OnPostRender()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            return;
        }
#endif

        if (!debugMaterial)
        {
            return;
        }

        if (controller && controller.Posture != null && controller.Placement != null)
        {
            GL.Begin(GL.LINES);
            debugMaterial.SetPass(0);

            var board = controller.Placement.Board;
            var posture = controller.Posture;
            for (int foot = 0; foot < PlayerController.FEET; foot++)
            {
                //GL.Color(foot == PlayerController.LEFT ? Color.red : Color.blue);
                GL.Color(Color.green);

                GL.Vertex(board.TransformPoint(posture.feetPosition[foot]));
                GL.Vertex(board.TransformPoint(posture.kneesPosition[foot]));

                GL.Vertex(board.TransformPoint(posture.kneesPosition[foot]));
                GL.Vertex(board.TransformPoint(posture.hipsPosition[foot]));
            }

            GL.Color(Color.green);
            GL.Vertex(posture.hipsPosition[PlayerController.LEFT]);
            GL.Vertex(posture.hipsPosition[PlayerController.RIGHT]);

            GL.End();
        }

    }

}
