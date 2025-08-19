using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemHandler : MonoBehaviour
{
    // 현재 들고있는 아이템
    private String holdItemID = "";
    // 아이템 목록(이름,좌표리스트)
    private Dictionary<String, List<Transform>> itemList;
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

    public String GetItemID()
    {
        return holdItemID;
    }
}
