using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
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
    // 빈칸일 수도 있음
    public Item linkedItem = null;


    // 현재 오브젝트가 맡고 있는 도착지(종단 아이템에는 필요없는 변수)
    public Transform playerSpawnPoint;
    public Transform cameraSpawnPoint;

    // 기록되었는지 확인하는 변수, 디폴트값 false
    public bool isRecorded = false;
    // 텔레포트(연결된 물체가 있는지)기능이 있는 아이템인지 확인하는 변수, 디폴트값 true
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

        if (!isRecorded)
        {
            RecordItem(player);
        }

        if (isTeleportItem)
        {
            Teleport(player);
        }
    }

    public void RecordItem(GameObject player)
    {
        // 정보 저장은 핸들러가 하는게 자연스러워 보임
        PlayerItemHandler itemHandler = player.GetComponent<PlayerItemHandler>();
        itemHandler.RecordFromItem(this);
        // 저장된 아이템인지만 갱신
        isRecorded = true;
        if (isTeleportItem)
        {
            linkedItem.isRecorded = true;
        }
    }

    public void Teleport(GameObject player)
    {
        // 연결된 물체로 상대 이동
        if (player != null && linkedItem.playerSpawnPoint != null)
        {
            Debug.Log("플레이어");
            player.transform.position = linkedItem.playerSpawnPoint.position;
            player.transform.rotation = linkedItem.playerSpawnPoint.rotation;
        }

        // 카메라 위치 이동
        if (linkedItem.cameraSpawnPoint != null && Camera.main != null)
        {
            Camera.main.transform.position = linkedItem.cameraSpawnPoint.position;
            Camera.main.transform.rotation = linkedItem.cameraSpawnPoint.rotation;
        }
    }
}
