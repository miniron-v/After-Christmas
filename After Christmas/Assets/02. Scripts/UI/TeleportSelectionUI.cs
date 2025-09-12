using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeleportSelectionUI : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private Button optionButtonPrefab;
    [SerializeField] private float radius;

    private Action<int> onSelect;


    public void Open(Vector3 worldPosition, List<string> labels, Action<int> onSelectIndex)
    {
        transform.position = worldPosition;

        UIModalGate.Acquire(this, Close);

        onSelect = onSelectIndex;

        // 기존 버튼 제거
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);

        int N = labels.Count;
        float startAngle = -90f;
        float endAngle = 90f;

        Vector3 canvasScale = contentRoot.lossyScale;
        float scaleFactor = canvasScale.x;

        for (int i = 0; i < N; i++)
        {
            float angle = Mathf.Lerp(startAngle, endAngle, N == 1 ? 0.5f : (float)i / (N - 1));
            float rad = angle * Mathf.Deg2Rad;

            Vector3 localPos = new Vector3(
                Mathf.Sin(rad) * radius / scaleFactor,
                Mathf.Cos(rad) * radius / scaleFactor,
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    void OnDisable()
    {
        UIModalGate.Release(this);
    }
}
