using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlayerItemHandler : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;
    // 현재 들고있는 아이템
    public string holdItemID = "";
    // 아이템 목록(이름,좌표)
    // 한 스테이지에서 다른 스테이지에서 먹은 아이템들을 볼 수는 있게 하려면 3차원 자료구조를 써야할지도 ex.(List<Dict<string,List<>>>)
    private Dictionary<string, List<SpawnTransform>> itemList = new Dictionary<string, List<SpawnTransform>>();
    // 아이템은 씬별로 사용 가능함, 단 하이라이트 씬에서는 전부 사용 가능하게 할것
    private Dictionary<string, string> useableScene = new Dictionary<string, string>();

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
        return itemList.ContainsKey(itemID);
    }

    public void GetNewItem(string itemID)
    {
        itemList.Add(itemID, new List<SpawnTransform>());
    }

    public void RecordItemInfo(string itemID, Transform playerT, Transform cameraT, string mapName)
    {
        Vector3 playerVec = playerT.transform.position;
        Vector3 cameraVec = cameraT.transform.position;
        itemList[itemID].Add(new SpawnTransform(playerVec, cameraVec, mapName));
    }

    #endregion

    #region 텔레포트 관련 함수

    // 아이템의 Teleport와는 다르게, 선택한 정보를 매개변수로 줘서 텔레포트
    public void Teleport(SpawnTransform SelectedTransform)
    {
        // 선택한 위치로 플레이어 이동
        gameObject.transform.position = SelectedTransform.playerSpawnPoint;

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
        var ids = new List<string>(itemList.Keys);
        ids.Sort(StringComparer.Ordinal);
        return ids;
    }

    // 특정 itemID에 기록된 스폰 정보(SpawnTransform) 목록을 반환 (없으면 빈 목록, 읽기 전용)
    public IReadOnlyList<SpawnTransform> GetSpawnsOf(string itemID)
    {
        if (!itemList.TryGetValue(itemID, out var list))
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
}
