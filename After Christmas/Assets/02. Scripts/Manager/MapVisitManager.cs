using System;
using System.Collections.Generic;
using UnityEngine;

public class MapVisitManager : MonoBehaviour
{
    public static MapVisitManager Instance;

    [SerializeField] private Map startMap;

    private HashSet<string> visitedMaps = new HashSet<string>();
    private Dictionary<string, Map> mapObjectMap = new Dictionary<string, Map>();

    // Map이 등록될 때 발생하는 이벤트
    public event Action<Map> OnMapRegistered;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (startMap != null)
        {
            // StartMap 방문은 OnMapRegistered 이벤트로 처리
            OnMapRegistered += CheckStartMapVisit;
        }
    }

    private void CheckStartMapVisit(Map map)
    {
        if (map == startMap)
        {
            VisitMap(map.mapName);
            OnMapRegistered -= CheckStartMapVisit; // 한 번만 호출
        }
    }

    public void RegisterMap(string mapName, Map mapObj)
    {
        if (!mapObjectMap.ContainsKey(mapName))
            mapObjectMap[mapName] = mapObj;

        // Map 등록 이벤트 호출
        OnMapRegistered?.Invoke(mapObj);
    }

    public void VisitMap(string mapName)
    {
        if (visitedMaps.Contains(mapName)) return;

        visitedMaps.Add(mapName);

        if (mapObjectMap.TryGetValue(mapName, out Map mapObj))
        {
            if (mapObj.cinematicController != null)
            {
                // 컷신 종료 후 arrivalDialogue 실행하도록 콜백 전달
                mapObj.cinematicController.StartCutscene(() =>
                {
                    if (mapObj.arrivalDialogue != null)
                        StartCoroutine(DialogueManager.Instance.StartDialogue(mapObj.arrivalDialogue));
                });
            }
            else
            {
                // 컷신 없으면 바로 대화
                if (mapObj.arrivalDialogue != null)
                    StartCoroutine(DialogueManager.Instance.StartDialogue(mapObj.arrivalDialogue));
            }
        }
    }

    // 방문한 맵 이름 리스트 반환
    public IEnumerable<string> GetVisitedMaps()
    {
        return visitedMaps;
    }

    // mapName으로 Map 객체 반환
    public Map GetMapObject(string mapName)
    {
        if (mapObjectMap.TryGetValue(mapName, out Map mapObj))
            return mapObj;
        return null;
    }

    public bool HasVisited(string mapName)
    {
        return visitedMaps.Contains(mapName);
    }
}
