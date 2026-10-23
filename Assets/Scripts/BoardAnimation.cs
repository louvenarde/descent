using UnityEngine;
using System.Collections;

public class BoardAnimation : MonoBehaviour {


	[SerializeField]
	private BoardController controller;

	[SerializeField]
	private ParticleSystem snowDriftFX;

	[SerializeField]
	private SpriteRenderer boardTrailSpriteRenderer;

	[Range(0, 1)]
	[SerializeField]
	private float snowFxDriftAmountThreshold = 0.25f;

	bool wasGrounded = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if (!controller) {
			return;
		}

		UpdateTrail ();
		UpdateSnowDriftFX ();

		wasGrounded = controller.IsGrounded;
	}


	private void UpdateSnowDriftFX()
	{
		if (snowDriftFX) {

			float dot = Vector3.Dot (controller.Velocity.normalized, controller.transform.forward);
			float sideMovementAmount = 1f - Mathf.Abs (dot);

			float fxAmount = sideMovementAmount * controller.ControlFromVelocity01;

			if (fxAmount > snowFxDriftAmountThreshold && controller.IsGrounded) {


				snowDriftFX.transform.localEulerAngles = Vector3.up * (90f - 90f * Mathf.Sign(controller.LocalVelocity.x));

				if (!snowDriftFX.isPlaying) {
					snowDriftFX.Play ();
				}
			} else {
				if (snowDriftFX.isPlaying) {
					snowDriftFX.Stop ();
				}
			}
		}
	}

	private void UpdateTrail()
	{
		if (boardTrailSpriteRenderer)
		{
			if (controller.IsGrounded) {

				if (!wasGrounded) {
					boardTrailSpriteRenderer.color = Color.white;
				} 
				else {
					// The first time we hit the snow we want a big impact so we set the sprite to white, then we lerp to a more subtle deformation 
					boardTrailSpriteRenderer.color = Color.Lerp (boardTrailSpriteRenderer.color, Color.grey, 0.02f);
				}
			}
			else
			{
				boardTrailSpriteRenderer.color = new Color(0, 0, 0, 0);
			}
		}

	}
}
