using System.Collections.Generic;

public static class PostItRegistry
{
    private static readonly HashSet<PostItView> activePostIts = new();

    public static void Register(PostItView view)
    {
        activePostIts.Add(view);
    }

    public static void Unregister(PostItView view)
    {
        activePostIts.Remove(view);
    }

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

}
