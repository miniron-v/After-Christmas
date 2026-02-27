using UnityEngine;

public class SmoothFollower : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Transform target;   // 따라갈 대상 (플레이어)

    [Header("Camera Follow Settings")]
    [SerializeField] private float smoothTime = 0.125f;

    [SerializeField] private Vector3 cameraOffset;

    private Vector3 currentVelocity = Vector3.zero;
    private bool isFollowing = true;

    private void Awake()
    {
        if (!target)
        {
            Debug.LogError("SmoothFollower: target이 설정되지 않았습니다.");
            enabled = false;
            return;
        }
    }

    private void FixedUpdate()
    {
        if (!isFollowing) return;
        FollowTarget();
    }

    public void StartFollow()
    {
        isFollowing = true;
    }

    public void StopFollow()
    {
        isFollowing = false;
        currentVelocity = Vector3.zero; // 잔떨림방지
    }

    private void FollowTarget()
    {
        Vector3 desiredPos = target.position + cameraOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref currentVelocity,
            smoothTime
        );
    }
}