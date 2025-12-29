using UnityEngine;

public class ZoomManager : MonoBehaviour
{
    private Camera targetCam;
    private float minZoomSize;
    private float maxZoomSize;

    [Header("줌 속도")]
    [SerializeField] private float zoomSpeed = 2f;

    // ZoomManager 활성화 여부
    private bool isZoomEnabled = false;

    void Start()
    {
        targetCam = Camera.main;
    }

    private void Update()
    {
        // Zoom 기능 활성화 상태일 때만 동작
        if (isZoomEnabled)
        {
            HandleZoom();
        }
    }

    private void HandleZoom()
    {
        // 마우스 휠 입력에 따른 줌 조정
        if (Input.GetAxis("Mouse ScrollWheel") != 0)  // 마우스 휠이 움직였을 때만 실행
        {
            float zoomChange = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            Debug.Log(zoomChange);

            // 마우스 화면 위치가 유효한지 확인 (화면 범위 안에 있어야만 줌 처리)
            if (IsMouseInBounds())
            {
                // 카메라 사이즈 변경 (Clamp 제거)
                targetCam.orthographicSize -= zoomChange;

                // 카메라 사이즈를 적절히 제한
                targetCam.orthographicSize = Mathf.Clamp(targetCam.orthographicSize, minZoomSize, maxZoomSize);
            }
        }
    }


    private bool IsMouseInBounds()
    {
        // 마우스가 화면 내에 있는지 체크
        if (Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width &&
            Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height)
        {
            return true;
        }
        return false;
    }

    // ZoomManager를 활성화/비활성화하는 메서드
    public void SetZoomState(bool state)
    {
        isZoomEnabled = state;
    }

    // MinimapManager에서 줌 범위 설정
    public void Init(float minSize, float maxSize)
    {
        minZoomSize = minSize;
        maxZoomSize = maxSize;
        Debug.Log(minZoomSize + " " +  maxZoomSize);
    }

    public bool IsZoomDone()
    {
        return isZoomEnabled;
    }
}
