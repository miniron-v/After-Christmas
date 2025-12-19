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


    private Vector3 velocity = Vector3.zero;

    private Vector3 offset;

    private void OnEnable()
    {
        TeleportEventManager.OnTeleport += TeleportFollower;
    }

    private void OnDisable()
    {
        TeleportEventManager.OnTeleport -= TeleportFollower;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        offset = transform.position - player.position;

        float distance = Vector3.Distance(transform.position, player.position);

        // 최솟값보다 가까우면 멈춤
        if (distance <= minDistance) return;

        // 플레이어 쪽 방향
        Vector3 direction = (player.position - transform.position).normalized;

        // 목표 위치 = 플레이어 기준 minDistance 만큼 떨어진 위치
        Vector3 targetPos = player.position - direction * minDistance;

        // SmoothDamp로 부드럽게 이동
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            smoothTime);

        RotateTowardsPlayer();
    }

    private void TeleportFollower()
    {
        // 텔레포트 후에도 동일한 상대 위치 유지
        transform.position = player.position + offset;

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
