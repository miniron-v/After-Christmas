using System;
using UnityEngine;

public static class UIModalGate
{
    private static object currentOwner;
    private static Action currentOwnerClose;

    public static void Acquire(object newOwner, Action newOwnerClose)
    {
        if (currentOwner != null && currentOwner != newOwner)
        {
            Debug.Log("여기");
            // Unity Object가 이미 Destroy 되었는지 체크
            if (currentOwner is UnityEngine.Object unityObj && unityObj == null)
            {
                // 이미 파괴됨 → 그냥 Release
                Release(currentOwner);
            }
            else
            {
                currentOwnerClose?.Invoke();
            }
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
