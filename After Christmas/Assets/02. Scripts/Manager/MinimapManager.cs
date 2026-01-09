using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEditor.Rendering;

public enum MinimapState { Normal, MinimapView, PostItMode }

public class MinimapManager : MonoBehaviour
{
    public static MinimapManager Instance { get; private set; }

    [Header("상태 관리")]
    private MinimapState currentState = MinimapState.Normal;
    public MinimapState CurrentState => currentState;
    [Header("플레이어 참조")]
    [SerializeField] private Transform player;
    [Header("카메라 참조")]
    [SerializeField] private Camera targetCam;

    [HideInInspector] public Vector3 minimapPosition;
    [HideInInspector] public float minimapSize;

    [Header("전환 관련 설정")]
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private KeyCode minimapToggleKey = KeyCode.Y;
    [SerializeField] private KeyCode postItToggleKey = KeyCode.P;

    [Header("미니맵 클릭 설정")]
    [SerializeField] private LayerMask planeLayerMask;

    [Header("ZoomManager 참조")]
    [SerializeField] private ZoomManager zoomManager;
    [Header("호버링 이동 캔버스 참조")]
    [SerializeField] private GameObject cameraMoveCanvas;
    [Header("포스트잇 버튼이 생성될 패널 참조")]
    [SerializeField] private GameObject buttonPanel;
    private CanvasGroup cameraMoveCanvasGroup;

    private Vector3 originalPosition;
    private float originalSize;

    private bool isMinimapMode = false;
    private bool isPostItMode = false;
    public bool IsPostItMode => isPostItMode;
    private bool isTweening = false;
    private Map hoveredMap = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    private void Start()
    {
        buttonPanel.SetActive(false);
        cameraMoveCanvasGroup = cameraMoveCanvas.GetComponent<CanvasGroup>();

        originalSize = targetCam.orthographicSize;
        if (targetCam == null)
            targetCam = Camera.main;
        if (zoomManager != null)
        {
            // 줌 범위 설정 (originalSize, minimapSize를 줌 범위로 설정)
            zoomManager.Init(originalSize, minimapSize);
        }
    }

    private void Update()
    {
        if (isTweening) return;

        // 🔹 토글 입력
        if (Input.GetKeyDown(minimapToggleKey) && IsMinimapControlEnabled)
        {
            ToggleMinimapView();
        }

        // 🔹 미니맵 모드에서만 hover / click
        if (isMinimapMode && zoomManager.IsZoomDone())
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                TogglePostItView();
            }
            if (!isPostItMode)
            {
                HandleHover();
                if (Input.GetMouseButtonDown(0))
                {
                    HandleMinimapClick();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.U))
            {
                Debug.Log(currentState);
            }
    }

    private void ToggleMinimapView()
    {
        if (!isMinimapMode)
        {
            DisableUnvisitedMapObjects();
            originalPosition = targetCam.transform.position;
            PlayerStateManager.Instance.SetState(PlayerState.MiniMap);
            PostItRegistry.SetAllButtonsVisible(true);
            EnterMinimap();
            currentState = MinimapState.MinimapView;
        }
        else
        {
            isMinimapMode = false;
            PostItRegistry.SetAllButtonsVisible(false);
            CloseAllPostIts();
            if(isPostItMode)
            {
                ExitPostItMode();
            }
            MoveToOriginal(originalPosition, Vector3.zero, false);  // 원래 위치로 돌아갈 때
            zoomManager.SetZoomState(false);
            FadeOutCanvas();
            currentState = MinimapState.Normal;
        }
    }

    private void TogglePostItView()
    {
        if (!isPostItMode)
        {
            currentState = MinimapState.PostItMode;
            EnterPostItMode();
        }
        else
        {
            currentState = MinimapState.MinimapView;
            ExitPostItMode();
        }
    }

    private void EnterPostItMode()
    {
        isPostItMode = true;
        CloseAllPostIts();
        currentState = MinimapState.PostItMode;
        buttonPanel.SetActive(true);
    }

    private void ExitPostItMode()
    {
        isPostItMode = false;
        currentState = MinimapState.MinimapView;
        CloseAllPostIts();
        buttonPanel.SetActive(false);
    }

    private void EnterMinimap()
    {
        isMinimapMode = true;
        currentState = MinimapState.MinimapView;

        isTweening = true;
        cameraMoveCanvas.SetActive(true);
        FadeInCanvas();

        targetCam.transform.DOMove(minimapPosition, transitionDuration).SetEase(Ease.InOutQuad);
        DOTween.To(
            () => targetCam.orthographicSize,
            x => targetCam.orthographicSize = x,
            minimapSize,
            transitionDuration
        ).SetEase(Ease.InOutQuad)
         .OnComplete(() =>
         {
             isTweening = false;
             zoomManager.SetZoomState(true);
         });
    }

    private void CloseAllPostIts()
    {
        PostItRegistry.HideAll();
    }


    public void SaveCurrentCameraAsMinimapView()
    {
        if (targetCam == null)
            targetCam = Camera.main;

        minimapPosition = targetCam.transform.position;
        minimapSize = targetCam.orthographicSize;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        Debug.Log("미니맵 정보 저장 완료");
    }


    private void MoveToOriginal(Vector3 targetCameraPosition, Vector3 targetPlayerPosition, bool isMapMove)
    {
        isTweening = true;
        hoveredMap = null;
        isPostItMode = false;
        isMinimapMode = false;
        SetEnableMapAlpha();
        PostItRegistry.SetAllButtonsVisible(false);
        CloseAllPostIts();

        targetCam.transform
            .DOMove(targetCameraPosition, transitionDuration)
            .SetEase(Ease.InOutQuad);

        // 플레이어 이동 처리
        if (isMapMove)
        {
            player.position = targetPlayerPosition;
            TeleportEventManager.NotifyTeleport();
        }

        // 카메라 사이즈 복원
        DOTween
            .To(() => targetCam.orthographicSize, x => targetCam.orthographicSize = x,
                originalSize, transitionDuration)
            .SetEase(Ease.InOutQuad);

        DOTween
            .To(() => targetCam.orthographicSize, x => targetCam.orthographicSize = x,
                originalSize, transitionDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                isTweening = false;
                EnableAllMapObjects();
                PlayerStateManager.Instance.SetState(PlayerState.Play);
                CloseAllPostIts();
                DisableOtherMap();
            });
    }

    private void DisableOtherMap()
    {
        int sceneIndex = MapInfoManager.Instance.GetSceneIndex(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
        if (sceneIndex < 0) return;

        var mapIDs = MapInfoManager.Instance.GetAllMapIDs(sceneIndex);

        foreach (var mapID in mapIDs)
        {
            Map mapObj = MapInfoManager.Instance.GetMapObject(mapID);
            if (mapID != MapInfoManager.Instance.currentMap)
            {
                mapObj.gameObject.SetActive(false);
            }
        }
    }


    private void DisableUnvisitedMapObjects()
    {
        int sceneIndex = MapInfoManager.Instance.GetSceneIndex(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
        if (sceneIndex < 0) return;

        var mapIDs = MapInfoManager.Instance.GetAllMapIDs(sceneIndex);
        var visitedIDs = MapInfoManager.Instance.GetVisitedMapIDs(sceneIndex);

        HashSet<string> visited = new HashSet<string>(visitedIDs);

        foreach (var mapID in mapIDs)
        {
            Map mapObj = MapInfoManager.Instance.GetMapObject(mapID);
            if (mapObj == null) continue;

            // 방문하지 않은 Map
            if (!visited.Contains(mapID))
            {
                mapObj.gameObject.SetActive(false);
            }
            else
            {
                if (mapID == MapInfoManager.Instance.currentMap)
                {
                    mapObj.gameObject.SetActive(true);
                    mapObj.SetMapAlpha(false);
                }
                else
                {
                    mapObj.gameObject.SetActive(true);
                    Debug.Log("지금1");
                    mapObj.SetMapAlpha(true);
                }
            }
        }
    }

    private void EnableAllMapObjects()
    {
        int sceneIndex = MapInfoManager.Instance.GetSceneIndex(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
        if (sceneIndex < 0) return;

        var mapIDs = MapInfoManager.Instance.GetAllMapIDs(sceneIndex);

        foreach (var mapID in mapIDs)
        {
            Map mapObj = MapInfoManager.Instance.GetMapObject(mapID);
            if (mapObj != null)
            {
                mapObj.gameObject.SetActive(true);
                mapObj.SetMapAlpha(false);
            }
        }

    }

    private void SetEnableMapAlpha()
    {
        int sceneIndex = MapInfoManager.Instance.GetSceneIndex(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
        if (sceneIndex < 0) return;

        var mapIDs = MapInfoManager.Instance.GetAllMapIDs(sceneIndex);

        foreach (var mapID in mapIDs)
        {
            Map mapObj = MapInfoManager.Instance.GetMapObject(mapID);
            if (mapID == MapInfoManager.Instance.currentMap)
            {
                mapObj.SetMapAlpha(false);
                continue;
            }
            if (mapObj != null)
            {
                Debug.Log("지금2");
                mapObj.SetMapAlpha(true);
            }
        }
    }

    /*private void SetEnableMapAlpha(bool isMapMove)
    {
        int sceneIndex = MapInfoManager.Instance.GetSceneIndex(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
        if (sceneIndex < 0) return;

        var mapIDs = MapInfoManager.Instance.GetAllMapIDs(sceneIndex);
        var visitedIDs = MapInfoManager.Instance.GetVisitedMapIDs(sceneIndex);

        HashSet<string> visited = new HashSet<string>(visitedIDs);

        foreach (var mapID in mapIDs)
        {
            Map mapObj = MapInfoManager.Instance.GetMapObject(mapID);
            if (!visited.Contains(mapID))
            {
                continue;
            }
            if (mapID == MapInfoManager.Instance.currentMap)
            {
                mapObj.SetMapAlpha(false);
            }
            else
            {
                mapObj.SetMapAlpha(true);
            }
        }
    }*/

    private bool IsMinimapControlEnabled =>
        PlayerStateManager.Instance.currentState is PlayerState.Play or PlayerState.MiniMap;

    private void HandleMinimapClick()
    {
        Ray ray = targetCam.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(ray.origin, ray.direction * 2000f, Color.yellow, 1f); // 디버그용

        if (Physics.Raycast(ray, out RaycastHit hit, 2000f, planeLayerMask))
        {
            TrySelectMapFromRay(hit);
        }
        else
        {
            Debug.Log("감지 안됨");
        }
    }

    private void TrySelectMapFromRay(RaycastHit hit)
    {
        Map map = hit.collider.GetComponentInParent<Map>();

        if (map != null)
        {
            CursorShapeManager.Instance.RequestCursor(this, CursorType.Default);

            if (MapInfoManager.Instance.currentMap != map.mapName)
            {
                // Map 클릭 시, 다른 맵이라면 해당 Map으로 텔레포트 처리
                MapInfoManager.Instance.currentMap = map.mapName;
                MoveToOriginal(map.cameraSpawnPoint.position, map.playerSpawnPoint.position, true);
            }
            else
            {
                // 원래 맵 클릭했다면 기존 맵으로
                MoveToOriginal(originalPosition, Vector3.zero, false);
            }
            isMinimapMode = false;
            zoomManager.SetZoomState(false);
            FadeOutCanvas();
            currentState = MinimapState.Normal;
        }
    }

    // CanvasGroup을 이용한 페이드 인
    private void FadeInCanvas()
    {
        if (cameraMoveCanvasGroup != null)
        {
            cameraMoveCanvasGroup.alpha = 0f; // 시작은 투명
            cameraMoveCanvasGroup.DOFade(1f, transitionDuration); // 서서히 나타나게 함
        }
    }

    // CanvasGroup을 이용한 페이드 아웃
    private void FadeOutCanvas()
    {
        if (cameraMoveCanvasGroup != null)
        {
            cameraMoveCanvasGroup.DOFade(0f, transitionDuration) // 서서히 사라지게 함
                .OnComplete(() =>
                {
                    cameraMoveCanvas.SetActive(false); // 애니메이션 완료 후 SetActive(false)로 비활성화
                });
        }
    }

    private void HandleHover()
    {
        string currentMapName = MapInfoManager.Instance.currentMap;

        Ray ray = targetCam.ScreenPointToRay(Input.mousePosition);
        bool isHoveringMap = Physics.Raycast(ray, out RaycastHit hit, 2000f, planeLayerMask);

        // 🔹 Raycast 기준으로 커서 먼저 결정 (원래 로직 핵심)
        if (isHoveringMap)
            CursorShapeManager.Instance.RequestCursor(this, CursorType.ButtonHover);
        else
            CursorShapeManager.Instance.RequestCursor(this, CursorType.Default);

        if (isHoveringMap)
        {
            hoveredMap = hit.collider.GetComponentInParent<Map>();
            if (hoveredMap != null)
            {
                // ✅ 현재 맵이면 "alpha만 안 바꾸고" 끝
                if (hoveredMap.mapName == currentMapName)
                {
                    return;
                }

                // 다른 맵 hover
                hoveredMap.SetMapAlpha(false);
                return;
            }
        }
        else
        {
            if (hoveredMap != null)
            {
                if (hoveredMap.mapName == currentMapName)
                {
                    return;
                }
                hoveredMap.SetMapAlpha(true);
                return;
            }

        }

        // 🔹 아무 맵도 hover 안 함 → 정리
        CursorShapeManager.Instance.RequestCursor(this, CursorType.Default);
    }

}
