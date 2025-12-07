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
    public PlayerState CurrentState { get; private set; } = PlayerState.Play;

    private void Awake()
    {
        Instance = this;
    }

    public void SetState(PlayerState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        Debug.Log(CurrentState);
    }

    public bool IsPlayerControllable()
    {
        return CurrentState == PlayerState.Play;
    }
}

