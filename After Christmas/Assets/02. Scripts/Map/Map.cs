using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("맵 관련 데이터 (SO)")]
    public MapDataSO mapDataSO;

    // 맵 id, so에서 관리하므로 hide in inspector
    [HideInInspector]
    public string mapName => mapDataSO.mapID;

    [Header("현재 맵으로 텔레포트했을 때 플레이어 스폰 위치")]
    public Transform playerSpawnPoint;

    [Header("현재 맵으로 텔레포트했을 때 카메라 스폰 위치")]
    public Transform cameraSpawnPoint;

    [Header("최초 입장 시 실행할 대화")]
    public DialogueData arrivalDialogue;

    // 맵 입장 시 사용할 시네마틱 컨트롤러
    // awake에서 초기화해 주므로 hide in inspector
    [HideInInspector]
    public CinematicController cinematicController;

    public bool isStartMap = false;

    private void Awake()
    {
        // 변수 할당 최소화를 위한 getcomponent 사용
        cinematicController = GetComponent<CinematicController>();
    }

    void Start()
    {
        MapInfoManager.Instance.RecordMap(this);
        InitializeChildItems();
    }

    private void InitializeChildItems()
    {
        Item[] items = GetComponentsInChildren<Item>(true);
        foreach (var item in items)
        {
            item.mapName = mapName;
            item.cameraSpawnPoint = cameraSpawnPoint;
        }
    }

    // 추가된 부분: 알파 값을 수정하는 함수
    public void SetMapAlpha(float alpha)
    {
        // 모든 Renderer에 대해 alpha 값 수정
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            Material material = renderer.material;

            // 렌더링 모드를 Transparent로 변경
            if (material.HasProperty("_Mode"))
            {
                material.SetFloat("_Mode", 3); // 3은 Transparent 모드
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0); // 깊이 버퍼 비활성화 (투명 객체)
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000; // 투명 오브젝트의 기본 렌더 큐 값
            }

            // 알파 값 설정
            Color color = material.color;
            color.a = alpha;  // alpha 값 수정
            material.color = color;
        }
    }


}
