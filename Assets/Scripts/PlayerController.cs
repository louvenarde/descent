using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{

    public const byte LEFT = 0;
    public const byte RIGHT = 1;
    public const byte FEET = 2;

    public class HumanoidPlacement
    {
        public HumanoidParameters Parameters { get { return controller.parameters; } }
        public Transform Board { get { return controller.board; } }
        public Transform[] FeetSlots { get { return controller.feetSlots; } }
        public Vector2 LeftFootInput { get { return controller.LeftFootInput; } }
        public Vector2 RightFootInput { get { return controller.RightFootInput; } }

        private readonly PlayerController controller;

        public HumanoidPlacement(PlayerController controller)
        {
            this.controller = controller;
        }
    }

    public class LegsPosture
    {
        public readonly Vector3[] feetDirections = new Vector3[FEET];
        public readonly Vector3[] feetPosition = new Vector3[FEET];

        public Vector3 hipDirectionVector;

        public readonly float[] feetFlex01 = new float[FEET];
        public readonly float[] legsLength = new float[FEET];
        public Vector3 bodyCenterPosition = new Vector3();

        public Vector3[] feetPostureUpVector = new Vector3[FEET];
        public Vector3[] hipsPosition = new Vector3[FEET];

        // Place knees
        public Vector3[] kneesPosition = new Vector3[FEET];


        public void Refresh(HumanoidPlacement data, bool drawGizmos = false)
        {
            RefreshFeetPlacement(data, drawGizmos);
            RefreshHipDirection(data, drawGizmos);
            RefreshLegsLength(data, drawGizmos);
            RefreshBodyCenterPosition(data, drawGizmos);
            RefreshHipsPosition(data, drawGizmos);
            RefreshKneesPlacement(data, drawGizmos);

#if UNITY_EDITOR
            if (drawGizmos)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(
                    data.Board.TransformPoint(Vector3.up * legsLength[LEFT] + hipDirectionVector * data.Parameters.hipWidth * 0.5f),
                     data.Board.TransformPoint(Vector3.up * legsLength[RIGHT] - hipDirectionVector * data.Parameters.hipWidth * 0.5f)
                );
            }
#endif
        }

        private void RefreshFeetPlacement(HumanoidPlacement data, bool drawGizmos = false)
        {
            for (int foot = 0; foot < FEET; foot++)
            {
                Vector2 input = foot == LEFT ? data.LeftFootInput : data.RightFootInput;
                Color color = foot == LEFT ? Color.red : Color.blue;

                Vector3 rotatedInput = new Vector3(input.y, 0f, input.x);

                Vector3 inputLocalDirection = rotatedInput;

                feetDirections[foot] = inputLocalDirection;

                Transform slot = data.FeetSlots[foot];

                if (slot)
                {
#if UNITY_EDITOR
                    if (drawGizmos)
                    {
                        Gizmos.color = Color.Lerp(Color.yellow, color, 0.75f);
                        Gizmos.DrawSphere(slot.position, 0.02f);
                        Gizmos.DrawSphere(slot.position + rotatedInput * 0.2f, 0.03f);
                    }
#endif
                    feetPosition[foot] = data.Board.InverseTransformPoint(slot.position);
                }

                float tiltForwardAmount = inputLocalDirection.z;
                if (foot == RIGHT)
                {
                    tiltForwardAmount *= -1;
                }

                float flex01 = data.Parameters.baseFlex01;
                if (tiltForwardAmount > 0f)
                {
                    flex01 = Mathf.Lerp(flex01, data.Parameters.maxFlex01, tiltForwardAmount);
                }
                else if (tiltForwardAmount < 0)
                {
                    flex01 = Mathf.Lerp(flex01, data.Parameters.minFlex01, -tiltForwardAmount);
                }

                feetFlex01[foot] = flex01;
            }
        }

        private void RefreshHipDirection(HumanoidPlacement data, bool drawGizmos = false)
        {
            // Compute hip direction vector to align the legs
            Vector3 hipDirectionVector =
                (
                    (feetPosition[RIGHT] + feetDirections[RIGHT]) -
                    (feetPosition[LEFT] + feetDirections[LEFT])
                ).normalized;
        }

        private void RefreshLegsLength(HumanoidPlacement data, bool drawGizmos = false)
        {
            for (int foot = 0; foot < FEET; foot++)
            {
                legsLength[foot] = (data.Parameters.thighLength + data.Parameters.ankleLength) * feetFlex01[foot];

#if UNITY_EDITOR
                if (drawGizmos)
                {
                    UnityEditor.Handles.DrawWireDisc(data.Board.TransformPoint(feetPosition[foot]), data.Board.up, legsLength[foot]);
                }
#endif
            }

        }

        private void RefreshBodyCenterPosition(HumanoidPlacement data, bool drawGizmos = false)
        {
            // Compute body center position as the intersection of leg lengths
            {
                Vector3 a1, a2;
                MathsToolkit.GetCirclesIntersection(
                    feetPosition[LEFT], legsLength[LEFT],
                    feetPosition[RIGHT], legsLength[RIGHT],
                    out a1,
                    out a2);

                bodyCenterPosition = a2;

                float up = (legsLength[LEFT] + legsLength[RIGHT]) -
                    (feetPosition[LEFT] - feetPosition[RIGHT]).magnitude;
                bodyCenterPosition.y = up;

#if UNITY_EDITOR
                if (drawGizmos)
                {
                    //Gizmos.color = Color.yellow;
                    //Gizmos.DrawWireSphere(board.TransformPoint(a2), 0.1f);

                    //Gizmos.color = Color.gray;
                    //Gizmos.DrawWireSphere(board.TransformPoint(a1), 0.1f);
                }
#endif
            }
        }

        private void RefreshHipsPosition(HumanoidPlacement data, bool drawGizmos = false)
        {
            for (int foot = 0; foot < FEET; foot++)
            {
                Quaternion leanRot = Quaternion.Euler(0f, 0f, -data.Parameters.maxDegreesRotation * feetDirections[foot].x);
                float upLength = bodyCenterPosition.y;

                Vector3 boardUp = Vector3.up * upLength;
                boardUp.z = bodyCenterPosition.z + data.Parameters.hipWidth * 0.5f * (foot == RIGHT ? 1 : -1);

                boardUp = leanRot * boardUp;

                hipsPosition[foot] = new Vector3(boardUp.x, boardUp.y, boardUp.z);

                feetPostureUpVector[foot] = boardUp - feetPosition[foot];

                Color color = foot == LEFT ? Color.red : Color.blue;

#if UNITY_EDITOR
                if (drawGizmos)
                {
                    Gizmos.color = Color.Lerp(Color.magenta, color, 0.65f);
                    Gizmos.color = new Color(color.r, color.g, color.b, 0.4f);
                    Gizmos.DrawLine(
                        data.Board.TransformPoint(feetPosition[foot]),
                        data.Board.TransformPoint(feetPosition[foot] + feetPostureUpVector[foot])
                    );
                }
#endif
            }

        }

        private void RefreshKneesPlacement(HumanoidPlacement data, bool drawGizmos = false)
        {



            for (int foot = 0; foot < FEET; foot++)
            {
                Color color = foot == LEFT ? Color.red : Color.blue;

                float thighRadius = data.Parameters.thighLength;
                Vector3 hipSphereCenter = hipsPosition[foot];

                float ankleRadius = data.Parameters.ankleLength;
                Vector3 ankleSphereCenter = feetPosition[foot];

                float targetDistance = legsLength[foot];

#if UNITY_EDITOR
                if (drawGizmos)
                {
                    Gizmos.color = new Color(color.r, color.g, color.b, 0.2f);
                    Gizmos.DrawWireSphere(data.Board.TransformPoint(hipSphereCenter), thighRadius);
                    Gizmos.DrawWireSphere(data.Board.TransformPoint(ankleSphereCenter), ankleRadius);
                }
#endif

                float intersectionRadius;
                {
                    // h = 1/2 + (r_1 * r_1 - r_2 * r_2)/(2 * d*d)
                    float h = 1 / 2f + (ankleRadius * ankleRadius - thighRadius * thighRadius) / (2 * targetDistance * targetDistance);

                    // r_i = sqrt(r_1*r_1 - h*h*d*d)
                    intersectionRadius = Mathf.Sqrt(ankleRadius * ankleRadius - h * h * targetDistance * targetDistance);
                }

                // ?? true in the context of the board but like... uuuh...
                Vector3 legRight = Vector3.right;
                Vector3 kneePosition =
                    feetPosition[foot]
                    + feetPostureUpVector[foot].normalized * data.Parameters.ankleLength * feetFlex01[foot]
                    + legRight * intersectionRadius;

                kneesPosition[foot] = kneePosition;

#if UNITY_EDITOR
                if (drawGizmos)
                {
                    Gizmos.color = color;
                    Gizmos.DrawLine(
                        data.Board.TransformPoint(ankleSphereCenter),
                        data.Board.TransformPoint(kneePosition)
                    );

                    Gizmos.DrawLine(
                        data.Board.TransformPoint(hipSphereCenter),
                        data.Board.TransformPoint(kneePosition)
                    );

                    UnityEditor.Handles.color = Color.cyan;
                    UnityEditor.Handles.DrawWireDisc(
                         data.Board.TransformPoint(feetPosition[foot] + feetPostureUpVector[foot].normalized * data.Parameters.ankleLength * feetFlex01[foot]),
                         data.Board.TransformDirection(feetPostureUpVector[foot]),
                         intersectionRadius
                     );
                }
#endif
            }

        }
    }

    [SerializeField]
    private Transform board;

    [SerializeField]
    private Transform[] feetSlots = new Transform[2];

    [Header("Humanoid")]
    [SerializeField]
    private HumanoidParameters parameters;

#if UNITY_EDITOR
    [Header("Input mock")]
    [SerializeField]
    private Vector2 leftStick;

    [SerializeField]
    private Vector2 rightStick;
#endif

    protected Vector2 LeftFootInput
    {
        get
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                return Vector3.ClampMagnitude(leftStick, 1f);
            }
#endif
            if (input != null)
            {
                return input.GetLeftDirection();
            }

            return Vector2.zero;
        }
    }

    protected Vector2 RightFootInput
    {
        get
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                return Vector3.ClampMagnitude(rightStick, 1f);
            }
#endif
            if (input != null)
            {
                return input.GetRightDirection();
            }

            return Vector2.zero;
        }
    }

    public HumanoidPlacement Placement { get { return placement; } }

    public LegsPosture Posture { get { return posture; } }

    private PlayerInput input;

    private readonly LegsPosture posture = new LegsPosture();

    private HumanoidPlacement placement;

    private void Awake()
    {
        placement = new HumanoidPlacement(this);
        input = PlayerInput.MakeForPlatform(0);

    }

    private void Update()
    {
        input.Refresh();
        posture.Refresh(placement);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!board)
        {
            return;
        }

        // Compute feet direction and positions
        UnityEditor.Handles.color = Color.magenta;
        UnityEditor.Handles.ArrowCap(0, board.position + board.forward * 1f, board.rotation, 1f);

        HumanoidPlacement p = new HumanoidPlacement(this);
        posture.Refresh(p, drawGizmos: true);
    }
#endif

}
