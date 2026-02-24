using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemHandler : MonoBehaviour
{
    [SerializeField] private NewInventoryUI inventoryUI;

    public string holdItemID { get; private set; } = "";

    public void HoldItem(string itemID)
    {
        HoldItemUI.Instance.Show(ItemInfoManager.Instance.GetItemIcon(itemID));
        holdItemID = itemID;
    }
    public void UnHoldItem()
    {
        HoldItemUI.Instance.Hide();
        holdItemID = "";
    }

    public string GetHoldItemID() => holdItemID;

    public void RecordFromItem(Item item, int linkedItemIDX)
    {
        // 현재 상호작용한 아이템의 추가 설명 기록
        if (!item.isDescriptionRecorded)
        {
            SpawnTransform info = new SpawnTransform(
                item.playerSpawnPoint.position,
                item.cameraSpawnPoint.position,
                item.mapName
            );

            ItemInfoManager.Instance.RecordItemInfo(
                item.itemID,
                info,
                item.itemIcon,
                item.itemDescription,
                item.AdditiveDescription
            );

            item.isDescriptionRecorded = true;
            item.isRecorded = true;
        }

        // 연결된(대상) 아이템의 공간 정보만 미리 기록
        if (item.isTeleportItem && linkedItemIDX >= 0 && linkedItemIDX < item.linkedItems.Count)
        {
            Item linked = item.linkedItems[linkedItemIDX];

            if (!linked.isRecorded)
            {
                SpawnTransform linkedInfo = new SpawnTransform(
                    linked.playerSpawnPoint.position,
                    linked.cameraSpawnPoint.position,
                    linked.mapName
                );

                ItemInfoManager.Instance.RecordItemInfo(
                    linked.itemID,
                    linkedInfo,
                    linked.itemIcon,
                    linked.itemDescription,
                    null // 대상 아이템의 설명은 나중에 그 아이템을 직접 눌렀을 때 기록
                );

                linked.isRecorded = true;

                if (itemAcquiredUI != null)
                {
                    StartCoroutine(itemAcquiredUI.NoticeItemAcquired(item.itemID));
                }
            }
        }
    }

    [SerializeField] private ItemAcquiredUI itemAcquiredUI;

    public bool isHavingItem(string itemID) => ItemInfoManager.Instance.IsHavingItem(itemID);

    public IReadOnlyList<string> GetAllItemIDs() => ItemInfoManager.Instance.GetAllItemIDs(
        ItemInfoManager.Instance.GetSceneIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));

    public IReadOnlyList<string> GetAllItemIDs(int sceneIndex) => ItemInfoManager.Instance.GetAllItemIDs(sceneIndex);

    public IReadOnlyList<SpawnTransform> GetSpawnsOf(string itemID) => ItemInfoManager.Instance.GetSpawnsOf(
        itemID, ItemInfoManager.Instance.GetSceneIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));

    public IReadOnlyList<SpawnTransform> GetSpawnsOf(string itemID, int sceneIndex) => ItemInfoManager.Instance.GetSpawnsOf(itemID, sceneIndex);

    public void Teleport(SpawnTransform selected, string mapID)
    {
        StartCoroutine(TeleportRoutine(selected, mapID));
    }

    private IEnumerator TeleportRoutine(SpawnTransform selected, string mapID)
    {
        // 페이드 아웃
        bool isFadeOutComplete = false;
        FadeManager.Instance.FadeOut(() => { isFadeOutComplete = true; });

        // 페이드 아웃 끝날 때까지 대기
        yield return new WaitUntil(() => isFadeOutComplete);

        // 위치 이동
        transform.position = selected.playerSpawnPoint;
        if (Camera.main != null)
            Camera.main.transform.position = selected.cameraSpawnPoint;

        TeleportEventManager.NotifyTeleport();
        MapInfoManager.Instance.currentMap = mapID;
        // 0.5초 대기
        yield return new WaitForSeconds(0.5f);

        // 페이드 인
        FadeManager.Instance.FadeIn();
    }


    private void Update()
    {
        // inventoryUI가 null이 아니고, 오브젝트가 파괴되지 않은 경우만 열기
        if (Input.GetKeyDown(KeyCode.F) && IsInventoryControlEnabled)
        {
            if (inventoryUI != null && inventoryUI.gameObject != null)
            {
                if (inventoryUI.gameObject.activeSelf)
                    inventoryUI.Close();
                else
                    inventoryUI.Open(this);
            }
        }


        if (Input.GetKeyDown(KeyCode.G))
            UnHoldItem();
    }

    private bool IsInventoryControlEnabled =>
        GameStateManager.Instance.currentState is GameState.Play or GameState.UIOpen;
}
