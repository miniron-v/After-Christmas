using UnityEngine.Playables;
using UnityEngine;

[System.Serializable]
public class CutsceneEvent
{
    // 이벤트 타입 결정
    [Header("Event Type")]
    [Tooltip("체크 시, Dialogue 활성화, Timeline은 배경 움직임 적용.")]
    public bool hasDialogueContent = false;
    
    // 시네마틱 설정
    [Header("Cinematic Timeline")]
    [Tooltip("대화 있음: 대화 중 재생될 배경 애니메이션/움직임.\n- 대화 없음: 단독으로 재생될 전체 컷신 시퀀스.")]
    public PlayableDirector cinematicTimeline;

    [Tooltip("대화가 있는 경우, 반복 여부 결정")]
    public bool loopTimeline = false;

    // 대화 내용 설정
    [Header("Dialogue Content")]
    public DialogueData dialogueData;
}