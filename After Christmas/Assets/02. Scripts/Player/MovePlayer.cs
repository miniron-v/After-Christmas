using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovePlayer : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;

    // 45도 쿼터뷰 기준 전방/오른쪽 벡터
    private Vector3 isoForward = new Vector3(1, 0, 1).normalized;
    private Vector3 isoRight = new Vector3(1, 0, -1).normalized;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation; // 회전 고정
    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 입력 방향을 쿼터뷰 좌표로 변환
        Vector3 moveDir = isoForward * v + isoRight * h;

        // 정규화해서 대각선 속도 보정
        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        // Rigidbody 이동
        rb.linearVelocity = moveDir * moveSpeed + new Vector3(0, rb.linearVelocity.y, 0);
    }
}