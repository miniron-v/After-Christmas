using System;
using UnityEngine;

[Serializable]
public class CursorShapeData
{
    public CursorType type;
    public Texture2D texture;

    [Tooltip("픽셀 기준 핫스팟 (좌상단이 0,0)")]
    public Vector2 hotspot;
}

public enum CursorType
{
    Default,
    ButtonHover,
    TextInput
}

public class CursorShapeManager : MonoBehaviour
{
    public static CursorShapeManager Instance;

    [Header("Cursor Settings")]
    [SerializeField] private CursorShapeData[] cursors;

    private CursorType currentCursor = CursorType.Default;
    private object currentOwner = null;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RequestCursor(object owner, CursorType type)
    {
        if (owner == null) return;

        currentOwner = owner;
        ApplyCursor(type);
    }

    public void ReleaseCursor(object owner)
    {
        if (currentOwner != owner)
            return;

        currentOwner = null;
        ApplyCursor(CursorType.Default);
    }

    private void ApplyCursor(CursorType type)
    {
        if (currentCursor == type)
            return;

        if (type == CursorType.Default)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            currentCursor = CursorType.Default;
            return;
        }

        foreach (var cursor in cursors)
        {
            if (cursor.type == type && cursor.texture != null)
            {
                Cursor.SetCursor(cursor.texture, cursor.hotspot, CursorMode.Auto);
                currentCursor = type;
                return;
            }
        }

        // 못 찾았으면 안전하게 Default
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        currentCursor = CursorType.Default;
    }

    public Vector2 GetCurrentHotspot()
    {
        foreach (var c in cursors)
        {
            if (c.type == currentCursor)
                return c.hotspot;
        }
        return Vector2.zero;
    }

}
