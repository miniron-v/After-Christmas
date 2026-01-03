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

        Vector3 moveDir = isoForward * moveInput.y + isoRight * moveInput.x;
        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        Vector3 targetlinearVelocity = moveDir * moveSpeed;
        rb.linearVelocity = new Vector3(targetlinearVelocity.x, rb.linearVelocity.y, targetlinearVelocity.z);

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
}