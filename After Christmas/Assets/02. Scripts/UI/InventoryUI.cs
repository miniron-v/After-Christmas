using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Button backToItemsButton;

    private PlayerItemHandler handler;
    private enum PanelState { Items, Spawns }
    private PanelState state = PanelState.Items;

    private List<string> itemSnapshot = new();
    private List<SpawnTransform> spawnSnapshot = new();
    private string currentItemID = null;

    private void Awake()
    {
        gameObject.SetActive(false);
        if (backToItemsButton != null)
        {
            backToItemsButton.onClick.AddListener(() => BuildItems());
        }
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
        BuildItems();
        gameObject.SetActive(true);
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

        var ids = handler.GetAllItemIDs();
        itemSnapshot.AddRange(ids);

        for (int i = 0; i < itemSnapshot.Count; i++)
        {
            int captured = i;
            var btn = Instantiate(buttonPrefab, contentRoot);
            btn.GetComponentInChildren<TMP_Text>().text = itemSnapshot[i];
            btn.onClick.AddListener(() =>
            {
                currentItemID = itemSnapshot[captured];
                if (handler.isHavingItem(currentItemID))
                {
                    handler.HoldItem(currentItemID);
                    BuildSpawns();
                }
                else
                {
                    Debug.Log("현재 씬에서 사용할 수 없습니다.");
                }
            });
        }
    }

    private void BuildSpawns()
    {
        backToItemsButton?.gameObject.SetActive(true);
        state = PanelState.Spawns;
        Clear(contentRoot);
        spawnSnapshot.Clear();

        if (handler == null) return;
        var spawns = handler.GetSpawnsOf(handler.GetHoldItemID());
        spawnSnapshot.AddRange(spawns);

        for (int i = 0; i < spawnSnapshot.Count; i++)
        {
            int captured = i;
            var btn = Instantiate(buttonPrefab, contentRoot);
            btn.GetComponentInChildren<TMP_Text>().text = spawnSnapshot[i].mapName;
            btn.onClick.AddListener(() =>
            {
                handler.Teleport(spawnSnapshot[captured]);
                Close();
            });
        }
    }

    private void Clear(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
    }
}
