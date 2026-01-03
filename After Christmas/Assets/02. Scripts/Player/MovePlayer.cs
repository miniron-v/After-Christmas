using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MovePlayer : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 5f;

    [Header("Rotation")]
    public float rotationSpeed = 10f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private Vector2 moveInput;
    private Rigidbody rb;

    private readonly Vector3 isoForward = new Vector3(1, 0, 1).normalized;
    private readonly Vector3 isoRight = new Vector3(1, 0, -1).normalized;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        if (!PlayerStateManager.Instance.IsPlayerControllable())
            return;

        Vector3 moveDir = isoForward * moveInput.y + isoRight * moveInput.x;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        animator.SetBool("isMoving", isMoving); 
    }

    private void FixedUpdate()
    {
        if (!PlayerStateManager.Instance.IsPlayerControllable())
        {
            // 컨트롤 불가능할 때 미끄러지지 않도록 속도 제어
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        Vector3 moveDir = isoForward * moveInput.y + isoRight * moveInput.x;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        // Velocity 기반 이동으로 변경
        Vector3 targetVelocity = moveDir * moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }
}