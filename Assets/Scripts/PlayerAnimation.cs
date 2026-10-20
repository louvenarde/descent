using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAnimation : MonoBehaviour
{
    [System.Serializable]
    public class LegSkeleton
    {
        [SerializeField]
        private bool placeThighs = true;

        [SerializeField]
        private bool placeShins = true;

        [SerializeField]
        private bool placeFeet = true;

        [SerializeField]
        private bool placeHeels = true;

        [SerializeField]
        private bool setLimbPositions = true;

        [SerializeField]
        private Transform[] thighs = new Transform[PlayerController.FEET];

        [SerializeField]
        private Transform[] shins = new Transform[PlayerController.FEET];

        [SerializeField]
        private Transform[] feet = new Transform[PlayerController.FEET];

        [SerializeField]
        private Transform[] heels = new Transform[PlayerController.FEET];

        public void Place(Transform board, PlayerController.LegsPosture posture)
        {
            for (int i = 0; i < PlayerController.FEET; i++)
            {
                int foot = i;

                if (thighs[foot] && placeThighs)
                {
                    thighs[foot].rotation = Quaternion.LookRotation(
                        board.TransformPoint
                        (
                            posture.kneesPosition[foot]
                        ) -
                        board.TransformPoint
                        (
                            posture.hipsPosition[foot]
                        ),
                        board.up
                    ) * Quaternion.Euler(90f, 0f, 0f);

                    thighs[foot].localRotation *= Quaternion.Euler(0f, -90f, 0f);

                    if (setLimbPositions)
                    {
                        thighs[foot].position = board.TransformPoint
                            (
                                posture.hipsPosition[foot]
                            );
                    }
                }

                if (shins[foot] && placeShins)
                {
                    shins[foot].rotation = Quaternion.LookRotation(
                        board.TransformPoint
                        (
                            posture.feetPosition[foot]
                        ) -
                        board.TransformPoint
                        (
                            posture.kneesPosition[foot]
                        ),
                        board.up
                    ) * Quaternion.Euler(-90f, 0, 0f)
                    * Quaternion.Euler(0f, 0f, 180f);

                    if (setLimbPositions)
                    {
                        shins[foot].position = board.TransformPoint
                        (
                            posture.kneesPosition[foot]
                        );
                    }
                }

                if (feet[foot] && placeFeet)
                {
                    feet[foot].rotation = Quaternion.LookRotation(
                        board.up,
                        -board.right);

                }

                if (heels[foot] && placeHeels)
                {
                    if (setLimbPositions)
                    {
                        heels[foot].position = board.TransformPoint
                        (
                            posture.feetPosition[foot]
                        );
                    }
                }

            }
        }
    }

    [System.Serializable]
    public class Ragdoll
    {
        [System.Serializable]
        public class RagdollBodyPart
        {
            public Rigidbody body;
            public Collider collider;

            public void Activate()
            {
                body.isKinematic = false;
                collider.enabled = true;
            }

            public void Deactivate()
            {
                body.isKinematic = true;
                collider.enabled = false;
            }
        }

        public RagdollBodyPart[] parts;

		public void Activate(Vector3 impulse=default(Vector3))
        {
            for(int i = 0; i < parts.Length; i++)
            {
                parts[i].Activate();

				parts [i].body.AddForce (impulse, ForceMode.VelocityChange);
            }
        }

        public void Deactivate()
        {
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i].Deactivate();
            }
        }
    }

    [SerializeField]
    private Transform hipCenter;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private Ragdoll ragdoll;

    [SerializeField]
    private bool placeHipCenter = false;

    [SerializeField]
    private bool rotateHipCenter = false;


    [SerializeField]
    private LegSkeleton legs;

    private PlayerController.LegsPosture posture;

    private PlayerController.HumanoidPlacement placement;

    private Transform board;

    private bool isFallen;

    public void SetPosture(PlayerController.LegsPosture posture, PlayerController.HumanoidPlacement placement, Transform board)
    {
        this.posture = posture;
        this.placement = placement;
        this.board = board;
    }

	public void SetFallen(bool isFalling, Vector3 fallImpulse=default(Vector3))
	{
		if (isFalling != isFallen)
		{
			if (isFalling)
			{
				ragdoll.Activate(fallImpulse);
				animator.enabled = false;
			}
			else
			{
				ragdoll.Deactivate();
				animator.enabled = true;
			}

			isFallen = isFalling;
		}

	}

    public Vector3 GetCameraTarget()
    {
        if (isFallen && ragdoll != null && ragdoll.parts.Length > 0)
        {
            return ragdoll.parts[0].body.position;
        }
		else if (board)
		{
			return board.transform.position;
		}
        else
        {
            return GetBodyPosition();
        }
    }

    public Vector3 GetBodyPosition()
    {
        if (posture != null && board)
        {
            return board.TransformPoint(
                posture.bodyCenterPosition
            );
        }

        return transform.position;
    }

    public Vector3 GetFeetPosition(byte leg)
    {
        if (posture != null)
        {
            return board.TransformPoint(
                posture.feetPosition[leg]
            );
        }

        return Vector3.zero;
    }

    public Vector3 GetFeetDirection(byte leg)
    {
        if (posture != null)
        {
            return posture.feetDirections[leg] * placement.Parameters.maxDegreesRotation;
        }

        return Vector3.zero;
    }

    public Vector3 GetHipPosition(byte leg)
    {
        if (posture != null)
        {
            return board.TransformPoint(
                posture.hipsPosition[leg]
            );
        }

        return Vector3.zero;
    }

    public Vector3 GetKneePosition(byte leg)
    {
        if (posture != null)
        {
            return board.TransformPoint(
                posture.kneesPosition[leg]
            );
        }

        return Vector3.zero;
    }

    void LateUpdate()
    {
        if (placement != null && posture != null && board)
        {
            if (hipCenter)
            {
                if (placeHipCenter)
                {
                    hipCenter.position = board.TransformPoint(
                        (posture.hipsPosition[PlayerController.LEFT] + posture.hipsPosition[PlayerController.RIGHT]) / 2f
                    );
                }
            }

            legs.Place(board, posture);
        }
    }

#if UNITY_EDITOR


    [ContextMenu("Grab bones")]
    private void GrabBones()
    {
        if (animator)
        {
            var root = animator.transform.FindChild("metarig").FindChild("root");

            if (root)
            {
                List<Ragdoll.RagdollBodyPart> parts = new List<Ragdoll.RagdollBodyPart>();
                var colliders = root.gameObject.GetComponentsInChildren<Collider>();
                for (int i = 0; i < colliders.Length; i++)
                {
                    var col = colliders[i];
                    var body = colliders[i].GetComponent<Rigidbody>();

                    if (body)
                    {
                        Ragdoll.RagdollBodyPart part = new Ragdoll.RagdollBodyPart()
                        {
                            body = body,
                            collider = col
                        };

                        part.Deactivate();
                        parts.Add(part);
                    }

                }

                this.ragdoll.parts = parts.ToArray();
            }
        }
    }
#endif

    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (posture != null)
        {
            for (int foot = 0; foot < PlayerController.FEET; foot++)
            {
                Gizmos.color = foot == PlayerController.LEFT ? Color.magenta : Color.cyan;
                Gizmos.DrawSphere(board.TransformPoint(posture.feetPosition[foot]), 0.05f);
                Gizmos.DrawSphere(board.TransformPoint(posture.hipsPosition[foot]), 0.05f);
                Gizmos.DrawSphere(board.TransformPoint(posture.kneesPosition[foot]), 0.05f);
            }
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(board.TransformPoint(posture.bodyCenterPosition), 0.05f);
        }

#endif
    }
}