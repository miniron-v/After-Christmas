using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
    [SerializeField] private Transform player;

    [Header("유지 최소거리")]
    [SerializeField] private float minDistance = 2f;

    [Header("부드러움(SmoothDamp)")]
    [SerializeField] private float smoothTime = 0.15f;

    [Header("회전")]
    [SerializeField] private float rotationSpeed = 8f;

    [SerializeField] private Animator animator;
    private Vector3 lastPosition;
    private float stopBuffer = 0.1f;





    private Vector3 velocity = Vector3.zero;

    private Vector3 offset;

    private void OnEnable()
    {
        lastPosition = transform.position;
        TeleportEventManager.OnTeleport += TeleportFollower;
    }

    private void OnDisable()
    {
        TeleportEventManager.OnTeleport -= TeleportFollower;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        offset = player.position - transform.position;
        if (distance > minDistance + stopBuffer)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            Vector3 targetPos = player.position - direction * minDistance;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPos,
                ref velocity,
                smoothTime);


        }
        RotateTowardsPlayer();
        Vector3 movement = transform.position - lastPosition;
        movement.y = 0f;

        bool isMoving = movement.sqrMagnitude > 0.00001f;

        animator.SetBool("isMoving", isMoving);
        lastPosition = transform.position;
    }

    private void TeleportFollower()
    {
        // 텔레포트 후에도 동일한 상대 위치 유지
        transform.position = player.position + offset;
        lastPosition = transform.position;
        // SmoothDamp 잔여 속도 제거
        velocity = Vector3.zero;
    }

    private void RotateTowardsPlayer()
    {
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0f; // 상하 회전 제거

        if (lookDir.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(lookDir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );
    }

}
