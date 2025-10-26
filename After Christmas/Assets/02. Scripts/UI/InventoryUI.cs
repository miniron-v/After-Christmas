using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private PoolManager buttonPool;     // 범용 버튼 풀
    [SerializeField] private Button prevSceneButton;
    [SerializeField] private Button nextSceneButton;
    [SerializeField] private TeleportSelectionUI teleportSelectionUI;

    private PlayerItemHandler handler;
    private List<string> itemSnapshot = new();
    private List<SpawnTransform> spawnSnapshot = new();
    private string currentItemID = null;
    private int currentSceneIndex = 0;
    private int activeSceneIndex = 0;

    // 씬별 버튼 매핑
    private Dictionary<string, Button> itemButtons = new();

    private void Awake()
    {
        if (prevSceneButton != null)
            prevSceneButton.onClick.AddListener(() => ChangeScenePage(-1));
        if (nextSceneButton != null)
            nextSceneButton.onClick.AddListener(() => ChangeScenePage(1));
    }

    public void Open(PlayerItemHandler h)
    {
        if (gameObject.activeSelf) return;

        handler = h;
        activeSceneIndex = ItemInfoManager.Instance.GetSceneIndex(SceneManager.GetActiveScene().name);
        currentSceneIndex = activeSceneIndex;

        UIModalGate.Acquire(this, Close);
        gameObject.SetActive(true);
        BuildItems();
    }

    public void Close()
    {
        if (this == null || gameObject == null) return;

        gameObject.SetActive(false);
        handler = null;
        itemSnapshot.Clear();
        spawnSnapshot.Clear();
        currentItemID = null;

        UIModalGate.Release(this);
    }

    private void BuildItems()
    {
        buttonPool.ReleaseAll();   // 모든 버튼 비활성화
        itemButtons.Clear();       // 딕셔너리 초기화

        var ids = handler.GetAllItemIDs(currentSceneIndex);
        itemSnapshot.Clear();
        itemSnapshot.AddRange(ids);

        foreach (var itemID in itemSnapshot)
        {
            var obj = buttonPool.Get();
            obj.transform.SetParent(contentRoot, false);

            var btn = obj.GetComponent<Button>();
            itemButtons[itemID] = btn;

            var txt = obj.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = itemID;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                currentItemID = itemID;
                if (handler.isHavingItem(itemID))
                {
                    handler.HoldItem(itemID);
                    BuildSpawns();
                }
            });

            btn.interactable = true;
            btn.gameObject.SetActive(true);
        }

        UpdateSceneButtons();
    }



    private void BuildSpawns()
    {
        if (handler == null) return;

        var spawns = handler.GetSpawnsOf(handler.GetHoldItemID(), currentSceneIndex);
        spawnSnapshot.Clear();
        spawnSnapshot.AddRange(spawns);

        if (spawnSnapshot.Count == 0)
        {
            Debug.Log("해당 아이템에 사용할 스폰 지점이 없습니다.");
            UpdateSceneButtons();
            return;
        }

        List<string> labels = new List<string>();
        foreach (var spawn in spawnSnapshot)
            labels.Add(spawn.mapName);

        Vector3 headPos = handler != null && handler.gameObject != null
            ? handler.gameObject.transform.position + Vector3.up * 1.8f
            : Vector3.zero;

        UIModalGate.Release(this);

        teleportSelectionUI.Open(headPos, labels, (selectedIndex) =>
        {
            if (selectedIndex < 0 || selectedIndex >= spawnSnapshot.Count) return;

            handler.Teleport(spawnSnapshot[selectedIndex]);
            teleportSelectionUI.Close();
        });

        gameObject.SetActive(false);
        UpdateSceneButtons();
    }

    private void ChangeScenePage(int dir)
    {
        int sceneCount = ItemInfoManager.Instance.GetSceneCount();
        currentSceneIndex = Mathf.Clamp(currentSceneIndex + dir, 0, sceneCount - 1);
        BuildItems();
    }

    private void UpdateSceneButtons()
    {
        int sceneCount = ItemInfoManager.Instance.GetSceneCount();

        if (prevSceneButton != null)
        {
            bool hasPrev = currentSceneIndex > 0 && ItemInfoManager.Instance.HasVisitedScene(currentSceneIndex - 1);
            prevSceneButton.gameObject.SetActive(hasPrev);
        }

        if (nextSceneButton != null)
        {
            bool hasNext = currentSceneIndex < sceneCount - 1 && ItemInfoManager.Instance.HasVisitedScene(currentSceneIndex + 1);
            nextSceneButton.gameObject.SetActive(hasNext);
        }
    }
}
