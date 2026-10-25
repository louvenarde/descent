using System.Collections;
using UnityEngine;
using UnityEngine.Analytics;

public class BoardController : MonoBehaviour
{
    public struct BodyEffect
    {
        public float torque;
        public float steepnessVelocityMultiplier;
        public float frictionIncrease;
        public float tiltSide;
        public bool isFallen;
		public float crossingDirections01;
    }

    [SerializeField]
    private BoardProfile profile;

    [SerializeField]
    private Collider[] boardColliders = new Collider[0];

    [SerializeField]
    private Rigidbody boardBody;

    [SerializeField]
    private Transform boardElementsParent;

    [SerializeField]
    private Transform boardVisual;

    [SerializeField]
    private Transform[] localAttachPoints = new Transform[0];

    [SerializeField]
    private float castLength = 0.15f;

    [SerializeField]
    private LayerMask groundMask = new LayerMask() { value = 0xFFFF };

    public bool IsGrounded { get { return groundHits > 0; } }

	public float NoseDiveAmountM101 { get { return noseDiveTendency; } }

    public Vector3 Velocity { get { return velocity; } }

	public Vector3 LocalVelocity { get { return transform.InverseTransformVector (Velocity); } }

    public float ControlFromVelocity01 { get { return Mathf.Clamp01((3.6f * velocity.magnitude / Time.fixedDeltaTime) / profile.minimumControlVelocityKmh); } }

    private Vector3 velocity;

    private Vector3 lastPosition;

    private float velocityAngle;

    private SmoothFloat smoothVelocityAngle = new SmoothFloat();

    private float noseDiveTendency;

    private float tiltSideAmount = 0f;

    private byte groundHits = 0;

	private float gripMultiplier = 0f;

    private BodyEffect currentBodyEffect;

    private readonly Vector3[] hits = new Vector3[2];

    private PhysicMaterial canonicalPhysicalMaterial;

    private PhysicMaterial dynamicPhysicsMaterial;

    public void SetBodyEffect(BodyEffect bodyEffect)
    {
        currentBodyEffect = bodyEffect;
    }

    public string Dump()
    {
        return string.Join("\n",
            new string[] {
                string.Format("Velocity angle: {0}°", velocityAngle.ToString("n0")),
                string.Format("Smooth velocity angle: {0}°", smoothVelocityAngle.Get().ToString("n0")),
                string.Format("Friction: {0}", dynamicPhysicsMaterial.dynamicFriction.ToString("n2")),
                string.Format("Ground hits: {0}", groundHits),
                string.Format("Grounded: {0}", IsGrounded),
                string.Format("Raw velocity: {0}", velocity),
                string.Format("Edge friction: {0}%", (GetEdgeFrictionAmount01() * 100).ToString("n0")),
				string.Format("Grip multiplier: {0}%", (gripMultiplier * 100).ToString("n0")),
				string.Format("BE X Dir: {0}%", (currentBodyEffect.crossingDirections01 * 100).ToString("n0")),
                string.Format("BE Torque: {0}", (currentBodyEffect.torque).ToString("n1")),
                string.Format("BE Steep Incr: x{0}", (currentBodyEffect.steepnessVelocityMultiplier).ToString("n2")),
                string.Format("BE Frix Incr: {0}%", (currentBodyEffect.frictionIncrease * 100).ToString("n0")),
				string.Format("BE Tilt Side: {0}", (currentBodyEffect.tiltSide).ToString("n1")),
            }
        );
    }

    void Awake()
    {
        if (boardColliders.Length > 0)
        {
            canonicalPhysicalMaterial = boardColliders[0].sharedMaterial;
            dynamicPhysicsMaterial = PhysicMaterial.Instantiate(canonicalPhysicalMaterial);

            for (int i = 0; i < boardColliders.Length; i++)
            {
                boardColliders[i].sharedMaterial = dynamicPhysicsMaterial;
            }
        }
    }

    private float GetEdgeFrictionAmount01()
    {
        return Mathf.Clamp01(Mathf.Abs(smoothVelocityAngle / 90f));
    }

    private void Update()
    {
        if (boardVisual)
        {
            float tilt = - Mathf.Lerp((velocityAngle / 180f), tiltSideAmount, 0.5f);
            boardVisual.localEulerAngles = Vector3.forward * profile.maxVisualTiltDegrees * tilt;
        }

    }

    private void FixedUpdate()
    {
        velocity = transform.position - lastPosition;

        if (currentBodyEffect.isFallen)
        {
            // Do nothing

			dynamicPhysicsMaterial.dynamicFriction = 1;
        }
        else
        {
			if (PlaceOnGround ()) {
				// Grounded status has changed
				if (IsGrounded) {
					// We just landed

					float dot = Vector3.Dot (transform.forward, Velocity.normalized);
					gripMultiplier = Mathf.Clamp01(dot); // If we landed facing velocity we keep the grip, otherwise we lose it
				}
			}

            RefreshMaths();

            RefreshTilt();

            RefreshPhysicalMaterial();

			RefreshRigidBodyDrag ();

            AddTorqueFromBodyEffect();

			if (IsGrounded) {

				ModifyGripFromBodyEffect ();

				BendThrustTowardsDirection ();

				ModifySpeedFromBodyEffect ();

				AddSympatheticAngularVelocity ();

				RecoverGrip ();
			}
        }

        lastPosition = transform.position;
    }

	private void RecoverGrip()
	{
		if (gripMultiplier < 1f) {
			gripMultiplier += Time.deltaTime / profile.gripRecoveryTimeSeconds;
			gripMultiplier = Mathf.Clamp01 (gripMultiplier);
		}
	}

    private void RefreshTilt()
    {
        tiltSideAmount = Mathf.Clamp(currentBodyEffect.tiltSide, -1f, 1f);
    }


    private void ModifySpeedFromBodyEffect()
    {
			boardBody.AddForce (
				boardBody.transform.forward
				* currentBodyEffect.steepnessVelocityMultiplier
				* noseDiveTendency
				* Time.deltaTime,
				ForceMode.Acceleration
			);
    }

    private void AddTorqueFromBodyEffect()
    {
		float reducerFromVelocityAngle = Mathf.Clamp01(Mathf.Abs (velocityAngle) / 60f);

		// If noseDive is Zero, we're going up
		// If we're going up and we're crossing legs, it's time to stop applying torque
		float multiplierFromSteepCross = 
			1f - reducerFromVelocityAngle * currentBodyEffect.crossingDirections01;


        boardBody.AddTorque(
            boardBody.transform.up
                * currentBodyEffect.torque
                * ControlFromVelocity01
				* multiplierFromSteepCross // This will have no effect except when we're climbing up AND crossing
                * Time.deltaTime,
            ForceMode.Acceleration
        );
    }

	private void ModifyGripFromBodyEffect()
	{
		gripMultiplier *= 1f - currentBodyEffect.crossingDirections01;
	}

    private void BendThrustTowardsDirection()
    {
		Vector3 velocityWithoutGravity = new Vector3 (boardBody.velocity.x, 0f, boardBody.velocity.z);
		Quaternion rotationTowardsDirection = Quaternion.FromToRotation (velocityWithoutGravity.normalized, boardBody.transform.forward);

		Quaternion lerpedRotation = Quaternion.Lerp (Quaternion.identity, rotationTowardsDirection, profile.grip * gripMultiplier);

		boardBody.velocity = lerpedRotation * velocityWithoutGravity + Vector3.up * boardBody.velocity.y;
	} 

    private void AddSympatheticAngularVelocity()
	{
		float multiplier = 1f - currentBodyEffect.crossingDirections01;
		multiplier *= Mathf.Clamp01 (noseDiveTendency);

		boardBody.AddTorque (
			boardBody.transform.up
			* smoothVelocityAngle.Get ()
			* profile.sympatheticCorrectionForce
//                * ControlFromVelocity01
			* multiplier
			* Time.deltaTime,
			ForceMode.Acceleration
		);
    }

    private void RefreshMaths()
    {
        Vector3 fwd = transform.forward;
        if (IsGrounded)
        {
            float attachPointsLength = Vector3.Distance(hits[0], hits[1]);
            noseDiveTendency = (hits[1].y - hits[0].y) / attachPointsLength;
            velocityAngle = MathsToolkit.SignedAngle(fwd, velocity, transform.up);
            smoothVelocityAngle.Add(velocityAngle);
        }
    }

	private void RefreshRigidBodyDrag()
	{
		boardBody.angularDrag = Mathf.Lerp (profile.turnDrag, profile.crossedTurnDrag, currentBodyEffect.crossingDirections01);
	}

    private void RefreshPhysicalMaterial()
    {
        if (IsGrounded)
        {
            dynamicPhysicsMaterial.dynamicFriction = Mathf.Lerp(canonicalPhysicalMaterial.dynamicFriction, profile.maxEdgeFriction, GetEdgeFrictionAmount01() * ControlFromVelocity01);
        }
        else
        {
            dynamicPhysicsMaterial.dynamicFriction = canonicalPhysicalMaterial.dynamicFriction;
        }
    }

    private bool PlaceOnGround()
    {
		bool wasGrounded = IsGrounded;
        groundHits = 0;
        for (int i = 0; i < localAttachPoints.Length; i++)
        {
            if (localAttachPoints[i])
            {
                RaycastHit hit;
                Color debugColor = Color.yellow;
                Vector3 dir = -localAttachPoints[i].transform.up;
                Vector3 basePosition = localAttachPoints[i].transform.position;

                if (Physics.Raycast(basePosition - dir * castLength, dir, out hit, castLength * 2, groundMask))
                {
                    debugColor = Color.green;
                    hits[i] = hit.point;
                    groundHits++;
                }
                else
                {
                    hits[i] = basePosition + dir * castLength;
                }

                Debug.DrawRay(basePosition - dir * castLength, dir * castLength * 2, debugColor);
            }
        }

        if (groundHits > 0)
        {
            Vector3 center = (hits[0] + hits[1]) / 2;
            Quaternion rotation = Quaternion.LookRotation(hits[0] - hits[1]);
            boardElementsParent.rotation = rotation;
        }

		return wasGrounded != IsGrounded;
    }

#if DEBUG
    void OnDrawGizmos()
    {
        UnityEditor.Handles.Label(transform.position + Vector3.up, string.Format("IsGrounded:{0}\nVelocity: {1}\nAngle: {2}°\nNose dive: {3}%", groundHits, velocity, Mathf.FloorToInt(velocityAngle), Mathf.FloorToInt(noseDiveTendency * 100)), new GUIStyle() { fontSize = 12, fontStyle = FontStyle.Bold, normal = new GUIStyleState() { textColor = Color.red } });
    }
#endif
}
