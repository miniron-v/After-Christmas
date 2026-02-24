using UnityEngine;

public class SmoothFollower : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Transform target;   // 따라갈 대상 (플레이어)

    [Header("Camera Follow Settings")]
    [SerializeField] private float smoothTime = 0.125f;

    [SerializeField] private Vector3 cameraOffset;

    private Vector3 currentVelocity = Vector3.zero;

    private void Awake()
    {
        if (!target)
        {
            Debug.LogError("CameraFollow: target이 설정되지 않았습니다.");
            enabled = false;
            return;
        }
    }

    private void FixedUpdate()
    {
        if (GameStateManager.Instance.currentState == GameState.MiniMap) { return; }

        FollowTarget();
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
