using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public struct SpawnTransform
{
    public Vector3 playerSpawnPoint;
    public Vector3 cameraSpawnPoint;
    public String mapName;

    public SpawnTransform(Vector3 player, Vector3 camera, String mapName)
    {
        this.mapName = mapName;
        playerSpawnPoint = player;
        cameraSpawnPoint = camera;
    }
}

public class Item : MonoBehaviour, IInteractable
{
    public String mapName;
    public String itemID;
    private Renderer rend;
    private Color originalColor;

    [SerializeField] private Color glowColor = Color.yellow;
    [SerializeField] private float glowDuration = 0.2f;

    //===================
    // 로직 변경
    // 아이템은 텔레포트시킬 위치가 아니라, 텔레포트될 상대 오브젝트 자체를 가지고 있음
    // 거기에서 좌표를 가져오는 걸로
    // + 자신이 맡고 있는 좌표 또한 기억해야 함
    //===================

    // 반대편 오브젝트
    // 빈칸일 수도 있고, 여러개일 수도 있음
    public List<Item> linkedItems = new List<Item>();

    [SerializeField] private GameObject teleportUI;


    // 현재 오브젝트가 맡고 있는 도착지(종단 아이템에는 필요없는 변수)
    public Transform playerSpawnPoint;
    public Transform cameraSpawnPoint;

    // 반대 아이템으로 가는 경로가 열렸는지 체크하는 변수
    public bool isRecorded;

    // 텔레포트(연결된 물체가 있는지)기능이 있는 아이템인지 확인하는 변수, 디폴트값 true
    // isteleportitem이 true라면 linkeditem이 1개는 있어야 함
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
            if (detected)
            {
                rend.material.color = glowColor;
            }
            else
            {
                rend.material.color = originalColor;
            }
        }
    }
    public void Interact()
    {
        Debug.Log("interact");
        // 플레이어 찾기 (Tag 이용)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return;
        }
        if (isTeleportItem)
        {
            ShowUI(player);
        }
    }

    public void RecordItem(GameObject player, int linkedItemIDX)
    {
        // 정보 저장은 핸들러가 하는게 자연스러워 보임
        PlayerItemHandler itemHandler = player.GetComponent<PlayerItemHandler>();
        itemHandler.RecordFromItem(this, linkedItemIDX);
        isRecorded = true;
    }

    private void ShowUI(GameObject player)
    {
        teleportUI.gameObject.SetActive(true);
        // linkedItems의 길이가 2 이상이라면 isRecorded가 true 인 것들만
        // 길이가 1이라면 그것만
        var candidateIndices = new List<int>();
        var candidateLabels = new List<string>();

        if (linkedItems.Count == 1)
        {
            Debug.Log("여기1");
            candidateIndices.Add(0);
            // 오브젝트 이름이 아니라, 오브젝트가 있는 기억(맵)의 이름을 전달해줘야 할 것 같음
            candidateLabels.Add(linkedItems[0].mapName);
        }
        else
        {
            for (int i = 0; i < linkedItems.Count; i++)
            {
                Item dst = linkedItems[i];
                // 방문해 본 목적지만 후보
                if (dst.isRecorded)
                {
                    candidateIndices.Add(i);
                    candidateLabels.Add(dst.mapName);
                }
            }
        }

        // 컴포넌트 참조해서 Open에 '데이터와 콜백'을 전달
        TeleportSelectionUI ui = teleportUI.GetComponent<TeleportSelectionUI>();
        ui.Open(
            candidateLabels,
            onSelectIndex: selectedIdxInCandidates =>
            {
                int originalIndex = candidateIndices[selectedIdxInCandidates];

                // 저장 → 텔레포트
                RecordItem(player, originalIndex);
                TeleportByIndex(player, originalIndex);
            }
        );
    }

    // 인덱스로 텔레포트 (UI에서 호출)
    public void TeleportByIndex(GameObject player, int index)
    {
        var target = linkedItems[index];

        // 플레이어 이동
        player.transform.SetPositionAndRotation(
            target.playerSpawnPoint.position,
            target.playerSpawnPoint.rotation
        );

        // 카메라 이동
        if (Camera.main != null)
        {
            Camera.main.transform.SetPositionAndRotation(
                target.cameraSpawnPoint.position,
                target.cameraSpawnPoint.rotation
            );
        }

        // 도착지 방문 기록 (다음부터 이 경로가 후보로 보임)
        target.isRecorded = true;
    }
}
