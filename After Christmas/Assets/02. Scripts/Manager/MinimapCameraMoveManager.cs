using UnityEngine;

public class MinimapCameraMoveManager : MonoBehaviour
{
    [Header("카메라 이동 설정")]
    public Camera mainCamera;  // 카메라 참조
    public float moveSpeed = 3f;  // 카메라 이동 속도
    private Vector3 currentDirection;  // 카메라 이동 방향
    private bool isMoving = false;  // 이동 여부

    void Update()
    {
        // 이동 여부가 true일 때만 이동
        if (isMoving)
        {
            MoveCamera(currentDirection);
        }
    }

    // 카메라 이동 함수
    private void MoveCamera(Vector3 direction)
    {
        mainCamera.transform.position += direction * moveSpeed * Time.deltaTime;
    }

    // 방향을 설정하고 이동을 시작
    public void SetCurrentDirection(Vector3 direction)
    {
        currentDirection = direction;
        isMoving = true;  // 이동 시작
    }

    // 이동을 멈추는 함수
    public void StopMoving()
    {
        currentDirection = Vector3.zero;
        isMoving = false;  // 이동 멈춤
    }
}
