using System;
using System.Collections.Generic;
using UnityEngine;

public struct SpawnTransform
{
    public Vector3 playerSpawnPoint;
    public Vector3 cameraSpawnPoint;
    public string mapName;

    public SpawnTransform(Vector3 player, Vector3 camera, string mapName)
    {
        this.mapName = mapName;
        playerSpawnPoint = player;
        cameraSpawnPoint = camera;
    }
}

public class Item : MonoBehaviour, IInteractable
{
    // 아이템이 있던 위치(하나의 기억)
    public string mapName;
    // 아이템의 이름(식별자)
    public string itemID;
    private Renderer rend;
    private Color originalColor;

    [SerializeField] private Color glowColor = Color.yellow;
    [SerializeField] private float glowDuration = 0.2f;

    //========로직=========
    // 아이템은 텔레포트시킬 위치가 아니라, 텔레포트될 상대 오브젝트(링크 오브젝트)자체를 가지고 있음
    // 거기에서 좌표를 가져오는 걸로
    // + 자신이 맡고 있는 좌표 또한 기억해야 함
    //====================

    // 반대편 오브젝트
    // 빈칸일 수도 있고, 여러개일 수도 있음
    public List<Item> linkedItems = new List<Item>();

    [SerializeField] private GameObject teleportUI;

    // 현재 오브젝트가 맡고 있는 좌표
    public Transform playerSpawnPoint;
    public Transform cameraSpawnPoint;

    // 반대 아이템으로 가는 경로가 열렸는지 체크하는 변수, 디폴트값 false
    public bool isRecorded = false;

    // 물건을 가지고 와서 잡은 상태로 상호작용하면 특수 상호작용할 수 있는지 체크하는 변수, 디폴트값 false
    [SerializeField] private bool isInteractableWithItem = false;
    [SerializeField] private string needItemID;
    public static event Action interactWithItem;

    // 텔레포트(연결된 물체가 있는지)기능이 있는 아이템인지 확인하는 변수, 디폴트값 true
    // isTeleportItem이 true라면 linkeditem이 1개는 있어야 함
    // isTeleportItem이 false라면 이 아이템은 종단 아이템
    public bool isTeleportItem = true;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;
    }

    public void Glow(bool detected)
    {
        Debug.Log("Sucessed Glow");
        if (rend != null)
        {
            rend.material.color = detected ? glowColor : originalColor;
        }
    }

    public void Interact()
    {
        Debug.Log("interact");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // 아이템을 든 상태로 상호작용 가능한지 먼저 체크
        if (isInteractableWithItem && IsSpecialInteractable(player))
        {
            SpecialInteraction();
            return;
        }

        // 자기 자신 기록 (Interact 시점)
        isRecorded = true;

        // 텔레포트 가능(반대 아이템 있음)
        if (isTeleportItem)
        {
            ShowTeleportUI(player);
        }
        // 아닐 경우 정보 저장만
        else
        {
            RecordItem(player, -1);
        }
    }

    private void RecordItem(GameObject player, int linkedItemIDX = -1)
    {
        PlayerItemHandler itemHandler = player.GetComponent<PlayerItemHandler>();
        itemHandler.RecordFromItem(this, linkedItemIDX);
        isRecorded = true;
    }

    private void ShowTeleportUI(GameObject player)
    {
        List<Item> candidateItems = new List<Item>();
        List<string> candidateLabels = new List<string>();

        // BFS 후보 수집 (isRecorded된 아이템만)
        HashSet<Item> visited = new HashSet<Item>();
        Queue<Item> q = new Queue<Item>();

        visited.Add(this);
        q.Enqueue(this);

        while (q.Count > 0)
        {
            Item current = q.Dequeue();
            foreach (var neighbor in current.linkedItems)
            {
                if (neighbor != null && !visited.Contains(neighbor) && neighbor.isRecorded)
                {
                    visited.Add(neighbor);
                    q.Enqueue(neighbor);
                }
            }
        }

        // 자기 자신 제외 후보
        foreach (var item in visited)
        {
            if (item != this)
            {
                candidateItems.Add(item);
                candidateLabels.Add(item.mapName);
            }
        }

        // 후보가 없으면 linkedItems 중 첫 번째로 이동
        if (candidateItems.Count == 0 && linkedItems.Count > 0)
        {
            Item target = linkedItems[0];
            RecordItem(player, 0);
            TeleportByItem(player, target);
            return;
        }

        // 후보가 1개 → 바로 이동
        if (candidateItems.Count == 1)
        {
            Item target = candidateItems[0];
            int idx = linkedItems.IndexOf(target);
            RecordItem(player, idx >= 0 ? idx : -1);
            TeleportByItem(player, target);
            return;
        }

        // 후보가 2개 이상 → UI
        teleportUI.SetActive(true);
        TeleportSelectionUI ui = teleportUI.GetComponent<TeleportSelectionUI>();
        ui.Open(candidateLabels, selectedIdx =>
        {
            Item target = candidateItems[selectedIdx];
            int idx = linkedItems.IndexOf(target);
            RecordItem(player, idx >= 0 ? idx : -1);
            TeleportByItem(player, target);
        });
    }

    private void TeleportByItem(GameObject player, Item target)
    {
        player.transform.SetPositionAndRotation(
            target.playerSpawnPoint.position,
            target.playerSpawnPoint.rotation
        );

        if (Camera.main != null)
            Camera.main.transform.SetPositionAndRotation(
                target.cameraSpawnPoint.position,
                target.cameraSpawnPoint.rotation
            );

        target.isRecorded = true;
    }

    private bool IsSpecialInteractable(GameObject player)
    {
        string holdItemID = player.GetComponent<PlayerItemHandler>().GetHoldItemID();
        return holdItemID == needItemID;
    }

    private void SpecialInteraction()
    {
        interactWithItem?.Invoke();
        isInteractableWithItem = false;
    }
}
