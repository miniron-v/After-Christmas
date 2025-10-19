using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using System.Collections;
using static Unity.VisualScripting.FlowStateWidget;
using System;

public class CinematicController : MonoBehaviour
{

    public List<CutsceneEvent> cutsceneEventList;

    // 컷신 실행 상태 관리를 위한 코루틴
    private Coroutine runningCutscene = null;

    private PlayableDirector activeCinematicTimeline;

    // 임시 테스트용 코드 ( C로 실행 )
    #region
    void Update()
    {
        // 임시 시작 C
        if (Input.GetKeyDown(KeyCode.C))
        {
            // 실행 중이 아닐 때만 실행
            if (runningCutscene == null)
            {
                StartCutscene();
            }
        }
    }
    #endregion

    public void StartCutscene(Action onCutsceneEnd = null)
    {
        Debug.Log("시네마틱 시작됨");
        if (runningCutscene == null)
        {
            runningCutscene = StartCoroutine(RunCutsceneSequence(onCutsceneEnd));
        }
    }

    public void StopCutscene()
    {
        if (runningCutscene != null)
        {
            StopCoroutine(runningCutscene);
            runningCutscene = null;
        }

        if (activeCinematicTimeline != null)
        {
            activeCinematicTimeline.Stop();
            activeCinematicTimeline.extrapolationMode = DirectorWrapMode.Hold;
            activeCinematicTimeline = null;
        }

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
        {
            DialogueManager.Instance.EndDialogue();
        }

        Debug.Log("컷신 종료");
    }

    private IEnumerator RunCutsceneSequence(Action onCutsceneEnd)
    {
        Debug.Log("컷신 시작");

        foreach (CutsceneEvent currentEvent in cutsceneEventList)
        {
            // Dialogue가 있는 이벤트인지 확인
            if (currentEvent.hasDialogueContent)
            {
                yield return StartCoroutine(RunDialogueEvent(currentEvent));
            }
            else
            {
                yield return StartCoroutine(RunTimelineEvent(currentEvent.cinematicTimeline));
            }
        }
        // ==디버그용 1초 동안 진행 로그 출력==
        float debugTime = 1f;
        float elapsed = 0f;
        while (elapsed < debugTime)
        {
            Debug.Log("시네마틱 진행중...");
            yield return null;
            elapsed += Time.deltaTime;
        }
        // ==디버그용==
        Debug.Log("시네마틱 끝ㄴ");
        runningCutscene = null;
        // 컷신 종료 후 콜백 호출
        onCutsceneEnd?.Invoke();
    }

    private IEnumerator RunTimelineEvent(PlayableDirector timelineToPlay)
    {
        if (timelineToPlay == null) yield break;

        Debug.Log($"단독 Timeline 재생 시작: {timelineToPlay.name}");

        timelineToPlay.extrapolationMode = DirectorWrapMode.Hold;

        bool timelineFinished = false;
        double duration = timelineToPlay.duration;

        System.Action<PlayableDirector> onFinished = null;
        onFinished = (aDirector) =>
        {
            timelineFinished = true;
            aDirector.stopped -= onFinished;
        };
        timelineToPlay.stopped += onFinished;

        timelineToPlay.Play();

        while (!timelineFinished)
        {
            if (timelineToPlay.time >= duration - 0.001)
            {
                Debug.LogWarning("Timeline 시간이 끝에 도달했으나 'stopped' 이벤트 미발생. 강제 종료 처리.");
                timelineFinished = true;
                timelineToPlay.stopped -= onFinished;
                break;
            }

            Debug.Log("기다리는 중");
            yield return null;
        }

        if (timelineToPlay.state == PlayState.Playing)
        {
            timelineToPlay.Stop();
        }

        Debug.Log($"Timeline 종료: {timelineToPlay.name}");
    }

    private IEnumerator RunDialogueEvent(CutsceneEvent currentEvent)
    {
        if (activeCinematicTimeline != null)
        {
            activeCinematicTimeline.Stop();
            activeCinematicTimeline.extrapolationMode = DirectorWrapMode.Hold;
            activeCinematicTimeline = null;
        }

        activeCinematicTimeline = currentEvent.cinematicTimeline;
        if (activeCinematicTimeline != null)
        {
            Debug.Log($"Dialogue 중 배경 Timeline 재생: {activeCinematicTimeline.name}");

            activeCinematicTimeline.extrapolationMode = currentEvent.loopTimeline ? DirectorWrapMode.Loop : DirectorWrapMode.Hold;
            activeCinematicTimeline.Play();
        }

        if (DialogueManager.Instance != null)
        {
            yield return StartCoroutine(DialogueManager.Instance.StartDialogue(currentEvent.dialogueData, isCutscene: true));
        }

        if (activeCinematicTimeline != null)
        {
            Debug.Log($"Dialogue 종료: 배경 Timeline({activeCinematicTimeline.name}) 정지");
            activeCinematicTimeline.Stop();
            activeCinematicTimeline.extrapolationMode = DirectorWrapMode.Hold;
            activeCinematicTimeline = null;
        }
    }
}