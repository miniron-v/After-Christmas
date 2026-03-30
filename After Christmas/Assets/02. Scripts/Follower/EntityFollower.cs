using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EntityFollower : BaseFollower
{
    [Header("유지 최소거리")]
    [SerializeField] private float minDistance = 2f;

    [Header("회전")]
    [SerializeField] private float rotationSpeed = 8f;

    [Header("거리 버퍼(떨림 방지)")]
    [SerializeField] private float stopBuffer = 0.1f;

    [Header("따라가기 속도 최대치")]
    [SerializeField] private float maxFollowSpeed = 6f;

    private Rigidbody rb;
    
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

    protected override void FollowLogic()
    {
        Vector3 toTargetFlat = target.position - rb.position;
        toTargetFlat.y = 0f;

        float distance = UpdateDistance(toTargetFlat);

        Follow(toTargetFlat, distance);
        Rotate(toTargetFlat);
    }

    private float UpdateDistance(Vector3 toTargetFlat)
    {
        float distSqr = toTargetFlat.sqrMagnitude;
        if (distSqr <= EpsilonSqr)
            return 0f;

        float distance = Mathf.Sqrt(distSqr);
        Vector3 dir = toTargetFlat / distance;
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
            ref currentVelocity, 
            smoothTime,
            maxFollowSpeed,
            Time.fixedDeltaTime
        );

        Vector3 desiredVel = (newPos - rb.position) / Time.fixedDeltaTime;
        desiredVel.y = rb.linearVelocity.y;
        rb.linearVelocity = desiredVel;
    }

    private void Rotate(Vector3 toTargetFlat)
    {
        if (toTargetFlat.sqrMagnitude <= EpsilonSqr) return;

        Quaternion targetRot = Quaternion.LookRotation(toTargetFlat.normalized, Vector3.up);

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

        currentVelocity = Vector3.zero;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}