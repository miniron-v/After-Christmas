using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ManualUI : MonoBehaviour
{
    [Header("Root UI")]
    [SerializeField] private GameObject manualUI;

    [Header("Timing")]
    [SerializeField] private float minimumActiveTime = 3f;

    [Header("UI")]
    [SerializeField] private Image panelImage;
    [SerializeField] private float keySize = 45f;

    [Header("Fade Animation")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Glow Fade")]
    [SerializeField] private float glowDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private Color glowColor = new(1.3f, 1.3f, 1.3f, 1f);

    // runtime
    private List<KeyCode> targetKeys;
    private bool actionCompleted;
    private float startTime;
    private Coroutine routine;

    void Start()
    {
        canvasGroup.alpha = 0f;
        manualUI.SetActive(false);
    }

    void Update()
    {
        if (!manualUI.activeSelf || actionCompleted || targetKeys == null)
            return;

        // 하나라도 눌리면 완료
        foreach (var key in targetKeys)
        {
            if (Input.GetKeyDown(key))
            {
                Debug.Log($"[{key}] 입력으로 조작 완료");
                actionCompleted = true;
                break;
            }
        }
    }

    // 조작법 알림 시작
    public void ShowManual(ManualActionSO action)
    {
        if (action == null)
        {
            Debug.LogWarning("[ManualUI] 액션이 존재하지 않음");
            return;
        }

        if (routine != null)
            StopCoroutine(routine);

        targetKeys = action.keys;
        actionCompleted = false;
        startTime = Time.realtimeSinceStartup;

        manualUI.SetActive(true);

        panelImage.sprite = action.sprite;

        RectTransform rt = panelImage.rectTransform;
        rt.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            keySize * action.rowKeyCount
        );

        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        Debug.Log($"[ManualUI] 조작법 표시 ({action.actionName})");

        StartCoroutine(FadeIn());
        routine = StartCoroutine(WaitAndHide());
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        canvasGroup.alpha = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator GlowAndFadeOut()
    {
        // 🔹 1단계: 번쩍 (밝아졌다가 원래대로)
        float t = 0f;
        Color originalColor = panelImage.color;

        while (t < glowDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / glowDuration;

            // 가운데에서 가장 밝게
            float glowStrength = Mathf.Sin(p * Mathf.PI);
            panelImage.color = Color.Lerp(
                originalColor,
                glowColor,
                glowStrength
            );

            yield return null;
        }

        panelImage.color = originalColor;

        // 🔹 2단계: 스르륵 사라짐
        t = 0f;
        canvasGroup.alpha = 1f;

        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    // 종료 대기
    private IEnumerator WaitAndHide()
    {
        yield return new WaitUntil(() => actionCompleted);

        float elapsed = Time.realtimeSinceStartup - startTime;
        if (elapsed < minimumActiveTime)
        {
            yield return new WaitForSecondsRealtime(minimumActiveTime - elapsed);
        }

        yield return StartCoroutine(GlowAndFadeOut());

        manualUI.SetActive(false);
        Debug.Log("[ManualUI] 조작법 종료");

        routine = null;
    }
}