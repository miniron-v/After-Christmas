using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeleportSelectionUI : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;     // 버튼들이 들어갈 부모 (teleportPanel)
    [SerializeField] private Button optionButtonPrefab; // 버튼 프리팹
    [SerializeField] private float radius;       // Canvas Local 기준 반지름

    private Action<int> onSelect; // 후보 인덱스 콜백

    void Awake()
    {
        gameObject.SetActive(false); // 기본 꺼두기
    }

    /// <summary>
    /// UI 열기 (worldPosition 기준으로 위치 설정, labels 반원형 배치)
    /// </summary>
    public void Open(Vector3 worldPosition, List<string> labels, Action<int> onSelectIndex)
    {
        // 위치 설정
        transform.position = worldPosition;

        // 기존 모달 관리
        UIModalGate.Acquire(this, Close);

        onSelect = onSelectIndex;

        // 기존 버튼 제거
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);

        int N = labels.Count;
        float startAngle = -90f; // 왼쪽
        float endAngle = 90f;    // 오른쪽

        // Canvas Scale 보정
        Vector3 canvasScale = contentRoot.lossyScale;
        float scaleFactor = canvasScale.x; // x, y가 같다고 가정

        // 버튼 생성 및 배치
        for (int i = 0; i < N; i++)
        {
            float angle = Mathf.Lerp(startAngle, endAngle, N == 1 ? 0.5f : (float)i / (N - 1));
            float rad = angle * Mathf.Deg2Rad;

            // 수정: X좌표는 sin, Y좌표는 cos로 배치
            Vector3 localPos = new Vector3(
                Mathf.Sin(rad) * radius / scaleFactor,  // X축 좌우
                Mathf.Cos(rad) * radius / scaleFactor,  // Y축 위로 볼록
                0f
            );

            var btn = Instantiate(optionButtonPrefab, contentRoot);
            btn.GetComponent<RectTransform>().localPosition = localPos;

            var text = btn.GetComponentInChildren<TMP_Text>();
            if (text) text.text = labels[i];

            int captured = i;
            btn.onClick.AddListener(() =>
            {
                onSelect?.Invoke(captured);
                Close();
            });
        }

        gameObject.SetActive(true);
    }

    public void Close()
    {
        UIModalGate.Release(this);
        gameObject.SetActive(false);
        onSelect = null;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    void OnDisable()
    {
        // 방어코드
        UIModalGate.Release(this);
    }
}
