using System;

public static class TeleportEventManager
{
    public static event Action OnTeleport;

    public static void NotifyTeleport()
    {
        OnTeleport?.Invoke();
    }
}
