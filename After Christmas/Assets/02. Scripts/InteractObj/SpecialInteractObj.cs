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

    private DialogueTrigger dialogueTrigger;

    // 최초 상호작용인지가 아닌, 특수 상호작용이 완료되었는지 확인하는 변수(한개로 여러번 상호작용 방지)
    private bool isSpecialInteracted = false;

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
        if (dialogueTrigger == null) return;

        // 1. 최초 상호작용 → A 대화 (index 0)
        if (firstInteraction)
        {
            firstInteraction = false;
            dialogueTrigger.StartDialogueAtIndex(0);
            return;
        }

        // 2. 이후 → 특수 상호작용 가능 여부 판단
        if (!IsSpecialInteractable(player))
        {
            // B 대화 (index 1)
            dialogueTrigger.StartDialogueAtIndex(1);
            return;
        }

        // 3. 대화 (index 2) → 끝나면 SpecialInteraction 실행
        // 이 부분 애매한게, 대화 -> 균열 메우기일지 / 균열 메우기 -> 대화 일지 순서 모호
        // 플레이상 자연스러우려면 균열을 메우고 대화 진행일 텐데, 그러면 콜백 안써도 될듯
        if (isSpecialInteracted)
        {
            dialogueTrigger.StartDialogueAtIndex(2, () =>
        {
            SpecialInteraction();
        });
        }

    }

    private bool IsSpecialInteractable(GameObject player)
    {
        return needItem.itemID == player.GetComponent<PlayerItemHandler>().GetHoldItemID();
    }

    private void SpecialInteraction()
    {
        isSpecialInteracted = true;
        interactWithItem?.Invoke();
    }
}
