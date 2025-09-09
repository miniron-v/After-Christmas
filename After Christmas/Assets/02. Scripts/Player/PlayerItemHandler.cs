using System.Collections.Generic;
using UnityEngine;

public class PlayerItemHandler : MonoBehaviour
{
    [SerializeField] private InventoryUI inventoryUI;

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

        if (item.isTeleportItem && linkedItemIDX >= 0 && linkedItemIDX < item.linkedItems.Count)
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

    public IReadOnlyList<string> GetAllItemIDs() => ItemInfoManager.Instance.GetAllItemIDs(
        ItemInfoManager.Instance.GetSceneIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));

    public IReadOnlyList<string> GetAllItemIDs(int sceneIndex) => ItemInfoManager.Instance.GetAllItemIDs(sceneIndex);

    public IReadOnlyList<SpawnTransform> GetSpawnsOf(string itemID) => ItemInfoManager.Instance.GetSpawnsOf(
        itemID, ItemInfoManager.Instance.GetSceneIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));

    public IReadOnlyList<SpawnTransform> GetSpawnsOf(string itemID, int sceneIndex) => ItemInfoManager.Instance.GetSpawnsOf(itemID, sceneIndex);

    public void Teleport(SpawnTransform selected)
    {
        transform.position = selected.playerSpawnPoint;
        if (Camera.main != null)
            Camera.main.transform.position = selected.cameraSpawnPoint;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && inventoryUI != null)
            inventoryUI.Open(this);

        if (Input.GetKeyDown(KeyCode.G))
            UnHoldItem();
    }
}
