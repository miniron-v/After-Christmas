using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    // 혼자 독립적으로 대화만 하는 오브젝트인지 체크
    private bool isAlone = true;
    [SerializeField] private List<DialogueData> dialogueSequence;

    public bool HasDialogue() => dialogueSequence != null && dialogueSequence.Count > 0;

    private void Awake()
    {
        // 같은 오브젝트에 자기 자신을 제외한 IInteractable이 존재하면 isAlone = false
        var interactables = GetComponents<IInteractable>();
        foreach (var comp in interactables)
        {
            if (comp != (IInteractable)this)
            {
                isAlone = false;
                return;
            }
        }

        isAlone = true;
    }

    // 기존 전체 시퀀스 재생용
    public void StartDialogueSequence(Action onEnd = null)
    {
        if (!HasDialogue())
        {
            onEnd?.Invoke();
            return;
        }

        PlayDialogueRecursive(0, onEnd);
    }

    // 특정 인덱스 하나만 재생
    public void StartDialogueAtIndex(int index, Action onEnd = null)
    {
        if (!HasDialogue() || index < 0 || index >= dialogueSequence.Count)
        {
            onEnd?.Invoke();
            return;
        }

        DialogueData data = dialogueSequence[index];
        DialogueManager.Instance.StartDialogue(data, onEnd);
    }

    private void PlayDialogueRecursive(int index, Action onEnd)
    {
        if (index >= dialogueSequence.Count)
        {
            onEnd?.Invoke();
            return;
        }

        DialogueData data = dialogueSequence[index];
        DialogueManager.Instance.StartDialogue(data, () =>
        {
            PlayDialogueRecursive(index + 1, onEnd);
        });
    }

    public void Interact()
    {
        if (!isAlone) return;
        StartDialogueSequence();
    }
}
