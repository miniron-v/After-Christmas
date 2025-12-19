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

    // 기존 isometric 방향
    private readonly Vector3 isoForward = new Vector3(1, 0, 1).normalized;
    private readonly Vector3 isoRight = new Vector3(1, 0, -1).normalized;

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        if (!PlayerStateManager.Instance.IsPlayerControllable())
            return;

        // 1. 입력 → 이동 벡터 변환
        Vector3 moveDir = isoForward * moveInput.y + isoRight * moveInput.x;

        // 2. 대각선 이동 속도 보정
        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        bool isMoving = moveDir.sqrMagnitude > 0.0001f;

        // 3. transform 기반 이동
        transform.position += moveDir * moveSpeed * Time.deltaTime;

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
