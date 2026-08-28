using System.Collections;
using UnityEngine;
using UnityEngine.Analytics;

public class BoardController : MonoBehaviour
{
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

    public float ControlFromVelocity01 { get { return Mathf.Clamp01(velocity.magnitude / profile.minimumControlVelocity); } }

    private Vector3 velocity;

    private Vector3 lastPosition;

    private float velocityAngle;

    private float noseDiveTendency;

    private byte groundHits = 0;

    private readonly Vector3[] hits = new Vector3[2];

    private PhysicMaterial canonicalPhysicalMaterial;

    private PhysicMaterial dynamicPhysicsMaterial;

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

    private void Update()
    {
        if (boardVisual)
        {
            boardVisual.localEulerAngles = Vector3.forward * profile.maxVisualTiltDegrees * (- velocityAngle / 180f);
        }
    }

    private void FixedUpdate()
    {
        velocity = transform.position - lastPosition;

        PlaceOnGround();

        RefreshMaths();

        RefreshPhysicalMaterial();

        AddSympatheticAngularVelocity();

        lastPosition = transform.position;
    }

    private void AddSympatheticAngularVelocity()
    {
        boardBody.AddTorque(
            boardBody.transform.up
                * velocityAngle 
                * profile.sympatheticCorrectionForce
                * ControlFromVelocity01
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
            noseDiveTendency = (hits[0].y - hits[1].y) / attachPointsLength;
            velocityAngle = MathsToolkit.SignedAngle(fwd, velocity, transform.up);
        }
    }

    private void RefreshPhysicalMaterial()
    {
        if (IsGrounded && velocity.sqrMagnitude > 0.01f)
        {
            dynamicPhysicsMaterial.dynamicFriction = Mathf.Lerp(canonicalPhysicalMaterial.dynamicFriction, profile.maxEdgeFriction, Mathf.Abs(velocityAngle / 90f));
        }
        else
        {
            dynamicPhysicsMaterial.dynamicFriction = canonicalPhysicalMaterial.dynamicFriction;
        }
    }

    private void PlaceOnGround()
    {
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
    }

#if DEBUG
    void OnDrawGizmos()
    {
        UnityEditor.Handles.Label(transform.position + Vector3.up, string.Format("IsGrounded:{0}\nVelocity: {1}\nAngle: {2}°\nNose dive: {3}%", groundHits, velocity, Mathf.FloorToInt(velocityAngle), Mathf.FloorToInt(noseDiveTendency * 100)), new GUIStyle() { fontSize = 12, fontStyle = FontStyle.Bold, normal = new GUIStyleState() { textColor = Color.red } });
    }
#endif
}
