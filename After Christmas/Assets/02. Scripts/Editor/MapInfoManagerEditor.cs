#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(MapInfoManager))]
public class MapInfoManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapInfoManager manager = (MapInfoManager)target;

        if (GUILayout.Button("Update Scene Names"))
        {
            // reflection으로 private 필드 가져오기
            var sceneInfosField = manager.GetType()
                .GetField("sceneInfos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (sceneInfosField != null)
            {
                var sceneInfos = sceneInfosField.GetValue(manager) as IList;
                if (sceneInfos != null)
                {
                    foreach (var info in sceneInfos)
                    {
                        // dynamic으로 접근
                        var sceneInfo = info as dynamic;
                        if (sceneInfo.sceneAsset != null)
                        {
                            sceneInfo.sceneName = sceneInfo.sceneAsset.name;
                        }
                    }

                    EditorUtility.SetDirty(manager);
                    Debug.Log("[MapInfoManagerEditor] Scene names updated.");
                }
            }
        }
    }
}
#endif
