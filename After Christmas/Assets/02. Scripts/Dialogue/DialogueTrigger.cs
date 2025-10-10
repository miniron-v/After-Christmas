// DialogueTrigger.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private List<DialogueData> dialogueSequence;
    private int currentIndex = 0;
    private bool hasPlayedDialogue = false;

    public bool HasDialogue() => dialogueSequence != null && dialogueSequence.Count > 0;

    // 대화 시퀀스 시작, 끝나면 onEnd 콜백 호출
    public void StartDialogueSequence(Action onEnd = null)
    {
        if (!HasDialogue() || hasPlayedDialogue)
        {
            onEnd?.Invoke();
            return;
        }

        hasPlayedDialogue = true;

        if (currentIndex < dialogueSequence.Count)
        {
            DialogueData data = dialogueSequence[currentIndex];
            currentIndex++;
            DialogueManager.Instance.StartDialogue(data, () =>
            {
                StartDialogueSequence(onEnd); // 다음 대화 또는 끝나면 onEnd 호출
            });
        }
        else
        {
            onEnd?.Invoke();
        }
    }

    public void Interact()
    {
        StartDialogueSequence();
    }
}
