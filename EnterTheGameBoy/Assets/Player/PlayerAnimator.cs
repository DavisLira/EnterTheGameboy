using UnityEngine;
using Mirror;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement movement;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        movement = GetComponentInParent<PlayerMovement>();
    }

    void Update()
    {
        if (movement == null || animator == null) return;

        animator.SetBool("IsMoving", movement.syncIsMoving);

        if (movement.syncIsMoving)
        {
            animator.SetFloat("MoveX", movement.syncMoveX);
            animator.SetFloat("MoveY", movement.syncMoveY);
        }
        else
        {
            animator.SetFloat("MoveX", movement.syncMoveX * 0.01f);
            animator.SetFloat("MoveY", movement.syncMoveY * 0.01f);
        }
    }
}