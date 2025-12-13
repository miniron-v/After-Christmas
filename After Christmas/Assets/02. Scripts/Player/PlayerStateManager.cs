using UnityEngine;

public enum PlayerState
{
    // 제한 X
    Play,
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

public class PlayerStateManager : MonoBehaviour
{
    public static PlayerStateManager Instance { get; private set; }

    public PlayerState currentState { get; private set; } = PlayerState.Play;
    public PlayerState prevState { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // 일반 상태 변경 (Play, MiniMap 등)
    public void SetState(PlayerState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        Debug.Log(currentState);
    }

    // UI / Dialogue / Cinematic 들어갈 때
    public void EnterOverlayState(PlayerState overlayState)
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
        return currentState == PlayerState.Play;
    }
}

