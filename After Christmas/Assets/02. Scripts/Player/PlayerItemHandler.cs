using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerItemHandler : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;

    // 싱글톤 인스턴스
    public static PlayerItemHandler Instance { get; private set; }

    // 현재 들고있는 아이템
    public string holdItemID = "";

    // 아이템 목록(이름, 좌표) - 3차원 자료구조로 씬 별 아이템을 관리
    private Dictionary<int, Dictionary<string, List<SpawnTransform>>> itemList = new Dictionary<int, Dictionary<string, List<SpawnTransform>>>();

    // 씬 전환에 따른 아이템 사용 가능 여부 관리
    private Dictionary<string, int> useableScene = new Dictionary<string, int>();

    #region 잡기 관련 함수

    public void HoldItem(string itemID)
    {
        holdItemID = itemID;
    }

    public void UnHoldItem()
    {
        holdItemID = "";
    }

    public string GetHoldItemID()
    {
        return holdItemID;
    }

    #endregion

    #region 아이템 저장 관련 함수

    // Item에서 상호작용 발생 → 여기서 “기록만” 수행
    public void RecordFromItem(Item item, int linkedItemIDX)
    {
        // 처음 먹는(아이템 리스트에 없는)아이템이면 먼저 공간 확보
        if (!isHavingItem(item.itemID))
        {
            GetNewItem(item.itemID);
        }
        // 중복저장 방지: 처음 방문일 때만 저장
        if (!item.isRecorded)
        {
            RecordItemInfo(item.itemID, item.playerSpawnPoint, item.cameraSpawnPoint, item.mapName);
        }
        // 반대 아이템이 없다면 반대 아이템은 없으므로 기록 못함, return
        if (!item.isTeleportItem)
        {
            return;
        }

        // 텔레포트 기능 있는 아이템이면 연결 대상 좌표도 기록
        // 연결된 아이템들을 list로 관리함에 따라 인덱스를 추가 파라미터로 받음
        Item linkeditem = item.linkedItems[linkedItemIDX];
        if (!linkeditem.isRecorded)
        {
            RecordItemInfo(item.itemID, linkeditem.playerSpawnPoint, linkeditem.cameraSpawnPoint, linkeditem.mapName);
        }
    }

    public bool isHavingItem(string itemID)
    {
        // 현재 씬에 해당 아이템이 있는지 확인
        int sceneIndex = useableScene[SceneManager.GetActiveScene().name];
        return itemList.ContainsKey(sceneIndex) && itemList[sceneIndex].ContainsKey(itemID);
    }

    public void GetNewItem(string itemID)
    {
        int sceneIndex = useableScene[SceneManager.GetActiveScene().name];

        // 씬 인덱스가 존재하지 않으면 새로 추가
        if (!itemList.ContainsKey(sceneIndex))
        {
            itemList.Add(sceneIndex, new Dictionary<string, List<SpawnTransform>>());
        }

        // 아이템이 없다면 새로 추가
        if (!itemList[sceneIndex].ContainsKey(itemID))
        {
            itemList[sceneIndex].Add(itemID, new List<SpawnTransform>());
        }
    }

    public void RecordItemInfo(string itemID, Transform playerT, Transform cameraT, string mapName)
    {
        int sceneIndex = useableScene[SceneManager.GetActiveScene().name];

        Vector3 playerVec = playerT.transform.position;
        Vector3 cameraVec = cameraT.transform.position;
        
        itemList[sceneIndex][itemID].Add(new SpawnTransform(playerVec, cameraVec, mapName));
    }

    #endregion

    #region 텔레포트 관련 함수

    // 아이템의 Teleport와는 다르게, 선택한 정보를 매개변수로 줘서 텔레포트
    public void Teleport(SpawnTransform SelectedTransform)
    {
        // 선택한 위치로 플레이어 이동
        transform.position = SelectedTransform.playerSpawnPoint;

        // 카메라 위치 이동
        if (Camera.main != null)
        {
            Camera.main.transform.position = SelectedTransform.cameraSpawnPoint;
        }
    }

    #endregion

    #region inventoryUI 관련 함수

    // 아이템 목록에 등록된 모든 itemID를 오름차순으로 정렬해 반환 (읽기 전용)
    public IReadOnlyList<string> GetAllItemIDs()
    {
        int sceneIndex = useableScene[SceneManager.GetActiveScene().name];
        var ids = new List<string>(itemList[sceneIndex].Keys);
        ids.Sort(StringComparer.Ordinal);
        return ids;
    }

    // 특정 itemID에 기록된 스폰 정보(SpawnTransform) 목록을 반환 (없으면 빈 목록, 읽기 전용)
    public IReadOnlyList<SpawnTransform> GetSpawnsOf(string itemID)
    {
        int sceneIndex = useableScene[SceneManager.GetActiveScene().name];

        if (!itemList.ContainsKey(sceneIndex) || !itemList[sceneIndex].TryGetValue(itemID, out var list))
        {
            return Array.Empty<SpawnTransform>();
        }
        return list;
    }

    private void ShowInventoryUI()
    {
        // 인벤토리 UI 열 때 이 핸들러 전달
        InventoryUI ui = inventoryUI.GetComponent<InventoryUI>();
        ui.Open(this);
    }

    #endregion

    #region 생명주기함수

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ShowInventoryUI();
        }

        // 임시로 아이템 내려놓기는 G로 설정
        if (Input.GetKeyDown(KeyCode.G))
        {
            UnHoldItem();
        }
    }

    private void OnEnable()
    {
        // 씬이 변경될 때마다 자동으로 호출되는 이벤트 등록
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        // 이벤트 해제
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    #endregion

    #region 씬 전환 관리 (useableScene)

    // 씬이 변경될 때마다 useableScene에 씬 이름 / 인덱스 추가
    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        string newSceneName = newScene.name;

        // 디폴트 씬은 무시
        if (newSceneName == "디폴트씬(현실) 씬 네임") return;

        // 씬이 처음 등장했을 때 인덱스 추가
        if (!useableScene.ContainsKey(newSceneName))
        {
            useableScene.Add(newSceneName, useableScene.Count);
        }
    }

    #endregion
}
