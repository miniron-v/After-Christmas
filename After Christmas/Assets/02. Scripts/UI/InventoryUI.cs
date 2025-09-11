using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Button backToItemsButton;
    [SerializeField] private Button prevSceneButton;
    [SerializeField] private Button nextSceneButton;

    private PlayerItemHandler handler;
    private enum PanelState { Items, Spawns }
    private PanelState state = PanelState.Items;

    private List<string> itemSnapshot = new();
    private List<SpawnTransform> spawnSnapshot = new();
    private string currentItemID = null;
    private int currentSceneIndex = 0;
    private int activeSceneIndex = 0;

    private void Awake()
    {
        if (backToItemsButton != null)
            backToItemsButton.onClick.AddListener(() => BuildItems());

        if (prevSceneButton != null)
            prevSceneButton.onClick.AddListener(() => ChangeScenePage(-1));
        if (nextSceneButton != null)
            nextSceneButton.onClick.AddListener(() => ChangeScenePage(1));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (state == PanelState.Items) Close();
            else BuildItems();
        }
    }

    public void Open(PlayerItemHandler h)
    {
        handler = h;
        activeSceneIndex = ItemInfoManager.Instance.GetSceneIndex(SceneManager.GetActiveScene().name);
        currentSceneIndex = activeSceneIndex;
        gameObject.SetActive(true);
        BuildItems();
        backToItemsButton?.gameObject.SetActive(false);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        handler = null;
        itemSnapshot.Clear();
        spawnSnapshot.Clear();
        currentItemID = null;
        state = PanelState.Items;
    }

    private void BuildItems()
    {
        backToItemsButton?.gameObject.SetActive(false);
        state = PanelState.Items;
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
                    Debug.Log("해당되는 씬 아님");
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
        backToItemsButton?.gameObject.SetActive(true);
        state = PanelState.Spawns;
        Clear(contentRoot);
        spawnSnapshot.Clear();

        if (handler == null) return;
        var spawns = handler.GetSpawnsOf(handler.GetHoldItemID(), currentSceneIndex);
        spawnSnapshot.AddRange(spawns);

        for (int i = 0; i < spawnSnapshot.Count; i++)
        {
            int captured = i;
            var btn = Instantiate(buttonPrefab, contentRoot);
            btn.GetComponentInChildren<TMP_Text>().text = spawnSnapshot[i].mapName;
            btn.interactable = currentSceneIndex == activeSceneIndex;

            btn.onClick.AddListener(() =>
            {
                if (!btn.interactable) return;
                handler.Teleport(spawnSnapshot[captured]);
                Close();
            });
        }

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
