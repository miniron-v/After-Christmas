using System;
using UnityEngine;

public class LockItem : MonoBehaviour, IInteractable, IGlowable
{
    [Header("필요 아이템")]
    [SerializeField] private Item needItem;

    [Header("잠금 해제 시 생성될 아이템")]
    [SerializeField] private GameObject unLockedItem;

    private Renderer rend;
    private Color originalColor;

    [SerializeField] private Color glowColor = Color.red;

    private DialogueTrigger dialogueTrigger;

    private bool firstInteraction = true;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;

        dialogueTrigger = GetComponent<DialogueTrigger>();
    }

    public void Glow(bool detected)
    {
        if (rend != null)
            rend.material.color = detected ? glowColor : originalColor;
    }

    public void Interact()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (dialogueTrigger == null) return;

        // 1. 최초 상호작용 → A 대화 (index 0)
        if (firstInteraction)
        {
            firstInteraction = false;
            dialogueTrigger.StartDialogueAtIndex(0);
            return;
        }

        // 2. 이후 → 잠금 해제 가능 여부 판단
        if (!IsUnLockable(player))
        {
            // B 대화 (index 1)
            dialogueTrigger.StartDialogueAtIndex(1);
            return;
        }

        // 3. 대화 (index 2) → 끝나면 UnLockItem 실행
        dialogueTrigger.StartDialogueAtIndex(2, () =>
        {
            UnLockItem();
        });
    }

    private bool IsUnLockable(GameObject player)
    {
        return needItem.itemID == player.GetComponent<PlayerItemHandler>().GetHoldItemID();
    }

    private void UnLockItem()
    {
        Debug.Log("Unlocked!");

        if (unLockedItem != null)
        {
            unLockedItem.SetActive(true);
        }

        gameObject.SetActive(false);
    }
}
