using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlayerItemHandler : MonoBehaviour
{
    // 현재 들고있는 아이템
    private String holdItemID = "";
    // 아이템 목록(이름,좌표리스트)
    private Dictionary<String, HashSet<SpawnTransform>> itemList = new Dictionary<string, HashSet<SpawnTransform>>();
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
        itemList.Add(itemID, new HashSet<SpawnTransform>());
    }

    public void RecordItemInfo(string itemID, Transform playerT, Transform cameraT)
    {
        itemList[itemID].Add(new SpawnTransform(playerT, cameraT));
    }

    // Item에서 상호작용 발생 → 여기서 “기록만” 수행
    public void RecordFromItem(Item item, int linkedItemIDX)
    {
        if (item == null) return;

        // 처음 먹는(아이템 리스트에 없는)아이템이면 먼저 공간 확보
        if (!itemList.ContainsKey(item.itemID))
        {
            GetNewItem(item.itemID);
        }

        // 자기 자신 좌표 기록
        RecordItemInfo(item.itemID, item.playerSpawnPoint, item.cameraSpawnPoint);

        // 텔레포트 기능 있는 아이템이면 연결 대상 좌표도 기록
        // 연결된 아이템들을 list로 관리함에 따라 인덱스를 추가 파라미터로 받음
        if (item.isTeleportItem)
        {
            RecordItemInfo(item.itemID, item.linkedItems[linkedItemIDX].playerSpawnPoint, item.linkedItems[linkedItemIDX].cameraSpawnPoint);
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
        gameObject.transform.position = SelectedTransform.playerSpawnPoint.position;
        gameObject.transform.rotation = SelectedTransform.playerSpawnPoint.rotation;

        // 카메라 위치 이동
        if (Camera.main != null)
        {
            Camera.main.transform.position = SelectedTransform.cameraSpawnPoint.position;
            Camera.main.transform.rotation = SelectedTransform.cameraSpawnPoint.rotation;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
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
                    string playerPos = spawn.playerSpawnPoint
                        ? spawn.playerSpawnPoint.position.ToString()
                        : "null";
                    string cameraPos = spawn.cameraSpawnPoint
                        ? spawn.cameraSpawnPoint.position.ToString()
                        : "null";

                    Debug.Log($"  PlayerSpawn: {playerPos}, CameraSpawn: {cameraPos}");
                }
            }*/


        }
    }
}
