using System;

public static class UIModalGate
{
    private static object currentOwner;
    private static Action currentOwnerClose;

    public static void Acquire(object newOwner, Action newOwnerClose)
    {
        if (currentOwner != null && currentOwner != newOwner)
        {
            currentOwnerClose?.Invoke();
        }
        currentOwner = newOwner;
        currentOwnerClose = newOwnerClose;
    }

    public static void Release(object owner)
    {
        if (currentOwner == owner)
        {
            currentOwner = null;
            currentOwnerClose = null;
        }
    }
}
