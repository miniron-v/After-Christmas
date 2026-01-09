using System.Collections.Generic;
using UnityEngine;

public static class PostItRegistry
{
    private static readonly HashSet<PostItView> activePostIts = new();

    private static readonly HashSet<GameObject> registeredButtons = new();

    public static void Register(PostItView view) { activePostIts.Add(view); }
    public static void Unregister(PostItView view) { activePostIts.Remove(view); }

    public static void RegisterButton(GameObject btn) => registeredButtons.Add(btn);
    public static void UnregisterButton(GameObject btn) => registeredButtons.Remove(btn);

    public static void HideAll()
    {
        // 복사본을 만들어서 순회
        var snapshot = new List<PostItView>(activePostIts);

        foreach (var view in snapshot)
        {
            view.Hide(); // Hide 안에서 Unregister 됨
        }

        activePostIts.Clear();
    }

    public static void SetAllButtonsVisible(bool isVisible)
    {
        foreach (var btn in registeredButtons)
        {
            if (btn != null) btn.SetActive(isVisible);
        }
    }
}
