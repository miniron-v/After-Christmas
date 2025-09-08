using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerItemHandler : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;

    public string holdItemID { get; private set; } = "";

    public void HoldItem(string itemID) => holdItemID = itemID;
    public void UnHoldItem() => holdItemID = "";

    public string GetHoldItemID() => holdItemID;

    public void RecordFromItem(Item item, int linkedItemIDX)
    {
        if (!item.isRecorded)
        {
            SpawnTransform info = new SpawnTransform(
                item.playerSpawnPoint.position,
                item.cameraSpawnPoint.position,
                item.mapName
            );
            ItemInfoManager.Instance.RecordItemInfo(item.itemID, info);
            item.isRecorded = true;
        }

        if (item.isTeleportItem && linkedItemIDX < item.linkedItems.Count)
        {
            Item linked = item.linkedItems[linkedItemIDX];
            if (!linked.isRecorded)
            {
                SpawnTransform info = new SpawnTransform(
                    linked.playerSpawnPoint.position,
                    linked.cameraSpawnPoint.position,
                    linked.mapName
                );
                ItemInfoManager.Instance.RecordItemInfo(linked.itemID, info);
                linked.isRecorded = true;
            }
        }
    }

    public bool isHavingItem(string itemID) => ItemInfoManager.Instance.IsHavingItem(itemID);

    public IReadOnlyList<string> GetAllItemIDs() => ItemInfoManager.Instance.GetAllItemIDs();
    public IReadOnlyList<SpawnTransform> GetSpawnsOf(string itemID) => ItemInfoManager.Instance.GetSpawnsOf(itemID);

    public void Teleport(SpawnTransform selected)
    {
        transform.position = selected.playerSpawnPoint;
        if (Camera.main != null)
            Camera.main.transform.position = selected.cameraSpawnPoint;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && inventoryUI != null)
            inventoryUI.GetComponent<InventoryUI>().Open(this);

        if (Input.GetKeyDown(KeyCode.G))
            UnHoldItem();
    }
}
