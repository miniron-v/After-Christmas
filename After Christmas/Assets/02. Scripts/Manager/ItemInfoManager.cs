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
        public SceneAsset sceneAsset;  // 드래그용
        [HideInInspector] public string sceneName;
    }

    public static ItemInfoManager Instance { get; private set; }

    [SerializeField] private List<SceneInfo> sceneInfos = new List<SceneInfo>();

    private List<Dictionary<string, List<SpawnTransform>>> itemList 
        = new List<Dictionary<string, List<SpawnTransform>>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (var info in sceneInfos)
            {
                if (info.sceneAsset != null)
                    info.sceneName = info.sceneAsset.name;

                itemList.Add(new Dictionary<string, List<SpawnTransform>>());
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable() => SceneManager.activeSceneChanged += OnSceneChanged;
    private void OnDisable() => SceneManager.activeSceneChanged -= OnSceneChanged;

    private void OnSceneChanged(Scene oldScene, Scene newScene) { }

    public int GetSceneIndex(string sceneName)
    {
        for (int i = 0; i < sceneInfos.Count; i++)
            if (sceneInfos[i].sceneName == sceneName) return i;
        return -1;
    }

    #region 데이터 접근

    public bool IsHavingItem(string itemID)
    {
        int sceneIndex = GetSceneIndex(SceneManager.GetActiveScene().name);
        if (sceneIndex < 0) return false;

        return itemList[sceneIndex].ContainsKey(itemID);
    }

    public void RecordItemInfo(string itemID, SpawnTransform info)
    {
        int sceneIndex = GetSceneIndex(SceneManager.GetActiveScene().name);
        if (sceneIndex < 0) return;

        var dict = itemList[sceneIndex];
        if (!dict.ContainsKey(itemID))
            dict[itemID] = new List<SpawnTransform>();

        dict[itemID].Add(info);
    }

    public IReadOnlyList<string> GetAllItemIDs(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= itemList.Count) return Array.Empty<string>();
        var keys = new List<string>(itemList[sceneIndex].Keys);
        keys.Sort(StringComparer.Ordinal);
        return keys;
    }

    public IReadOnlyList<SpawnTransform> GetSpawnsOf(string itemID, int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= itemList.Count) return Array.Empty<SpawnTransform>();
        var dict = itemList[sceneIndex];
        if (!dict.ContainsKey(itemID)) return Array.Empty<SpawnTransform>();
        return dict[itemID];
    }

    public int GetSceneCount() => sceneInfos.Count;

    #endregion
}
