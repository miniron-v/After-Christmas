using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using System.Collections;
using System;

public class CinematicController : MonoBehaviour
{

    public static Action OnStart;
    public static Action OnEnd;

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
        OnStart?.Invoke();

        // 1. 시네마틱 시작 시 플레이어 상태 CINEMATIC으로 전환
        PlayerStateManager.Instance?.SetState(PlayerState.Cinematic);

        if (runningCutscene == null)
        {
            // RunCutsceneSequence 호출, 끝나면 PLAY로 전환
            runningCutscene = StartCoroutine(RunCutsceneSequence(() =>
            {
                // 2. 시네마틱 종료 시 플레이어 상태 PLAY로 전환
                PlayerStateManager.Instance?.SetState(PlayerState.Play);
                OnEnd?.Invoke();
                // 기존 외부 콜백이 있으면 실행
                onCutsceneEnd?.Invoke();
            }));
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

        Debug.Log("dialogue : checking dialogue isActive and finished dialogue");
/*        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
        {
            DialogueManager.Instance.EndDialogue();
        }*/

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

        Debug.Log("dialogue: start dialogue coroutine");
/*        if (DialogueManager.Instance != null)
        {
            yield return StartCoroutine(DialogueManager.Instance.StartDialogue(currentEvent.dialogueData, isCutscene: true));
        }*/
        yield return new WaitForEndOfFrame(); // 코루틴 오류 방지용으로 추가한 것

        if (activeCinematicTimeline != null)
        {
            Debug.Log($"Dialogue 종료: 배경 Timeline({activeCinematicTimeline.name}) 정지");
            activeCinematicTimeline.Stop();
            activeCinematicTimeline.extrapolationMode = DirectorWrapMode.Hold;
            activeCinematicTimeline = null;
        }
    }
}