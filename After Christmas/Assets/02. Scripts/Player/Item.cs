using System;
using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    [SerializeField] private String itemID;
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
    [SerializeField] private Item linkedItem;


    // 현재 오브젝트가 맡고 있는 도착지(종단 아이템에는 필요없는 변수)
    public Transform playerSpawnPoint;
    public Transform cameraSpawnPoint;

    // 기록되었는지 확인하는 변수
    public bool isRecorded = false;
    // 텔레포트(연결된 물체가 있는지)여부 확인하는 변수
    public bool isTeleportItem = false;

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
        Teleport(player);
    }

    public void RecordItem(GameObject player)
    {
        // 상호작용한 적이 없다면 최초 1회에 정보 저장(반대편 아이템 정보까지)
        // 만약 사이클이 있어서
        // 상호작용됨 - 상호작용됨 - 상호작용 안됨 상태에서도 hashset을 사용하기 때문에 좌표 중복저장 방지됨
        isRecorded = true;
        linkedItem.isRecorded = true;
        PlayerItemHandler playerItemHandler = player.GetComponent<PlayerItemHandler>();
        // 만약 아이템 목록에 아예 없다면 새 공간 할당
        if (!playerItemHandler.isHavingItem(itemID))
        {
            playerItemHandler.GetNewItem(itemID);
        }
        // 현재 상호작용한 아이템 좌표정보와 상대 아이템 좌표정보 전부 기록
        playerItemHandler.RecordItemInfo(itemID, playerSpawnPoint, cameraSpawnPoint);
        playerItemHandler.RecordItemInfo(itemID, linkedItem.playerSpawnPoint, linkedItem.cameraSpawnPoint);
    }

    public void Teleport(GameObject player)
    {
        // 연결된 물체로 상대 이동
        if (player != null && playerSpawnPoint != null)
        {
            player.transform.position = linkedItem.playerSpawnPoint.position;
            player.transform.rotation = linkedItem.playerSpawnPoint.rotation;
        }

        // 카메라 위치 이동
        if (cameraSpawnPoint != null && Camera.main != null)
        {
            Camera.main.transform.position = linkedItem.cameraSpawnPoint.position;
            Camera.main.transform.rotation = linkedItem.cameraSpawnPoint.rotation;
        }
    }
}
