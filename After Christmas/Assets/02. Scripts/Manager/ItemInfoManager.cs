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
        public SceneAsset sceneAsset;
        [HideInInspector] public string sceneName;
    }

    public static ItemInfoManager Instance { get; private set; }

    [Header("기억 씬 리스트")]
    [Tooltip("기억 씬들을 플레이 순서에 따라서 인스펙터에 넣어 주세요")]
    [SerializeField] private List<SceneInfo> sceneInfos = new List<SceneInfo>();

    // 씬<아이템 이름,<아이템에 연결된 텔레포트 지점>>
    private List<Dictionary<string, List<SpawnTransform>>> itemList
        = new List<Dictionary<string, List<SpawnTransform>>>();

    private Dictionary<string, Sprite> itemIcons
        = new Dictionary<string, Sprite>();

    // 추가: 방문 기록
    private HashSet<int> visitedScenes = new HashSet<int>();

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

            // 씬 로딩 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int index = GetSceneIndex(scene.name);
        if (index >= 0)
            MarkSceneVisited(index);
    }

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

    public void RecordItemInfo(string itemID, SpawnTransform info, Sprite icon = null)
    {
        int sceneIndex = GetSceneIndex(SceneManager.GetActiveScene().name);
        if (sceneIndex < 0) return;

        // 씬별 아이템 위치 기록
        var dict = itemList[sceneIndex];
        if (!dict.ContainsKey(itemID))
            dict[itemID] = new List<SpawnTransform>();

        dict[itemID].Add(info);

        // 아이콘 저장 (이미 등록되어 있으면 건너뛰기)
        if (icon != null && !itemIcons.ContainsKey(itemID))
        {
            itemIcons[itemID] = icon;
        }
    }

    // 아이콘 조회용
    public Sprite GetItemIcon(string itemID)
    {
        if (itemIcons.TryGetValue(itemID, out var sprite))
            return sprite;
        return null;
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

    // 방문 기록 관련
    public void MarkSceneVisited(int sceneIndex)
    {
        if (!visitedScenes.Contains(sceneIndex))
            visitedScenes.Add(sceneIndex);
    }

    public bool HasVisitedScene(int sceneIndex)
    {
        return visitedScenes.Contains(sceneIndex);
    }

    #endregion
}
