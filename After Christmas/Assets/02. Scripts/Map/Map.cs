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

    // 맵 입장 시 사용할 시네마틱 컨트롤러
    // awake에서 초기화해 주므로 hide in inspector
    [HideInInspector]
    public CinematicController cinematicController;

    private void Awake()
    {
        // 변수 할당 최소화를 위한 getcomponent 사용
        cinematicController = GetComponent<CinematicController>();
    }

    void Start()
    {
        if (!string.IsNullOrEmpty(mapName))
        {
            MapVisitManager.Instance.RegisterMap(mapName, this);
        }

        // 자식 아이템 초기화
        InitializeChildItems();
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
}
