using UnityEngine;
using System;

public class SpecialInteractObj : MonoBehaviour, IInteractable, IGlowable
{
    [SerializeField] private Item needItem;
    public static event Action interactWithItem;

    // 기존 아이템 로직에도 있던 glow 로직 가져옴
    private Renderer rend;
    private Color originalColor;
    [SerializeField] private Color glowColor = Color.yellow;
    [SerializeField] private float glowDuration = 0.2f;

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
        Debug.Log("특수공간 상호작용");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (IsSpecialInteractable(player))
        {
            SpecialInteraction();
        }
        else
        {
            // 대화 넣기(ex. 잘못된 아이템인 것 같다 등)
        }
    }

    private bool IsSpecialInteractable(GameObject player)
    {
        return needItem.itemID == player.GetComponent<PlayerItemHandler>().GetHoldItemID();
    }

    private void SpecialInteraction()
    {
        interactWithItem?.Invoke();
    }
}
