using UnityEngine.Playables;
using UnityEngine;

[System.Serializable]
public class CutsceneEvent
{
    // Dialogue(true), Timeline(false)
    public bool isDialogueEvent = false;

    [Header("Timeline Settings")]
    public PlayableDirector timelineToPlay;

    [Header("Dialogue Settings")]
    public DialogueData dialogueData;

    public PlayableDirector timelineWhileDialogue; // 대화가 진행되는 동안 재생할 Timeline

    public bool loopBackgroundTimeline = false; // 배경 컷신 반복 여부
}