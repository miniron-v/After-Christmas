using System;
using UnityEngine;
using UnityEngine.Events;

public enum GameState
{
    // 제한 X
    Play,
    // 페이드 중일 때
    Fading,
    // UI 열려있는 시점
    UIOpen,
    // 대화창 열려있는 시점
    Dialogue,
    // 시네마틱 열려있는 시점
    Cinematic,
    // 미니맵 열려있는 시점
    MiniMap,
    // 일시정지(추후 시네마틱, 대화 시퀀스 등 전부 제어하기 위한 용도)
    Paused
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public GameState currentState { get; private set; } = GameState.Play;
    public GameState prevState { get; private set; }

    [Serializable]
    public class GameStateChangedUnityEvent : UnityEvent<GameState, GameState> { }

    [Header("Events")]
    // 모든 상태 변경 시 호출
    [SerializeField] private GameStateChangedUnityEvent onStateChanged;
    // 미니맵 진입 시 호출
    [SerializeField] private UnityEvent onEnterMiniMap;
    // 미니맵 이탈 시 호출
    [SerializeField] private UnityEvent onExitMiniMap;

    private void Awake()
    {
        Instance = this;
    }

    // 일반 상태 변경 (Play, MiniMap 등)
    public void SetState(GameState newState)
    {
        if (currentState == newState) return;

        var old = currentState;
        currentState = newState;

        onStateChanged?.Invoke(old, newState);

        if (old != GameState.MiniMap && newState == GameState.MiniMap) { onEnterMiniMap?.Invoke(); }
        if (old == GameState.MiniMap && newState != GameState.MiniMap) { onExitMiniMap?.Invoke(); }
        Debug.Log($"<color=red>{currentState}</color>");
    }

    // UI / Dialogue / Cinematic 들어갈 때
    public void EnterOverlayState(GameState overlayState)
    {
        prevState = currentState;
        SetState(overlayState);
    }

    // UI / Dialogue / Cinematic 나갈 때
    public void ExitOverlayState()
    {
        SetState(prevState);
    }

    public bool IsPlayerControllable()
    {
        return currentState == GameState.Play;
    }
}

