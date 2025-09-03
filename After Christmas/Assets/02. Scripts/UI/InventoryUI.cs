using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;   // 버튼들이 들어갈 부모 (하나만 사용)
    [SerializeField] private Button buttonPrefab;     // TMP_Text 포함 버튼 프리팹

    [Header("Context Menu")]
    [SerializeField] private Canvas rootCanvas;               // 최상위 Canvas
    //[SerializeField] private RectTransform contextMenu;       // 컨텍스트 메뉴 패널 (Pivot (0,1))
    //[SerializeField] private Button holdButton;               // "잡기"
    //[SerializeField] private Button readButton;               // "기억 데이터 읽기"

    // 추가: '기억 목록으로 돌아가기' 버튼
    [SerializeField] private Button backToItemsButton;
    //[SerializeField] private GameObject itemsPanel; // 아이템 목록 패널
    //[SerializeField] private GameObject spawnsPanel; // 기억 목록 패널

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

        // ===== 컨텍스트 메뉴 초기 설정 및 버튼 리스너 =====
        /*if (contextMenu) contextMenu.gameObject.SetActive(false);
        if (contextMenu) contextMenu.pivot = new Vector2(0f, 0f); // 마우스 기준으로 어디에 생성될지 정함. 기본값은 우측상단

        if (holdButton)
            holdButton.onClick.AddListener(() =>
            {
                handler.HoldItem(currentItemID);      // ← 잡기 수행
                Close();
            });

        if (readButton)
            readButton.onClick.AddListener(() =>
            {
                contextMenu.gameObject.SetActive(false);
                BuildSpawns(currentItemID);           // ← 기억 데이터 읽기
            });*/
        // =========================================================
        if (backToItemsButton)
        {
            backToItemsButton.onClick.AddListener(() =>
            {
                Close(); // ← 취소 시에 바로 모든 UI 끄기
                //BuildItems(); ← 이걸로 하면 아이템 선택창으로 돌아감
            });
        }
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
        backToItemsButton.gameObject.SetActive(false);
        state = PanelState.Items;
        Clear(contentRoot);
        itemIdSnapshot.Clear();
        spawnSnapshot.Clear();
        currentItemID = null;

        // 패널 전환
        //if (itemsPanel) itemsPanel.SetActive(true);
        //if (spawnsPanel) spawnsPanel.SetActive(false);

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
                if (IsUseableItemInCurrentScene(currentItemID))
                {
                    // 아이템을 잡기
                    handler.HoldItem(currentItemID);
                    // 잡은 아이템의 기억 목록으로 이동
                    BuildSpawns();
                }
                else
                {
                    ShowItemNotUsableMessage();
                }
            });
        }
    }

    // ================== 스폰 목록 구성 ==================

    private void BuildSpawns()
    {
        backToItemsButton.gameObject.SetActive(true);
        state = PanelState.Spawns;
        Clear(contentRoot);
        spawnSnapshot.Clear();

        // 패널 전환
        //if (itemsPanel) itemsPanel.SetActive(false);
        //if (spawnsPanel) spawnsPanel.SetActive(true);

        if (handler == null) return;

        string holdItemID = handler.GetHoldItemID();
        if (string.IsNullOrEmpty(holdItemID))
        {
            // 잡고 있는 아이템이 없으면 다시 아이템 목록으로 돌아감
            BuildItems();
            return;
        }

        var spawns = handler.GetSpawnsOf(holdItemID);
        spawnSnapshot.AddRange(spawns);

        for (int i = 0; i < spawnSnapshot.Count; i++)
        {
            var btn = Instantiate(buttonPrefab, contentRoot);
            var label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label)
            {
                string name = spawnSnapshot[i].mapName;
                label.text = name;
            }

            int captured = i;
            btn.onClick.AddListener(() =>
            {
                handler.Teleport(spawnSnapshot[captured]);
                Close();
            });
        }
    }

    // ===== 현재 씬에서 아이템을 사용할 수 있는지 확인 =====
    private bool IsUseableItemInCurrentScene(string itemID)
    {
        // 아이템이 현재 씬에서 사용 가능한지 확인
        return handler.isHavingItem(itemID);
    }

    // ===== 사용 불가능한 아이템에 대한 메시지 출력 =====
    private void ShowItemNotUsableMessage()
    {
        Debug.Log("이 아이템은 현재 씬에서 사용할 수 없습니다.");
        // 추가로 UI에 알림 메시지를 띄우는 코드 작성 가능
    }

    // ===== 컨텍스트 메뉴 띄우기 =====
    /*private void ShowItemContextMenuAtMouse()
    {
        if (contextMenu == null || rootCanvas == null) return;

        // 화면 좌표 → Canvas 좌표 변환
        Vector2 screen = Input.mousePosition;
        RectTransform canvasRT = rootCanvas.transform as RectTransform;

        Vector2 localPoint;
        Camera cam = null;
        if (rootCanvas.renderMode == RenderMode.ScreenSpaceCamera ||
            rootCanvas.renderMode == RenderMode.WorldSpace)
        {
            cam = rootCanvas.worldCamera;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screen, cam, out localPoint))
        {
            contextMenu.anchoredPosition = localPoint;
            contextMenu.gameObject.SetActive(true);
        }
    }*/

    // =================================================

    public void Open(PlayerItemHandler h)
    {
        // 기존 모달이 있으면 자동으로 닫히고 소유권 점유
        UIModalGate.Acquire(this, Close);

        handler = h;
        state = PanelState.Items;
        BuildItems();
        gameObject.SetActive(true);

        if (backToItemsButton) backToItemsButton.gameObject.SetActive(false);
    }

    public void Close()
    {
        UIModalGate.Release(this);
        gameObject.SetActive(false);
        handler = null;
        itemIdSnapshot.Clear();
        spawnSnapshot.Clear();
        currentItemID = null;
        state = PanelState.Items;

        if (backToItemsButton) backToItemsButton.gameObject.SetActive(false);
    }

    private void Clear(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
    }

    private void OnDisable()
    {
        //방어코드
        UIModalGate.Release(this);
    }
}
