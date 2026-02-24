using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TargetFollower : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("유지 최소거리")]
    [SerializeField] private float minDistance = 2f;

    [Header("부드러움(SmoothDamp)")]
    [SerializeField] private float smoothTime = 0.15f;

    [Header("회전")]
    [SerializeField] private float rotationSpeed = 8f;

    [Header("거리 버퍼(떨림 방지)")]
    [SerializeField] private float stopBuffer = 0.1f;

    private Rigidbody rb;
    private Vector3 smoothDampVelocity = Vector3.zero;

    // 텔레포트 상대 위치 유지용
    private Vector3 offset;

    private const float EpsilonSqr = 0.0001f * 0.0001f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void OnEnable()
    {
        TeleportEventManager.OnTeleport += TeleportFollower;
    }

    private void OnDisable()
    {
        TeleportEventManager.OnTeleport -= TeleportFollower;
    }

    private void FixedUpdate()
    {
        Vector3 toPlayerFlat = target.position - rb.position;
        toPlayerFlat.y = 0f;

        float distance = UpdateDistance(toPlayerFlat);

        Follow(toPlayerFlat, distance);
        Rotate(toPlayerFlat);
    }

    private float UpdateDistance(Vector3 toPlayerFlat)
    {
        float distSqr = toPlayerFlat.sqrMagnitude;
        if (distSqr <= EpsilonSqr)
            return 0f;

        float distance = Mathf.Sqrt(distSqr);
        Vector3 dir = toPlayerFlat / distance;
        offset = -dir * minDistance;

        return distance;
    }

    private void Follow(Vector3 toTargetFlat, float distance)
    {
        float stopDistance = minDistance + stopBuffer;

        if (distance <= stopDistance || distance <= 0f)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        Vector3 dir = toTargetFlat / distance;
        Vector3 targetPos = target.position - dir * minDistance;
        targetPos.y = rb.position.y;

        Vector3 newPos = Vector3.SmoothDamp(
            rb.position,
            targetPos,
            ref smoothDampVelocity,
            smoothTime
        );

        Vector3 delta = newPos - rb.position;
        Vector3 desiredVel = delta / Time.fixedDeltaTime;
        desiredVel.y = rb.linearVelocity.y;

        rb.linearVelocity = desiredVel;
    }

    private void Rotate(Vector3 toPlayerFlat)
    {
        if (toPlayerFlat.sqrMagnitude <= EpsilonSqr) return;

        Quaternion targetRot = Quaternion.LookRotation(toPlayerFlat.normalized, Vector3.up);

        Quaternion newRot = Quaternion.Slerp(
            rb.rotation,
            targetRot,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newRot);
    }

    private void TeleportFollower()
    {
        rb.position = target.position + offset;

        smoothDampVelocity = Vector3.zero;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}