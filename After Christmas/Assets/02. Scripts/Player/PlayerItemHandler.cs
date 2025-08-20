using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public struct SpawnTransform
{
    public Transform playerSpawnPoint;
    public Transform cameraSpawnPoint;

    public SpawnTransform(Transform player, Transform camera)
    {
        playerSpawnPoint = player;
        cameraSpawnPoint = camera;
    }
}

public class PlayerItemHandler : MonoBehaviour
{
    // 현재 들고있는 아이템
    private String holdItemID = "";
    // 아이템 목록(이름,좌표리스트)
    private Dictionary<String, HashSet<SpawnTransform>> itemList;
    // 아이템은 씬별로 사용 가능함, 단 하이라이트 씬에서는 전부 사용 가능하게 할것
    private Dictionary<String, String> useableScene;

    public void HoldItem(String itemID)
    {
        holdItemID = itemID;
    }

    public void ClearItem()
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

    public void RecordItemInfo(String itemID, Transform playerTransform, Transform cameraTransform)
    {
        SpawnTransform itemInfo = new SpawnTransform(playerTransform, cameraTransform);
        if (itemList.ContainsKey(itemID))
        {
            itemList[itemID].Add(itemInfo);
        }
    }

    public String ReturnItemID()
    {
        return holdItemID;
    }
}
