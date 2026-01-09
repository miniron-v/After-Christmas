using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerCameraFollowManager : MonoBehaviour
{
    [Header("Camera Follow Settings")]
    public Camera targetCamera;          // 따라오는 카메라
    public float smoothTime = 0.125f;

    [SerializeField] private Vector3 cameraOffset;
    private Vector3 currentVelocity = Vector3.zero;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;

        if (targetCamera == null)
        {
            Debug.LogError("PlayerCameraFollowManager: targetCamera가 설정되지 않았습니다.");
            return;
        }

        // Player = this.transform
        // cameraOffset = targetCamera.transform.position - transform.position;
    }

    private void FixedUpdate()
    {
        if (PlayerStateManager.Instance.currentState == PlayerState.MiniMap)
            return;

        Vector3 desiredPos = transform.position + cameraOffset;

        targetCamera.transform.position = Vector3.SmoothDamp(
            targetCamera.transform.position,
            desiredPos,
            ref currentVelocity,
            smoothTime
        );
    }
}
