using UnityEngine;
using UnityEngine.EventSystems; // 클릭 이벤트를 받기 위해 필요

public class UIObjectSpawner : MonoBehaviour, IPointerClickHandler
{
    [Header("생성할 버튼/포스트잇 프리팹")]
    [SerializeField] private GameObject buttonPrefab;

    [Header("포스트잇 패널")]
    [SerializeField] private PostitPanel postitPanel;

    [Header("생성된 버튼들이 담길 부모 (미지정 시 이 패널의 자식으로 생성)")]
    [SerializeField] private Transform container;

    private void Awake()
    {
        if (container == null)
            container = this.transform;
    }

    // 패널이 클릭되었을 때 실행되는 함수
    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. 클릭된 월드 좌표 가져오기
        Vector3 clickWorldPos = eventData.pointerCurrentRaycast.worldPosition;

        // 2. 프리팹 생성
        GameObject newButton = Instantiate(buttonPrefab, container);
        newButton.GetComponentInChildren<PostItAnchorButton>().OutSideInit(postitPanel);

        // 3. 위치 설정
        newButton.transform.position = clickWorldPos;

        // 4. UI가 겹치지 않게 패널 앞으로 살짝 띄우기
        newButton.transform.localPosition += new Vector3(-3.66f,3f,-3.66f); 
    }
}