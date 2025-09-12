using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Button prevSceneButton;
    [SerializeField] private Button nextSceneButton;
    [SerializeField] private TeleportSelectionUI teleportSelectionUI;

    private PlayerItemHandler handler;

    private List<string> itemSnapshot = new();
    private List<SpawnTransform> spawnSnapshot = new();
    private string currentItemID = null;
    private int currentSceneIndex = 0;
    private int activeSceneIndex = 0;

    private void Awake()
    {
        if (prevSceneButton != null)
            prevSceneButton.onClick.AddListener(() => ChangeScenePage(-1));
        if (nextSceneButton != null)
            nextSceneButton.onClick.AddListener(() => ChangeScenePage(1));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Open(PlayerItemHandler h)
    {
        handler = h;
        activeSceneIndex = ItemInfoManager.Instance.GetSceneIndex(SceneManager.GetActiveScene().name);
        currentSceneIndex = activeSceneIndex;

        // InventoryUI가 Gate 확보
        UIModalGate.Acquire(this, Close);

        gameObject.SetActive(true);
        BuildItems();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        handler = null;
        itemSnapshot.Clear();
        spawnSnapshot.Clear();
        currentItemID = null;
    }

    private void BuildItems()
    {
        Clear(contentRoot);
        itemSnapshot.Clear();
        spawnSnapshot.Clear();
        currentItemID = null;

        if (handler == null) return;

        var ids = handler.GetAllItemIDs(currentSceneIndex);
        itemSnapshot.AddRange(ids);

        bool isCurrentScenePage = currentSceneIndex == activeSceneIndex;

        for (int i = 0; i < itemSnapshot.Count; i++)
        {
            int captured = i;
            var btn = Instantiate(buttonPrefab, contentRoot);
            btn.GetComponentInChildren<TMP_Text>().text = itemSnapshot[i];
            btn.interactable = isCurrentScenePage;

            btn.onClick.AddListener(() =>
            {
                if (!btn.interactable)
                {
                    Debug.Log("해당 씬에서는 사용할 수 없는 아이템입니다.");
                    return;
                }

                currentItemID = itemSnapshot[captured];
                if (handler.isHavingItem(currentItemID))
                {
                    handler.HoldItem(currentItemID);
                    BuildSpawns();
                }
            });
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

        // InventoryUI를 끄기 전에 Gate Release
        UIModalGate.Release(this);

        // Teleport UI 열기
        teleportSelectionUI.Open(headPos, labels, (selectedIndex) =>
        {
            if (selectedIndex < 0 || selectedIndex >= spawnSnapshot.Count) return;

            handler.Teleport(spawnSnapshot[selectedIndex]);
            teleportSelectionUI.Close();

        });

        // InventoryUI 비활성화
        gameObject.SetActive(false);

        UpdateSceneButtons();
    }

    private void Clear(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
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
