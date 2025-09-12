using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MovePlayer : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector2 moveInput;

    private readonly Vector3 isoForward = new Vector3(1, 0, 1).normalized;
    private readonly Vector3 isoRight   = new Vector3(1, 0, -1).normalized;

    [Header("Camera Follow")]
    public Camera mainCamera;
    public float cameraSmoothSpeed = 5f;

    private Vector3 cameraOffset; // 플레이어 기준 초기 offset

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
        {
            cameraOffset = mainCamera.transform.position - transform.position; // 초기 offset 저장
        }
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

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            Vector3 desiredPos = transform.position + cameraOffset;
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, desiredPos, cameraSmoothSpeed * Time.deltaTime);
            // 회전은 그대로 유지 → 카메라는 플레이어를 항상 중앙에 담음
        }
    }
}
