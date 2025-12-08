using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("맵 관련 데이터 (SO)")]
    public MapDataSO mapDataSO;

    // 맵 id, so에서 관리하므로 hide in inspector
    [HideInInspector]
    public string mapName => mapDataSO.mapID;

    [Header("현재 맵으로 텔레포트했을 때 플레이어 스폰 위치")]
    public Transform playerSpawnPoint;

    [Header("현재 맵으로 텔레포트했을 때 카메라 스폰 위치")]
    public Transform cameraSpawnPoint;

    [Header("최초 입장 시 실행할 대화")]
    public DialogueData arrivalDialogue;

    private Renderer[] mapRenderers;

    // 맵 입장 시 사용할 시네마틱 컨트롤러
    // awake에서 초기화해 주므로 hide in inspector
    [HideInInspector]
    public CinematicController cinematicController;

    public bool isStartMap = false;

    private void Awake()
    {
        // 변수 할당 최소화를 위한 getcomponent 사용
        cinematicController = GetComponent<CinematicController>();
    }

    void Start()
    {
        MapInfoManager.Instance.RecordMap(this);
        InitializeChildItems();
        CacheRenderers();
    }

    private void InitializeChildItems()
    {
        Item[] items = GetComponentsInChildren<Item>(true);
        foreach (var item in items)
        {
            item.mapName = mapName;
            item.cameraSpawnPoint = cameraSpawnPoint;
        }
    }

    // ① 모든 Renderer 캐싱
    private void CacheRenderers()
    {
        mapRenderers = GetComponentsInChildren<Renderer>(true);
    }

    public void SetDepthColoring(bool enable)
    {
        if (mapRenderers == null) return;

        foreach (Renderer r in mapRenderers)
        {
            Material mat = r.material;

            if (enable)
            {
                // 켜기
                mat.EnableKeyword("_DEPTH_COLORING_ON");
                mat.SetFloat("_DepthColoring", 1f);
            }
            else
            {
                // 끄기
                mat.DisableKeyword("_DEPTH_COLORING_ON");
                mat.SetFloat("_DepthColoring", 0f);
            }
        }
    }
}
