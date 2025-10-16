using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;

public class SequenceController : MonoBehaviour
{

    public List<CutsceneEvent> cutsceneSequence; // 시퀀스 목록
    private int currentEventIndex = -1;

    private PlayableDirector activeBackgroundTimeline; // 대화 중 재생할 Timeline

    void Start()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.onDialogueEnd += HandleDialogueFinished;
        }
    }

    void Update()
    {
        // 임시 시작 C
        if (Input.GetKeyDown(KeyCode.C))
        {
            // 실행 중이 아닐 때만 실행
            if (currentEventIndex == -1)
            {
                StartCutscene();
            }
        }
    }

    void OnDisable()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.onDialogueEnd -= HandleDialogueFinished;
        }
    }

    public void StartCutscene()
    {
        currentEventIndex = 0;
        ProcessCurrentEvent();
    }

    public void StopCutscene()
    {
        if (activeBackgroundTimeline != null)
        {
            activeBackgroundTimeline.Stop();
            activeBackgroundTimeline.extrapolationMode = DirectorWrapMode.Hold;
            activeBackgroundTimeline = null;
        }
        if (DialogueManager.Instance.IsDialogueActive()) DialogueManager.Instance.EndDialogue();
        currentEventIndex = -1;
    }

    // 시퀀스 처리 로직
    private void ProcessCurrentEvent()
    {
        if (currentEventIndex >= cutsceneSequence.Count)
        {
            Debug.Log("시퀀스 종료");
            currentEventIndex = -1;
            return;
        }

        CutsceneEvent currentEvent = cutsceneSequence[currentEventIndex];

        if (currentEvent.isDialogueEvent)
        {
            HandleDialogueEvent(currentEvent);
        }
        else
        {
            HandleTimelineEvent(currentEvent);
        }
    }

    // 단순 Timeline 이벤트 처리
    private void HandleTimelineEvent(CutsceneEvent currentEvent)
    {
        if (currentEvent.timelineToPlay == null)
        {
            AdvanceSequence();
            return;
        }

        Debug.Log($"Timeline 재생 시작: {currentEvent.timelineToPlay.name}");

        // 다음 이벤트 실행
        currentEvent.timelineToPlay.stopped += OnTimelineFinished;

        currentEvent.timelineToPlay.Play();
    }

    // 대화 이벤트 처리
    private void HandleDialogueEvent(CutsceneEvent currentEvent)
    {
        if (currentEvent.dialogueData == null)
        {
            AdvanceSequence();
            return;
        }

        // 이전에 재생 중인 Timeline 정지
        if (activeBackgroundTimeline != null)
        {
            activeBackgroundTimeline.Stop();
            activeBackgroundTimeline.extrapolationMode = DirectorWrapMode.Hold;
            activeBackgroundTimeline = null;
        }

        // 배경 Timeline 설정 및 재생
        activeBackgroundTimeline = currentEvent.timelineWhileDialogue;
        if (activeBackgroundTimeline != null)
        {
            Debug.Log($"Dialogue 중 백그라운드 Timeline 재생: {activeBackgroundTimeline.name}");

            // 반복 설정
            if (currentEvent.loopBackgroundTimeline)
            {
                activeBackgroundTimeline.extrapolationMode = DirectorWrapMode.Loop;
            }
            else
            {
                activeBackgroundTimeline.extrapolationMode = DirectorWrapMode.Hold;
            }
            activeBackgroundTimeline.Play();
        }

        // 대화 시작
        DialogueManager.Instance.StartDialogue(currentEvent.dialogueData);
    }

    // 콜백 함수
    private void OnTimelineFinished(PlayableDirector director)
    {
        // 이벤트 중복 호출 방지
        director.stopped -= OnTimelineFinished;

        Debug.Log($"Timeline 종료: {director.name}");
        AdvanceSequence();
    }

    // Dialogue 종료 시 호출
    private void HandleDialogueFinished()
    {
        // 대화 종료 시 백그라운드 Timeline 자동 종료
        if (activeBackgroundTimeline != null)
        {
            Debug.Log($"Dialogue 종료: 백그라운드 Timeline({activeBackgroundTimeline.name}) 정지");

            activeBackgroundTimeline.Stop();
            activeBackgroundTimeline.RebuildGraph();

            activeBackgroundTimeline.extrapolationMode = DirectorWrapMode.Hold;
            activeBackgroundTimeline = null;
        }

        AdvanceSequence();
    }

    // 다음 시퀀스로 이동
    private void AdvanceSequence()
    {
        currentEventIndex++;
        ProcessCurrentEvent();
    }
}