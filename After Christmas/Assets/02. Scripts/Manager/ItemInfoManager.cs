using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

[Serializable]
public class ItemRecord
{
    public List<SpawnTransform> spawnPoints = new List<SpawnTransform>();
    public Sprite icon;
    public string description;
    public List<string> additiveDescriptions = new List<string>(); // 추가설명 리스트

    public ItemRecord(SpawnTransform firstSpawn, Sprite icon = null, string desc = null, string addDesc = null)
    {
        if (firstSpawn.mapName != null)
            spawnPoints.Add(firstSpawn);

        this.icon = icon;
        description = desc;

        if (!string.IsNullOrEmpty(addDesc))
            additiveDescriptions.Add(addDesc);
    }
}

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

    // ✅ 씬별 아이템 데이터: 씬 -> (아이템 이름 -> 아이템정보)
    private List<Dictionary<string, ItemRecord>> sceneItemData
        = new List<Dictionary<string, ItemRecord>>();

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

                sceneItemData.Add(new Dictionary<string, ItemRecord>());
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log($"[ItemInfoManager] Awake - Created new instance ({GetInstanceID()})");
        }
        else
        {
            Debug.LogWarning($"[ItemInfoManager] Duplicate found ({GetInstanceID()}), destroying.");
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

    private void Update()
    {
        // 0키 누르면 전체 디버그 출력
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            DebugDumpAllData();
        }
        // 설명이 잘 들어가는지 테스트하기 위한 임시 디버그로그. 다이아몬드 기준으로 작성됨
        if (Input.GetKeyDown(KeyCode.R))
        {
            string targetScene = "DemoTest-UIMakeScene";
            string targetItemID = "다이아몬드";

            int sceneIdx = GetSceneIndex(targetScene);

            if (sceneIdx != -1)
            {
                var dict = sceneItemData[sceneIdx];
                if (dict.TryGetValue(targetItemID, out var record))
                {
                    Debug.Log($"<color=cyan>[Item Debug]</color> <b>{targetItemID}</b> 의 추가 설명 (개수: {record.additiveDescriptions.Count})");

                    if (record.additiveDescriptions.Count == 0)
                    {
                        Debug.Log("- 등록된 추가 설명이 없습니다.");
                    }
                    else
                    {
                        for (int i = 0; i < record.additiveDescriptions.Count; i++)
                        {
                            Debug.Log($"  {i + 1}. {record.additiveDescriptions[i]}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[Item Debug] '{targetScene}' 씬에서 '{targetItemID}' 아이템 기록을 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogError($"[Item Debug] '{targetScene}' 이름의 씬이 ItemInfoManager 리스트에 등록되어 있지 않습니다.");
            }
        }
    }

    /// <summary>
    /// ✅ 현재 ItemInfoManager 내부의 모든 씬 / 아이템 / 스폰 정보 디버그 출력
    /// </summary>
    private void DebugDumpAllData()
    {
        Debug.Log($"[ItemInfoManager DEBUG DUMP] InstanceID={GetInstanceID()} | SceneCount={sceneInfos.Count}");

        for (int i = 0; i < sceneInfos.Count; i++)
        {
            string sceneName = sceneInfos[i].sceneName;
            var dict = sceneItemData[i];
            int itemCount = dict.Count;
            bool visited = visitedScenes.Contains(i);

            Debug.Log($" ── Scene[{i}] '{sceneName}' | Items={itemCount} | Visited={visited}");

            foreach (var kvp in dict)
            {
                string itemID = kvp.Key;
                var record = kvp.Value;

                string iconInfo = record.icon != null ? "✅Icon" : "❌NoIcon";
                string descInfo = string.IsNullOrEmpty(record.description) ? "❌NoDesc" : $"📝{record.description}";
                Debug.Log($"     • ItemID='{itemID}' | {iconInfo} | {descInfo} | Spawns={record.spawnPoints.Count}");
            }
        }

        Debug.Log($"[ItemInfoManager DEBUG DUMP END]");
    }

    public int GetSceneIndex(string sceneName)
    {
        for (int i = 0; i < sceneInfos.Count; i++)
            if (sceneInfos[i].sceneName == sceneName)
                return i;
        return -1;
    }

    #region 데이터 접근

    public bool IsHavingItem(string itemID)
    {
        int sceneIndex = GetSceneIndex(SceneManager.GetActiveScene().name);
        if (sceneIndex < 0) return false;

        return sceneItemData[sceneIndex].ContainsKey(itemID);
    }

    // ✅ 아이템 기록 (한 번에 icon, description, spawn 포함)
    public void RecordItemInfo(string itemID, SpawnTransform info, Sprite icon = null, string description = null, string addDesc = null)
    {
        int sceneIndex = GetSceneIndex(SceneManager.GetActiveScene().name);
        if (sceneIndex < 0) return;

        var dict = sceneItemData[sceneIndex];

        if (!dict.TryGetValue(itemID, out var record))
        {
            record = new ItemRecord(info, icon, description, addDesc);
            dict[itemID] = record;
        }
        else
        {
            record.spawnPoints.Add(info);
            if (icon != null && record.icon == null)
                record.icon = icon;
            if (!string.IsNullOrEmpty(description) && string.IsNullOrEmpty(record.description))
                record.description = description;
            if (!string.IsNullOrEmpty(addDesc) && !record.additiveDescriptions.Contains(addDesc))
            {
                record.additiveDescriptions.Add(addDesc); // 추가설명 리스트에 추가
            }
        }
    }

    public Sprite GetItemIcon(string itemID)
    {
        foreach (var dict in sceneItemData)
        {
            if (dict.TryGetValue(itemID, out var record) && record.icon != null)
                return record.icon;
        }
        return null;
    }

    public string GetItemDescription(string itemID)
    {
        foreach (var dict in sceneItemData)
        {
            if (dict.TryGetValue(itemID, out var record) && !string.IsNullOrEmpty(record.description))
                return record.description;
        }
        return "No description";
    }

    public IReadOnlyList<string> GetAllItemIDs(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= sceneItemData.Count)
            return Array.Empty<string>();

        var keys = new List<string>(sceneItemData[sceneIndex].Keys);
        keys.Sort(StringComparer.Ordinal);
        return keys;
    }

    public IReadOnlyList<SpawnTransform> GetSpawnsOf(string itemID, int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= sceneItemData.Count)
            return Array.Empty<SpawnTransform>();

        var dict = sceneItemData[sceneIndex];
        if (!dict.TryGetValue(itemID, out var record))
            return Array.Empty<SpawnTransform>();

        return record.spawnPoints;
    }

    public int GetSceneCount() => sceneInfos.Count;

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
