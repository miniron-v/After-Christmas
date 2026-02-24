using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class IsometricMover : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 10f;

    private Vector2 moveInput;
    private Rigidbody rb;
    private readonly Vector3 isoForward = new Vector3(1, 0, 1).normalized;
    private readonly Vector3 isoRight = new Vector3(1, 0, -1).normalized;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        if (!PlayerStateManager.Instance.IsPlayerControllable())
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        Vector3 moveDir = CalculateMoveDir(moveInput);
        ApplyMovement(moveDir);
        ApplyRotation(moveDir);
    }

    private Vector3 CalculateMoveDir(Vector2 input)
    {
        Vector3 dir = isoForward * input.y + isoRight * input.x;

        if (dir.sqrMagnitude > 1f)
            dir.Normalize();

        return dir;
    }

    private void ApplyMovement(Vector3 moveDir)
    {
        Vector3 v = moveDir * moveSpeed;
        rb.linearVelocity = new Vector3(v.x, rb.linearVelocity.y, v.z);
    }

    private void ApplyRotation(Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );
    }
}