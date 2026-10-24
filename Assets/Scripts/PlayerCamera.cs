using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour {

    [SerializeField]
    private PlayerAnimation playerAnim;

	[SerializeField]
	private PlayerController controller;

    [SerializeField]
    private Material debugMaterial;

    [SerializeField]
    private float cameraDistance = 3f;

    [SerializeField]
    private float cameraHeight = 2f;

	[SerializeField]
	private float minFov = 60f;

	[SerializeField]
	private float maxFov = 120f;

	[SerializeField]
	private Camera cameraComponent;

	[SerializeField]
	private ParticleSystem speedLines;

	private SmoothVector smoothPlayerPosition = new SmoothVector();

    private void FixedUpdate()
	{
		smoothPlayerPosition.Add (playerAnim.GetCameraTarget());

		Vector3 target = smoothPlayerPosition.Get();

		transform.LookAt(target);

        Vector3 direction = (transform.position - target);
        direction.y = 0f;
        transform.position = direction.normalized * cameraDistance + target + cameraHeight * Vector3.up;
    
		if (controller) {
			
			float speed01 = Mathf.Clamp01 (controller.GetStableSpeedAmount ());

			if (cameraComponent) {
				cameraComponent.fieldOfView = Mathf.Lerp (minFov, maxFov, speed01);
			}

			if (speedLines) {
				speedLines.startColor = new Color (1f, 1f, 1f, speed01);
			}
		}
	}

    /*
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
    */

}
