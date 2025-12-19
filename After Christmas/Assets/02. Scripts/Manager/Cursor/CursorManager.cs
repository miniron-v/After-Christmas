using UnityEngine;

[System.Serializable]
public class CursorData
{
    public CursorType type;
    public Texture2D texture;

    [Tooltip("픽셀 기준 핫스팟 (좌상단이 0,0)")]
    public Vector2 hotspot;
}

public enum CursorType
{
    Default,
    ButtonHover
}

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    [Header("Cursor Settings")]
    [SerializeField] private CursorData[] cursors;

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
}
