using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class TransitionSettings
{
    [Header("현재 씬 → 로딩씬 효과 온오프")]
    public bool useOutEffect = true;  // A -> 로딩
    [Header("로딩씬 → 타겟 씬 효과 온오프")]
    public bool useInEffect = true;   // 로딩 -> B
    [Header("현재 씬 → 로딩씬 픽셀레이트 지속시간")]
    public float outDuration = 1.0f;
    [Header("로딩씬 → 타겟 씬 픽셀레이트 지속시간")]
    public float inDuration = 1.0f;
}

public class TempLoadingManager : MonoBehaviour
{
    public static TempLoadingManager Instance { get; private set; }

    public string loadingSceneName = "LoadingScene";
    
    // 기본 설정값
    public TransitionSettings defaultSettings;

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

    // 외부에서 호출할 때 설정을 같이 넘겨줌
    public void TransitionTo(string targetScene, TransitionSettings customSettings = null)
    {
        TransitionSettings settings = customSettings ?? defaultSettings;
        StartCoroutine(TransitionSequence(targetScene, settings));
    }

    private IEnumerator TransitionSequence(string targetScene, TransitionSettings settings)
    {
        var manager = PixelaterAndGreyScaleManager.Instance;
        manager.IsTransitioning = true;

        // A -> 로딩
        if (settings.useOutEffect)
        {
            // 점진적으로 픽셀화
            yield return StartCoroutine(FadeEffect(1, 200, 0f, 1f, settings.outDuration, manager));
        }

        yield return SceneManager.LoadSceneAsync(loadingSceneName);
        
        // 잠시 대기
        yield return new WaitForSeconds(0.5f);

        // 로딩 -> B
        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f) yield return null;

        // 씬 전환 전 픽셀 셋팅 (이미 픽셀화된 상태에서 시작)
        manager.targetPixelSize = 200;
        manager.targetGreyscale = 1f;

        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        if (settings.useInEffect)
        {
            // 점진적으로 해제
            yield return StartCoroutine(FadeEffect(200, 1, 1f, 0f, settings.inDuration, manager));
        }
        else
        {
            // 효과 안 쓰면 즉시 해제
            manager.targetPixelSize = 1;
            manager.targetGreyscale = 0f;
        }

        manager.IsTransitioning = false;
    }

    private IEnumerator FadeEffect(int startPix, int endPix, float startGrey, float endGrey, float duration, PixelaterAndGreyScaleManager manager)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            manager.targetPixelSize = (int)Mathf.Lerp(startPix, endPix, t);
            manager.targetGreyscale = Mathf.Lerp(startGrey, endGrey, t);
            yield return null;
        }
        manager.targetPixelSize = endPix;
        manager.targetGreyscale = endGrey;
    }
}
