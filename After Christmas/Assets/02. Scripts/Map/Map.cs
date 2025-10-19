using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("맵 이름 (고유)")]
    public string mapName;

    [Header("현재 맵 내의 오브젝트들로 텔레포트했을 때 카메라 스폰 위치")]
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

    private void Start()
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
        Item[] items = GetComponentsInChildren<Item>();
        foreach (var item in items)
        {
            item.mapName = mapName;
            item.cameraSpawnPoint = cameraSpawnPoint;
        }
    }
}
