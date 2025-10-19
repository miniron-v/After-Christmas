using System.Collections.Generic;
using UnityEngine;
using System;

public class MapVisitManager : MonoBehaviour
{
    public static MapVisitManager Instance;

    [Header("플레이어가 처음 시작하게 될 맵을 끌어다 두세요")]
    [SerializeField] private Map startMap;

    // 맵 이름 → DialogueData
    private Dictionary<string, DialogueData> mapDialogueMap = new Dictionary<string, DialogueData>();

    // 방문 기록
    private HashSet<string> visitedMaps = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 시작 맵 최초 방문 처리
        if (startMap != null)
        {
            VisitMap(startMap.mapName);
        }
    }

    /// <summary>
    /// 맵 최초 방문 시 대화 트리거 등록
    /// </summary>
    public void RegisterMapDialogue(string mapName, DialogueData dialogue)
    {
        if (!mapDialogueMap.ContainsKey(mapName))
        {
            mapDialogueMap[mapName] = dialogue;
        }
    }

    /// <summary>
    /// 특정 맵 방문 처리
    /// 최초 방문이면 대화 실행
    /// </summary>
    public void VisitMap(string mapName, Action onEnd = null)
    {
        if (visitedMaps.Contains(mapName))
        {
            // 이미 방문 → 대화 없음
            onEnd?.Invoke();
            return;
        }

        visitedMaps.Add(mapName);

        if (mapDialogueMap.TryGetValue(mapName, out DialogueData dialogue))
        {
            StartCoroutine(DialogueManager.Instance.StartDialogue(dialogue));
        }
        else
        {
            onEnd?.Invoke();
        }
    }

    /// <summary>
    /// 특정 맵 방문 여부 확인
    /// </summary>
    public bool HasVisited(string mapName)
    {
        return visitedMaps.Contains(mapName);
    }
}
