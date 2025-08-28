using System;

// UI가 한번에 하나만 열리게끔 제어
public static class UIModalGate
{
    private static object currentOwner;
    private static Action currentOwnerClose;

    // 새로 열 때: 기존이 있으면 닫고, 소유권을 현재로 교체
    public static void Acquire(object newOwner, Action newOwnerClose)
    {
        if (currentOwner != null && currentOwner != newOwner)
        {
            currentOwnerClose?.Invoke();
        }
        currentOwner = newOwner;
        currentOwnerClose = newOwnerClose;
    }

    // 닫을 때: 내가 소유자면 게이트 해제
    public static void Release(object owner)
    {
        if (currentOwner == owner)
        {
            currentOwner = null;
            currentOwnerClose = null;
        }
    }
}

