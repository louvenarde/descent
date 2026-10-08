using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorIK : MonoBehaviour {

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private PlayerAnimation playerAnimation;

    [SerializeField]
    private Transform leftFoot;
    [SerializeField]
    private Transform rightFoot;


    [SerializeField]
    private bool lookAt = true;

    [SerializeField]
    private bool ikPlacement = true;

    [SerializeField]
    private bool ikHints = true;

    private void OnAnimatorIK()
    {
        if (playerAnimation)
        {
            if (lookAt)
            {
                if (Camera.main)
                {
                    animator.SetLookAtPosition(Camera.main.transform.position + Camera.main.transform.forward * 100f);
                    animator.SetLookAtWeight(1f, 1f, 1f);
                }
                else
                {
                    animator.SetLookAtWeight(0f, 0f, 0f);
                }
            }            

            if (ikPlacement && leftFoot && rightFoot)
            {
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFoot.position);
                animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFoot.position);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);

                animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFoot.rotation);
                animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFoot.rotation);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);

                animator.bodyPosition = playerAnimation.GetBodyPosition();

                var avgFeetDir = playerAnimation.GetFeetDirection(PlayerController.LEFT) + playerAnimation.GetFeetDirection(PlayerController.RIGHT);
                avgFeetDir /= 2.0f;
                animator.bodyRotation *= Quaternion.Euler(avgFeetDir.x, 0, -avgFeetDir.z);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
            }

            if (ikHints)
            {
                animator.SetIKHintPosition(AvatarIKHint.LeftKnee, playerAnimation.GetKneePosition(PlayerController.LEFT));
                animator.SetIKHintPosition(AvatarIKHint.RightKnee, playerAnimation.GetKneePosition(PlayerController.RIGHT));
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 1f);
                animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 1f);
            }
            else
            {
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 0f);
                animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 0f);
            }
        }

    }
}
