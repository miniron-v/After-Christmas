using System;
using UnityEngine;

public class Item : MonoBehaviour, ITeleportable
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


    // 현재 오브젝트가 맡고 있는 도착지
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform cameraSpawnPoint;

    public bool isInteracted = false;


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
    public void Teleport()
    {
        Debug.Log("interact");  
        // 플레이어 찾기 (Tag 이용)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null && playerSpawnPoint != null)
        {
            player.transform.position = playerSpawnPoint.position;
            player.transform.rotation = playerSpawnPoint.rotation;
        }

        // 카메라 위치 이동
        if (cameraSpawnPoint != null && Camera.main != null)
        {
            Camera.main.transform.position = cameraSpawnPoint.position;
            Camera.main.transform.rotation = cameraSpawnPoint.rotation;
        }
    }
}
