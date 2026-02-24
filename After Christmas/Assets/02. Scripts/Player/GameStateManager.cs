using UnityEngine;

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

    private void Awake()
    {
        Instance = this;
    }

    // 일반 상태 변경 (Play, MiniMap 등)
    public void SetState(GameState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
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

