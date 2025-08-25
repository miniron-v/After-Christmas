using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeleportSelectionUI : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;     // 버튼들이 들어갈 부모
    [SerializeField] private Button optionButtonPrefab; // 버튼 프리팹

    private Action<int> onSelect; // 후보 인덱스 콜백

    void Awake()
    {
        gameObject.SetActive(false); // 기본 꺼두기
    }

    public void Open(List<string> labels, Action<int> onSelectIndex)
    {
        // 기존 모달이 있으면 자동으로 닫히고 이 창이 소유권을 가짐
        UIModalGate.Acquire(this, Close);

        onSelect = onSelectIndex;

        // 내용 초기화
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);

        // 버튼 생성
        for (int i = 0; i < labels.Count; i++)
        {
            Debug.Log("버튼생성?");
            var btn = Instantiate(optionButtonPrefab, contentRoot);
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
