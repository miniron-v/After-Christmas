using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MinimapManager))]
public class MinimapManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MinimapManager manager = (MinimapManager)target;

        GUILayout.Space(10);

        if (GUILayout.Button("현재 카메라 설정을 미니맵 뷰 시점으로 설정"))
        {
            manager.SaveCurrentCameraAsMinimapView();
        }
    }
}
