using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class ItemInfoManager : MonoBehaviour
{
    [Serializable]
    public class SceneInfo
    {
        [HideInInspector] public string sceneName;
        [HideInInspector] public int sceneIndex;
        public SceneAsset sceneAsset;  // 드래그용
    }


    public static ItemInfoManager Instance { get; private set; }

    [SerializeField] private List<SceneInfo> sceneInfos = new List<SceneInfo>();

    private Dictionary<string, int> useableScene = new Dictionary<string, int>();
    private Dictionary<int, Dictionary<string, List<SpawnTransform>>> itemList
        = new Dictionary<int, Dictionary<string, List<SpawnTransform>>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            for (int i = 0; i < sceneInfos.Count; i++)
            {
                SceneInfo info = sceneInfos[i];
                if (info.sceneAsset != null)
                    info.sceneName = info.sceneAsset.name;
                info.sceneIndex = i;

                if (!useableScene.ContainsKey(info.sceneName))
                    useableScene.Add(info.sceneName, info.sceneIndex);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable() => SceneManager.activeSceneChanged += OnSceneChanged;
    private void OnDisable() => SceneManager.activeSceneChanged -= OnSceneChanged;

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        string newSceneName = newScene.name;
        if (!useableScene.ContainsKey(newSceneName))
            useableScene.Add(newSceneName, useableScene.Count);
    }

    #region 데이터 접근

    public bool IsHavingItem(string itemID)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!useableScene.ContainsKey(sceneName)) return false;

        int sceneIndex = useableScene[sceneName];
        return itemList.ContainsKey(sceneIndex) && itemList[sceneIndex].ContainsKey(itemID);
    }

    public void RecordItemInfo(string itemID, SpawnTransform info)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!useableScene.ContainsKey(sceneName)) return;

        int sceneIndex = useableScene[sceneName];
        if (!itemList.ContainsKey(sceneIndex))
            itemList.Add(sceneIndex, new Dictionary<string, List<SpawnTransform>>());
        if (!itemList[sceneIndex].ContainsKey(itemID))
            itemList[sceneIndex].Add(itemID, new List<SpawnTransform>());

        itemList[sceneIndex][itemID].Add(info);
    }

    public IReadOnlyList<string> GetAllItemIDs()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!useableScene.ContainsKey(sceneName)) return Array.Empty<string>();

        int sceneIndex = useableScene[sceneName];
        if (!itemList.ContainsKey(sceneIndex)) return Array.Empty<string>();

        var keys = new List<string>(itemList[sceneIndex].Keys);
        keys.Sort(StringComparer.Ordinal);
        return keys;
    }

    public IReadOnlyList<SpawnTransform> GetSpawnsOf(string itemID)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!useableScene.ContainsKey(sceneName)) return Array.Empty<SpawnTransform>();

        int sceneIndex = useableScene[sceneName];
        if (!itemList.ContainsKey(sceneIndex) || !itemList[sceneIndex].TryGetValue(itemID, out var list))
            return Array.Empty<SpawnTransform>();

        return list;
    }

    #endregion
}
