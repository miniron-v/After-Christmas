using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("이동 속도")]
    [Tooltip("플레이어의 좌우 이동 속도")]
    public float moveSpeed = 5f;

    [Header("점프 설정")]
    [Tooltip("점프 시 처음 받는 속도 (높이 결정)")]
    public float jumpForce = 7f;

    [Tooltip("중력 가속도에 곱해지는 값 (낙하 속도 조절)")]
    public float gravityScale = 3f;

    [Header("땅 체크")]
    [Tooltip("플레이어 발 위치 기준 아래로 얼마나 내릴지")]
    public float temp = 0.45f;

    [Tooltip("땅으로 인식할 레이어")]
    public LayerMask groundLayer;

    [Header("점프 입력 버퍼")]
    [Tooltip("점프 입력이 버퍼링되는 시간 (초)")]
    public float jumpInputBufferDuration = 0.15f;

    private Rigidbody rb;
    private Vector2 moveInput = Vector2.zero;
    private float verticalVelocity = 0f;
    private float jumpInputBufferTime = -1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            // 점프 버튼 누른 시간 저장
            jumpInputBufferTime = Time.time;
        }
    }

    private void FixedUpdate()
    {
        // 중력 적용 (낙하 속도 제어)
        verticalVelocity += Physics.gravity.y * gravityScale * Time.fixedDeltaTime;

        // 점프 입력 버퍼가 유효하고, 땅에 닿아있으면 점프 실행
        if (IsGroundedBoxCast() && verticalVelocity < 0f)
        {
            if (Time.time - jumpInputBufferTime <= jumpInputBufferDuration)
            {
                verticalVelocity = jumpForce;
                jumpInputBufferTime = -1f; // 버퍼 초기화
            }
            else
            {
                verticalVelocity = 0f; // 땅에 닿으면 수직 속도 초기화
            }
        }

        // 좌우 이동 + 수직 속도 반영
        Vector3 newVelocity = new Vector3(moveInput.x * moveSpeed, verticalVelocity, 0f);
        rb.linearVelocity = newVelocity;
    }

    private bool IsGroundedBoxCast()
    {
        Vector3 boxCastOrigin = transform.position + Vector3.down * temp;
        Vector3 halfExtents = new Vector3(0.4f, 0.03f, 0.4f);
        float maxDistance = 0.2f;
        Vector3 direction = Vector3.down;

        return Physics.BoxCast(
            boxCastOrigin,
            halfExtents,
            direction,
            out RaycastHit hit,
            Quaternion.identity,
            maxDistance,
            groundLayer
        );
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 boxCastOrigin = transform.position + Vector3.down * temp;
        Vector3 halfExtents = new Vector3(0.4f, 0.03f, 0.4f);
        float maxDistance = 0.2f;
        Vector3 direction = Vector3.down;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(boxCastOrigin, halfExtents * 2);

        Vector3 boxCastEnd = boxCastOrigin + direction * maxDistance;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCastEnd, halfExtents * 2);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(boxCastOrigin, boxCastEnd);
    }
}
