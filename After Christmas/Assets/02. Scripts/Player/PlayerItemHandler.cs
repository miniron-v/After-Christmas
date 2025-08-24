using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlayerItemHandler : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;
    // 현재 들고있는 아이템
    private String holdItemID = "";
    // 아이템 목록(이름,좌표리스트)
    private Dictionary<String, List<SpawnTransform>> itemList = new Dictionary<String, List<SpawnTransform>>();
    // 아이템은 씬별로 사용 가능함, 단 하이라이트 씬에서는 전부 사용 가능하게 할것
    private Dictionary<String, String> useableScene = new Dictionary<string, string>();

    public void HoldItem(String itemID)
    {
        holdItemID = itemID;
    }

    public void UnHoldItem()
    {
        holdItemID = "";
    }

    public bool isHavingItem(String itemID)
    {
        return itemList.ContainsKey(itemID);
    }

    public void GetNewItem(String itemID)
    {
        itemList.Add(itemID, new List<SpawnTransform>());
    }

    public void RecordItemInfo(string itemID, Transform playerT, Transform cameraT, string mapName)
    {
        Vector3 playerVec = playerT.transform.position;
        Vector3 cameraVec = cameraT.transform.position;
        itemList[itemID].Add(new SpawnTransform(playerVec, cameraVec, mapName));
    }

    // Item에서 상호작용 발생 → 여기서 “기록만” 수행
    public void RecordFromItem(Item item, int linkedItemIDX)
    {
        if (item == null) return;

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

        // 텔레포트 기능 있는 아이템이면 연결 대상 좌표도 기록
        // 연결된 아이템들을 list로 관리함에 따라 인덱스를 추가 파라미터로 받음
        Item linkeditem = item.linkedItems[linkedItemIDX];
        if (item.isTeleportItem && !item.linkedItems[linkedItemIDX].isRecorded)
        {
            RecordItemInfo(item.itemID, linkeditem.playerSpawnPoint, linkeditem.cameraSpawnPoint, linkeditem.mapName);
        }
    }

    public String ReturnItemID()
    {
        return holdItemID;
    }

    // 아이템의 Teleport와는 다르게, 선택한 정보를 매개변수로 줘서 텔레포트
    public void Teleport(SpawnTransform SelectedTransform)
    {
        // 선택한 위치로 플레이어 이동
        gameObject.transform.position = SelectedTransform.playerSpawnPoint;
        //gameObject.transform.rotation = SelectedTransform.playerSpawnPoint.rotation;

        // 카메라 위치 이동
        if (Camera.main != null)
        {
            Camera.main.transform.position = SelectedTransform.cameraSpawnPoint;
            //Camera.main.transform.rotation = SelectedTransform.cameraSpawnPoint.rotation;
        }
    }

    public IReadOnlyList<string> GetAllItemIDs()
    {
        var ids = new List<string>(itemList.Keys);
        ids.Sort(StringComparer.Ordinal);
        return ids;
    }

    public IReadOnlyList<SpawnTransform> GetSpawnsOf(string itemID)
    {
        if (!itemList.TryGetValue(itemID, out var list) || list == null)
            return Array.Empty<SpawnTransform>();
        return list;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            inventoryUI.gameObject.SetActive(true);

            // [ADDED] 인벤토리 UI 열 때 이 핸들러 전달
            var ui = inventoryUI.GetComponent<InventoryUI>();
            if (ui != null) ui.Open(this);
            /*Debug.Log("=== PlayerItemHandler: 아이템 목록 출력 ===");

            
            if (itemList.Count == 0)
            {
                Debug.Log("보유한 아이템 없음");
                return;
            }

            foreach (var kvp in itemList)
            {
                string itemID = kvp.Key;
                Debug.Log($"아이템 ID: {itemID}");

                foreach (var spawn in kvp.Value)
                {
                    string playerPos = spawn.playerSpawnPoint.ToString();
                    string cameraPos = spawn.cameraSpawnPoint.ToString();
                    Debug.Log($"  PlayerSpawn: {playerPos}, CameraSpawn: {cameraPos}");
                }
            }*/
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            UnHoldItem();
        }
    }
}
