using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    // 혼자 독립적으로 대화만 하는 오브젝트인지 체크
    private bool isAlone = true;
    [SerializeField] private List<DialogueData> dialogueSequence;

    private Coroutine runningSequence = null;

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

        if (runningSequence == null)
        {
            runningSequence = StartCoroutine(RunDialogueSequence(onEnd));
        }
    }

    // 전체 대화 목록을 순서대로 재생
    private IEnumerator RunDialogueSequence(Action onEnd)
    {
        foreach (DialogueData data in dialogueSequence)
        {
            if (DialogueManager.Instance != null && data != null)
            {
                yield return StartCoroutine(DialogueManager.Instance.StartDialogue(
                    data,
                    isCutscene: false));
            }
        }

        onEnd?.Invoke();
        runningSequence = null;
    }

    // 특정 인덱스 하나만 재생
    public void StartDialogueAtIndex(int index, Action onEnd = null)
    {
        if (!HasDialogue() || index < 0 || index >= dialogueSequence.Count)
        {
            onEnd?.Invoke();
            return;
        }

        if (runningSequence == null)
        {
            // 단일 대화 실행 코루틴을 시작
            runningSequence = StartCoroutine(RunSingleDialogue(dialogueSequence[index], onEnd));
        }
        else
        {
            // 이미 실행 중일 경우, 외부 콜백(onEnd)만 호출하고 종료할 수 있습니다.
            onEnd?.Invoke();
        }
    }

    private IEnumerator RunSingleDialogue(DialogueData data, Action onEnd)
    {
        if (DialogueManager.Instance != null && data != null)
        {
            yield return StartCoroutine(DialogueManager.Instance.StartDialogue(data, isCutscene: false));
        }

        onEnd?.Invoke();
        runningSequence = null;
    }

    public void Interact(GameObject player)
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
        {
            return;
        }

        if (!isAlone) return;

        StartDialogueSequence();
    }
}
