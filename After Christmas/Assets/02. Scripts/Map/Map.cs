using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("맵 이름 (고유)")]
    public string mapName;

    [Header("현재 맵 내의 오브젝트들로 텔레포트했을 때 카메라 스폰 위치")]
    [SerializeField]
    private Transform cameraSpawnPoint;

    [Header("최초 입장 시 실행할 대화")]
    public DialogueData arrivalDialogue;

    private void Start()
    {
        // MapVisitManager에 등록
        if (!string.IsNullOrEmpty(mapName) && arrivalDialogue != null)
        {
            MapVisitManager.Instance.RegisterMapDialogue(mapName, arrivalDialogue);
        }

        // 자식 아이템들 초기화
        InitializeChildItems();
    }

    private void InitializeChildItems()
    {
        // 자식 오브젝트에서 Item / LockItem 컴포넌트 검색
        Item[] items = GetComponentsInChildren<Item>();
        foreach (var item in items)
        {
            item.mapName = mapName;
            item.cameraSpawnPoint = cameraSpawnPoint;
        }
    }
}
