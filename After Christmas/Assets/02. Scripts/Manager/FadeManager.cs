using UnityEngine;
using DG.Tweening;
using System;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [SerializeField] private CanvasGroup fadeCanvasGroup; // CanvasGroup on FadeCanvas
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        if (fadeCanvasGroup == null)
            fadeCanvasGroup = GetComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
    }

    /// <summary>
    /// 페이드 아웃 후 콜백 실행
    /// </summary>
    public void FadeOut(Action onComplete = null)
    {
        Debug.Log("[FadeManager] FadeOut 시작");
        fadeCanvasGroup.blocksRaycasts = true; // 입력 차단
        fadeCanvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.InQuad).OnComplete(() =>
        {
            Debug.Log("[FadeManager] FadeOut 완료");
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// 페이드 인 후 콜백 실행
    /// </summary>
    public void FadeIn(Action onComplete = null)
    {
        Debug.Log("[FadeManager] FadeIn 시작");
        fadeCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            fadeCanvasGroup.blocksRaycasts = false; // 입력 허용
            Debug.Log("[FadeManager] FadeIn 완료");
            onComplete?.Invoke();
        });
    }
}
