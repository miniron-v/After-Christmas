using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class MinimapManager : MonoBehaviour
{
    [Header("카메라 참조")]
    [SerializeField] private Camera targetCam;

    [HideInInspector] public Vector3 minimapPosition;
    [HideInInspector] public float minimapSize;

    [Header("전환 관련 설정")]
    [SerializeField] private float transitionDuration = 1f;

    [Header("미니맵 클릭 설정")]
    [SerializeField] private LayerMask planeLayerMask; 

    private Vector3 originalPosition;
    private float originalSize;

    private bool isMinimapMode = false;
    private bool isTweening = false;

    private void Start()
    {
        if (targetCam == null)
            targetCam = Camera.main;
    }

    private void Update()
    {
        if (isTweening) return;

        // Y 키 토글
        if (Input.GetKeyDown(KeyCode.Y) && IsMinimapControlEnabled)
        {
            ToggleMinimapView();
        }

        // 미니맵 모드에서 클릭 처리
        if (isMinimapMode && Input.GetMouseButtonDown(0))
        {
            HandleMinimapClick();
        }
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

    private void ToggleMinimapView()
    {
        if (!isMinimapMode)
        {
            DisableUnvisitedMapObjects();
            originalPosition = targetCam.transform.position;
            originalSize = targetCam.orthographicSize;

            PlayerStateManager.Instance.SetState(PlayerState.MiniMap);
            MoveToMinimap();
        }
        else
        {
            MoveToOriginal();
        }

        isMinimapMode = !isMinimapMode;
    }

    private void MoveToMinimap()
    {
        isTweening = true;

        targetCam.transform
            .DOMove(minimapPosition, transitionDuration)
            .SetEase(Ease.InOutQuad);

        DOTween
            .To(() => targetCam.orthographicSize, x => targetCam.orthographicSize = x,
                minimapSize, transitionDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => isTweening = false);
    }

    private void MoveToOriginal()
    {
        isTweening = true;

        targetCam.transform
            .DOMove(originalPosition, transitionDuration)
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
            });
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
                mapObj.gameObject.SetActive(true);
        }
    }

    private bool IsMinimapControlEnabled =>
        PlayerStateManager.Instance.CurrentState is PlayerState.Play or PlayerState.MiniMap;

    private void HandleMinimapClick()
    {
        Ray ray = targetCam.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(ray.origin, ray.direction * 500f, Color.yellow, 1f); // 디버그용

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, planeLayerMask))
        {
            TrySelectMapFromRay(hit);
        }
        else
        {
            Debug.Log("감지안됨");
        }
    }

    private void TrySelectMapFromRay(RaycastHit hit)
    {
        Map map = hit.collider.GetComponentInParent<Map>();

        if (map != null)
        {
            Debug.Log($"{map.mapName}");
        }
    }
}
