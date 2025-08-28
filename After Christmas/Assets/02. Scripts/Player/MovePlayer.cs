using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MovePlayer : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;

    // 45도 쿼터뷰 기준 벡터
    private readonly Vector3 isoForward = new Vector3(1, 0, 1).normalized;
    private readonly Vector3 isoRight   = new Vector3(1, 0, -1).normalized;

    // OnMove()에서 값 저장
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        Vector3 moveDir = isoForward * moveInput.y + isoRight * moveInput.x;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveDir.x * moveSpeed;
        velocity.z = moveDir.z * moveSpeed;
        rb.linearVelocity = velocity;
    }
}
