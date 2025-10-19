using System.Collections.Generic;
using UnityEngine;

public class MapVisitManager : MonoBehaviour
{
    public static MapVisitManager Instance;

    [SerializeField] private Map startMap;

    private HashSet<string> visitedMaps = new HashSet<string>();
    private Dictionary<string, Map> mapObjectMap = new Dictionary<string, Map>();

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
            VisitMap(startMap.mapName);
        }
    }

    public void RegisterMap(string mapName, Map mapObj)
    {
        if (!mapObjectMap.ContainsKey(mapName))
            mapObjectMap[mapName] = mapObj;
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


    public bool HasVisited(string mapName)
    {
        return visitedMaps.Contains(mapName);
    }
}
