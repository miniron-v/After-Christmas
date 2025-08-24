using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;   // 버튼들이 들어갈 부모 (하나만 사용)
    [SerializeField] private Button buttonPrefab;     // TMP_Text 포함 버튼 프리팹

    private PlayerItemHandler handler;

    // 현재 화면 상태
    private enum PanelState { Items, Spawns }
    private PanelState state = PanelState.Items;

    // 스냅샷(인덱싱 안정용)
    private List<string> itemIdSnapshot = new();
    private List<SpawnTransform> spawnSnapshot = new();

    // 현재 선택된 아이템 ID
    private string currentItemID = null;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Open(PlayerItemHandler h)
    {
        handler = h;
        state = PanelState.Items;
        BuildItems();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        handler = null;
        itemIdSnapshot.Clear();
        spawnSnapshot.Clear();
        currentItemID = null;
        state = PanelState.Items;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 아이템 목록 상태에서 ESC면 닫기, 기억 목록 상태면 뒤로가기
            if (state == PanelState.Items) Close();
            else BuildItems();
        }
    }

    // ================== 화면 구성 ==================

    private void BuildItems()
    {
        state = PanelState.Items;
        Clear(contentRoot);
        itemIdSnapshot.Clear();
        spawnSnapshot.Clear();
        currentItemID = null;

        if (handler == null) return;

        var ids = handler.GetAllItemIDs();
        itemIdSnapshot.AddRange(ids);

        for (int i = 0; i < itemIdSnapshot.Count; i++)
        {
            var btn = Instantiate(buttonPrefab, contentRoot);
            var label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label) label.text = itemIdSnapshot[i];

            int captured = i;
            btn.onClick.AddListener(() =>
            {
                currentItemID = itemIdSnapshot[captured];
                BuildSpawns(currentItemID);
            });
        }
    }

    private void BuildSpawns(string itemID)
    {
        state = PanelState.Spawns;
        Clear(contentRoot);
        spawnSnapshot.Clear();

        if (handler == null) return;

        var spawns = handler.GetSpawnsOf(itemID);
        spawnSnapshot.AddRange(spawns);

        for (int i = 0; i < spawnSnapshot.Count; i++)
        {
            var btn = Instantiate(buttonPrefab, contentRoot);
            var label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label)
            {
                // [중요] 맵 이름 그대로 표시 (없으면 좌표 요약)
                string name = spawnSnapshot[i].mapName;
                label.text = string.IsNullOrEmpty(name)
                    ? $"P {Fmt(spawnSnapshot[i].playerSpawnPoint)} | C {Fmt(spawnSnapshot[i].cameraSpawnPoint)}"
                    : name;
            }

            int captured = i;
            btn.onClick.AddListener(() =>
            {
                handler.Teleport(spawnSnapshot[captured]);
                Close();
            });
        }
    }

    // ================== 유틸 ==================

    private static void Clear(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
    }

    private static string Fmt(Vector3 v) => $"{v.x:0.##},{v.y:0.##},{v.z:0.##}";
}
