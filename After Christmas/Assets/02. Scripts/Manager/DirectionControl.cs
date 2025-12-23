using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DirectionControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private MinimapCameraMoveManager cameraMoveManager;

    [Header("상하 좌우 방향 설정")]
    [SerializeField] private bool isUp = false;
    [SerializeField] private bool isDown = false;
    [SerializeField] private bool isLeft = false;
    [SerializeField] private bool isRight = false;

    [Header("화살표 이미지")]
    [SerializeField] private Image arrowImage;

    private Material runtimeMat;

    private readonly Vector3 isoForward = new Vector3(1, 0, 1).normalized;
    private readonly Vector3 isoBack    = new Vector3(-1, 0, -1).normalized;
    private readonly Vector3 isoLeftVec = new Vector3(-1, 0, 1).normalized;
    private readonly Vector3 isoRightVec= new Vector3(1, 0, -1).normalized;

    private void Awake()
    {
        if (arrowImage != null)
        {
            // UI Material 인스턴스를 따로 만들어주지 않으면 모든 버튼에 반영됨
            runtimeMat = Instantiate(arrowImage.material);
            arrowImage.material = runtimeMat;
        }
    }

    // 마우스 올림
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("호버링됨");
        CalculateMoveDirection();
        EnableGlow(true);
    }

    // 마우스 나감
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("호버링해제");
        cameraMoveManager.StopMoving();
        EnableGlow(false);
    }

    // 실제 GLOW / OUTBASE 토글 함수
    private void EnableGlow(bool enable)
    {
        if (runtimeMat == null) return;

        if (enable)
        {
            runtimeMat.EnableKeyword("GLOW_ON");
            runtimeMat.EnableKeyword("OUTBASE_ON");
        }
        else
        {
            runtimeMat.DisableKeyword("GLOW_ON");
            runtimeMat.DisableKeyword("OUTBASE_ON");
        }
    }

    // 방향 벡터 계산
    private void CalculateMoveDirection()
    {
        Vector3 moveDirection = Vector3.zero;

        if (isUp)
            moveDirection += isoForward;
        if (isDown)
            moveDirection += isoBack;
        if (isLeft)
            moveDirection += isoLeftVec;
        if (isRight)
            moveDirection += isoRightVec;

        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        cameraMoveManager.SetCurrentDirection(moveDirection);
    }
}
