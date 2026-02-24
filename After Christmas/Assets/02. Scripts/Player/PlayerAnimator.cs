using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private float movingSpeedThreshold = 0.05f;

    private Rigidbody rb;
    private float ThresholdSqr => movingSpeedThreshold * movingSpeedThreshold;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!animator) return;

        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation()
    {
        Vector3 v = rb.linearVelocity;
        v.y = 0f;

        bool isMoving = v.sqrMagnitude > ThresholdSqr;
        animator.SetBool("isMoving", isMoving);
    }
}