using UnityEngine;
using System.Collections;

public class PlayerAnimation : MonoBehaviour
{
    [System.Serializable]
    public class LegSkeleton
    {
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

                if (thighs[foot])
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

                    thighs[foot].position = board.TransformPoint
                        (
                            posture.hipsPosition[foot]
                        );
                }

                if (shins[foot])
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

                    shins[foot].position = board.TransformPoint
                        (
                            posture.kneesPosition[foot]
                        );
                }

                if (feet[foot])
                {
                    feet[foot].rotation = Quaternion.LookRotation(
                        -board.right,
                        board.up
                        ) * Quaternion.Euler(0f, 0f, 180f)
                         * Quaternion.Euler(-10f, 0f, 0f)
                    ;


                    feet[foot].position = board.TransformPoint
                        (
                            posture.feetPosition[foot]
                        ) + board.up * 0.12f;
                }

            }
        }
    }

    [SerializeField]
    private PlayerController player;

    [SerializeField]
    private Transform board;

    [SerializeField]
    private LegSkeleton legs;

    [SerializeField]
    private Transform hipCenter;

    void LateUpdate()
    {
        if (player && board)
        {
            if (hipCenter)
            {
                hipCenter.position = board.TransformPoint(
                    (player.Posture.hipsPosition[PlayerController.LEFT] + player.Posture.hipsPosition[PlayerController.RIGHT]) / 2f
                );
            }

            legs.Place(board, player.Posture);
        }
    }

    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (player)
        {
            for(int foot = 0; foot < PlayerController.FEET; foot++)
            {
                Gizmos.color = foot == PlayerController.LEFT ? Color.magenta : Color.cyan;
                Gizmos.DrawSphere(board.TransformPoint(player.Posture.feetPosition[foot]), 0.05f);
                Gizmos.DrawSphere(board.TransformPoint(player.Posture.hipsPosition[foot]), 0.05f);
                Gizmos.DrawSphere(board.TransformPoint(player.Posture.kneesPosition[foot]), 0.05f);

            }
        }
        
#endif
    }
}