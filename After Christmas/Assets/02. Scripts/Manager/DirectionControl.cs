using UnityEngine;
using UnityEngine.EventSystems;

public class DirectionControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private MinimapCameraMoveManager cameraMoveManager;  // 카메라 이동을 제어하는 중앙 관리 시스템

    [Header("상하 좌우 방향 설정")]
    [SerializeField] private bool isUp = false;    // 상 (위)
    [SerializeField] private bool isDown = false;  // 하 (아래)
    [SerializeField] private bool isLeft = false;  // 좌 (왼쪽)
    [SerializeField] private bool isRight = false; // 우 (오른쪽)

    private readonly Vector3 isoForward = new Vector3(1, 0, 1).normalized;  // 위 방향 (대각선)
    private readonly Vector3 isoBack = new Vector3(-1, 0, -1).normalized;     // 아래 방향 (대각선)
    private readonly Vector3 isoLeft = new Vector3(-1, 0, 1).normalized;     // 왼쪽 방향 (대각선)
    private readonly Vector3 isoRight = new Vector3(1, 0, -1).normalized;   // 오른쪽 방향 (대각선)

    // 마우스를 올렸을 때 카메라가 해당 방향으로 이동
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("호버링됨");

        // 상하, 좌우 입력값을 받아서 이동 방향 계산
        CalculateMoveDirection();
    }

    // 마우스를 벗어났을 때 카메라 이동 멈추기
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("호버링해제");
        cameraMoveManager.StopMoving();  // 이동 멈추기
    }

    // 상하/좌우 방향을 계산하여 카메라 이동 방향을 설정
    private void CalculateMoveDirection()
    {
        Vector3 moveDirection = Vector3.zero;

        // 상하, 좌우 값에 따라 각 방향 벡터를 합산
        if (isUp)
            moveDirection += isoForward;  // 위 방향
        if (isDown)
            moveDirection += isoBack;    // 아래 방향
        if (isLeft)
            moveDirection += isoLeft;    // 왼쪽 방향
        if (isRight)
            moveDirection += isoRight;   // 오른쪽 방향

        // 대각선 보정을 위해, 크기를 1로 정규화 (단, 한 번에 여러 방향이 선택되면 대각선 이동)
        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        // 카메라 이동 방향 설정
        cameraMoveManager.SetCurrentDirection(moveDirection);
    }
}
