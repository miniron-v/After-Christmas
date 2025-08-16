using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    private Renderer rend;
    private Color originalColor;

    [SerializeField] private Color glowColor = Color.yellow;
    [SerializeField] private float glowDuration = 0.2f;


    [SerializeField] private Transform playerTarget;
    [SerializeField] private Transform cameraTarget;


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
        if (player != null && playerTarget != null)
        {
            player.transform.position = playerTarget.position;
            player.transform.rotation = playerTarget.rotation;
        }

        // 카메라 위치 이동
        if (cameraTarget != null && Camera.main != null)
        {
            Camera.main.transform.position = cameraTarget.position;
            Camera.main.transform.rotation = cameraTarget.rotation;
        }
    }
}
