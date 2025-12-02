using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        if (fadeImage == null)
            fadeImage = GetComponent<Image>();

        // 시작 시 투명
        SetAlpha(0);
        fadeImage.raycastTarget = false;
    }

    private void SetAlpha(float a)
    {
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }

    public void FadeOut(Action onComplete = null)
    {
        fadeImage.raycastTarget = true;

        fadeImage.DOFade(1f, fadeDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => onComplete?.Invoke());
    }

    public void FadeIn(Action onComplete = null)
    {
        fadeImage.DOFade(0f, fadeDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                fadeImage.raycastTarget = false;
                onComplete?.Invoke();
            });
    }
}
