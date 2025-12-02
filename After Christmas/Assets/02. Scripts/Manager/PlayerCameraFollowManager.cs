using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerCameraFollowManager : MonoBehaviour
{
    [Header("Camera Follow Settings")]
    public Camera targetCamera;          // 따라오는 카메라
    public float smoothSpeed = 5f;

    private Vector3 cameraOffset;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
        {
            Debug.LogError("PlayerCameraFollowManager: targetCamera가 설정되지 않았습니다.");
            return;
        }

        // Player = this.transform
        cameraOffset = targetCamera.transform.position - transform.position;
    }

    private void LateUpdate()
    {
        if (PlayerStateManager.Instance.CurrentState == PlayerState.MiniMap)
        {
            return;
        }
        if (targetCamera == null)
            return;

        Vector3 desiredPos = transform.position + cameraOffset;

        targetCamera.transform.position = Vector3.Lerp(
            targetCamera.transform.position,
            desiredPos,
            smoothSpeed * Time.deltaTime
        );
    }
}
