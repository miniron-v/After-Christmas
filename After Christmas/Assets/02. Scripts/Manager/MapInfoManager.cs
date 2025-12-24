using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

[Serializable]
public class MapRecord
{
    public string mapID;
    public Sprite mapIcon;
    public string description;
    public Vector3 playerSpawnPoint;
    public Vector3 cameraSpawnPoint;

    public MapRecord(string id, Sprite icon = null, string desc = null, Vector3? spawn = null, Vector3? cam = null)
    {
        mapID = id;
        mapIcon = icon;
        description = desc;
        playerSpawnPoint = spawn ?? Vector3.zero;
        cameraSpawnPoint = cam ?? Vector3.zero;
    }
}

public class MapInfoManager : MonoBehaviour
{
    [Serializable]
    public class SceneInfo
    {
        public SceneAsset sceneAsset;
        [HideInInspector] public string sceneName;
    }

    public static MapInfoManager Instance { get; private set; }

    [Header("환자(씬) 리스트")]
    [SerializeField] private List<SceneInfo> sceneInfos = new List<SceneInfo>();

    // 씬별 맵 데이터
    private List<Dictionary<string, MapRecord>> sceneMapData = new List<Dictionary<string, MapRecord>>();

    // 맵 이름 → Map 객체 참조 (컷신/대화용)
    private Dictionary<string, Map> mapObjectMap = new Dictionary<string, Map>();

    // 씬별 방문 기록
    private List<HashSet<string>> visitedMaps = new List<HashSet<string>>();
    [HideInInspector]
    public string currentMap;

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

                sceneMapData.Add(new Dictionary<string, MapRecord>());
                visitedMaps.Add(new HashSet<string>());
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log($"[MapInfoManager] Created instance ({GetInstanceID()})");
        }
        else
        {
            Debug.LogWarning($"[MapInfoManager] Duplicate detected, destroying {GetInstanceID()}");
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
        {
            Debug.Log($"[MapInfoManager] Scene Loaded: {scene.name} (Index {index})");
        }
    }

    public int GetSceneIndex(string sceneName)
    {
        for (int i = 0; i < sceneInfos.Count; i++)
            if (sceneInfos[i].sceneName == sceneName)
                return i;
        return -1;
    }

    private string GetVisitedKey(int sceneIndex, string mapID)
    {
        return $"{sceneInfos[sceneIndex].sceneName}_{mapID}";
    }

    /// <summary>
    /// 맵 방문 처리 (처음 방문일 경우만 컷신/대화 실행)
    /// </summary>
    public void VisitMap(string mapID)
    {
        // 코루틴으로 실행하여 대기 로직 처리
        StartCoroutine(VisitMapRoutine(mapID));
    }

    private IEnumerator VisitMapRoutine(string mapID)
    {
        int sceneIndex = GetSceneIndex(SceneManager.GetActiveScene().name);
        if (sceneIndex < 0) yield break;

        Map mapObj = GetMapObject(mapID);
        if (mapObj == null) yield break;

        // 핵심: 화면 페이드 연출(모자이크/흑백 해제)이 끝날 때까지 기다림
        if (PixelaterAndGreyScaleManager.Instance != null)
        {
            while (PixelaterAndGreyScaleManager.Instance.IsTransitioning)
            {
                yield return null;
            }
        }

        string key = GetVisitedKey(sceneIndex, mapID);
        bool firstVisit = !visitedMaps[sceneIndex].Contains(key);
        visitedMaps[sceneIndex].Add(key);

        if (!firstVisit) yield break;

        Debug.Log($"[MapInfoManager] Transition Finished. Starting Map Sequence: {mapID}");

        // 시네마틱 및 대화 실행
        if (mapObj.cinematicController != null)
        {
            mapObj.cinematicController.StartCutscene(() =>
            {
                if (mapObj.arrivalDialogue != null)
                    StartCoroutine(DialogueManager.Instance.StartDialogue(mapObj.arrivalDialogue));
            });
        }
        else if (mapObj.arrivalDialogue != null)
        {
            yield return StartCoroutine(DialogueManager.Instance.StartDialogue(mapObj.arrivalDialogue));
        }
    }

    /// <summary>
    /// 맵 등록 (Map.Start에서 호출)
    /// isFirst가 true이면 자동 방문 처리
    /// </summary>
    public void RecordMap(Map mapObj)
    {
        if (mapObj == null || string.IsNullOrEmpty(mapObj.mapName)) return;

        int sceneIndex = GetSceneIndex(SceneManager.GetActiveScene().name);
        if (sceneIndex < 0) return;

        var dict = sceneMapData[sceneIndex];
        if (!dict.ContainsKey(mapObj.mapName))
        {
            dict[mapObj.mapName] = new MapRecord(
                mapObj.mapName,
                mapObj.mapDataSO != null ? mapObj.mapDataSO.mapIcon : null,
                mapObj.mapDataSO != null ? mapObj.mapDataSO.description : "",
                mapObj.playerSpawnPoint != null ? mapObj.playerSpawnPoint.position : Vector3.zero,
                mapObj.cameraSpawnPoint != null ? mapObj.cameraSpawnPoint.position : Vector3.zero
            );
        }

        mapObjectMap[mapObj.mapName] = mapObj;

        // isFirst이면 방문 처리
        if (mapObj.isStartMap)
        {
            VisitMap(mapObj.mapName);
            currentMap = mapObj.mapName;
        }
    }

    public Map GetMapObject(string mapID)
    {
        if (mapObjectMap.TryGetValue(mapID, out Map mapObj))
            return mapObj;
        return null;
    }

    public MapRecord GetMapRecord(string mapID, int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= sceneMapData.Count)
            return null;

        sceneMapData[sceneIndex].TryGetValue(mapID, out var record);
        return record;
    }

    public IReadOnlyList<string> GetAllMapIDs(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= sceneMapData.Count)
            return Array.Empty<string>();

        var keys = new List<string>(sceneMapData[sceneIndex].Keys);
        keys.Sort(StringComparer.Ordinal);
        return keys;
    }

    public IReadOnlyList<string> GetVisitedMapIDs(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= sceneMapData.Count)
            return Array.Empty<string>();

        if (visitedMaps.Count <= sceneIndex)
            return Array.Empty<string>();

        List<string> visitedList = new List<string>();
        foreach (var mapID in sceneMapData[sceneIndex].Keys)
        {
            string key = GetVisitedKey(sceneIndex, mapID);
            if (visitedMaps[sceneIndex].Contains(key))
                visitedList.Add(mapID);
        }

        visitedList.Sort(StringComparer.Ordinal);
        return visitedList;
    }

}
