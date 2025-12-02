using System;
using System.Collections;
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

public class Item : MonoBehaviour, IInteractable, IGlowable
{
    [Header("SO Item Data")]
    [SerializeField] private ItemDataSO itemData;

    // 아이템 정보, itemData에서 중앙 관리하므로 hide in inspector
    [HideInInspector]
    public string itemID => itemData != null ? itemData.itemID : "Unknown";
    [HideInInspector]
    public Sprite itemIcon => itemData != null ? itemData.icon : null;
    [HideInInspector]
    public string itemDescription => itemData != null ? itemData.description : "No description";
    // 아이템이 있던 위치(하나의 기억)
    // map.cs에서 중앙 관리하므로 인스펙터에서 숨김
    [HideInInspector]
    public string mapName;
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

    // 현재 오브젝트가 맡고 있는 플레이어 스폰 좌표
    public Transform playerSpawnPoint;

    // 현재 맵의 카메라 스폰 좌표. 이것도 map.cs에서 중앙관리 가능하므로 인스펙터에서 숨김
    [HideInInspector]
    public Transform cameraSpawnPoint;

    [HideInInspector]
    // 반대 아이템으로 가는 경로가 열렸는지 체크하는 변수, 디폴트값 false
    public bool isRecorded = false;

    // 텔레포트(연결된 물체가 있는지)기능이 있는 아이템인지 확인, 디폴트값 true
    // isTeleportItem이 true라면 linkeditem이 1개는 있어야 함
    // isTeleportItem이 false라면 이 아이템은 종단 아이템
    [HideInInspector]
    public bool isTeleportItem = true;

    // 대화 로직 관련 변수
    private DialogueTrigger dialogueTrigger;
    private bool hasPlayedDialogue = false;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;

        // 인스펙터 수정을 최소화하기 위한 로직
        // 링크된 아이템이 없다 -> 텔레포트용 X, 종단 아이템
        if (linkedItems.Count == 0)
        {
            isTeleportItem = false;
        }

        dialogueTrigger = GetComponent<DialogueTrigger>();
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
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (!hasPlayedDialogue && dialogueTrigger != null && dialogueTrigger.HasDialogue())
        {
            hasPlayedDialogue = true;
            dialogueTrigger.StartDialogueSequence(() => PerformInteraction(player));
        }
        else
        {
            PerformInteraction(player);
        }
    }

    private void PerformInteraction(GameObject player)
    {
        if (isTeleportItem)
        {
            ShowTeleportUI(player);
        }
        else
        {
            RecordItem(player, -1);
        }
    }

    private void RecordItem(GameObject player, int linkedItemIDX = -1)
    {
        PlayerItemHandler itemHandler = player.GetComponent<PlayerItemHandler>();
        itemHandler.RecordFromItem(this, linkedItemIDX);
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
        // 수정
        ui.Open(transform.position + Vector3.up * 3f, candidateLabels, selectedIdx =>
        {
            Item target = candidateItems[selectedIdx];
            int idx = linkedItems.IndexOf(target);
            RecordItem(player, idx >= 0 ? idx : -1);
            TeleportByItem(player, target);
        });
    }

    private void TeleportByItem(GameObject player, Item target)
    {
        StartCoroutine(TeleportByItemRoutine(player, target));
    }

    private IEnumerator TeleportByItemRoutine(GameObject player, Item target)
    {
        // 페이드 아웃
        bool isFadeOutComplete = false;
        FadeManager.Instance.FadeOut(() => { isFadeOutComplete = true; });

        // 페이드 아웃 끝날 때까지 대기
        yield return new WaitUntil(() => isFadeOutComplete);

        // 0.5초 딜레이
        yield return new WaitForSeconds(0.5f);

        // 위치 이동
        player.transform.SetPositionAndRotation(
            target.playerSpawnPoint.position,
            target.playerSpawnPoint.rotation
        );

        if (Camera.main != null)
            Camera.main.transform.SetPositionAndRotation(
                target.cameraSpawnPoint.position,
                target.cameraSpawnPoint.rotation
            );

        TeleportEventManager.NotifyTeleport();

        MapInfoManager.Instance.VisitMap(target.mapName);
        target.isRecorded = true;

        // 페이드 인
        FadeManager.Instance.FadeIn();
    }

}
