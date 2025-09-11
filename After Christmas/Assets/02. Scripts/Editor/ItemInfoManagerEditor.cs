#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemInfoManager))]
public class ItemInfoManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ItemInfoManager manager = (ItemInfoManager)target;
        if (GUILayout.Button("Update Scene Names"))
        {
            foreach (var info in manager.GetType()
                     .GetField("sceneInfos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     .GetValue(manager) as System.Collections.IList)
            {
                var sceneInfo = info as dynamic;
                if (sceneInfo.sceneAsset != null)
                    sceneInfo.sceneName = sceneInfo.sceneAsset.name;
            }
        }
    }
}
#endif
