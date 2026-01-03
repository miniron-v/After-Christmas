using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TempLoadingManager : MonoBehaviour
{
    public static TempLoadingManager Instance { get; private set; }

    [Header("Transition Settings")]
    public string targetSceneName = "";
    public string loadingSceneName = "LoadingScene";
    public float transitionTime = 1.0f;

    private void Awake()
    {
        // 싱글톤 및 파괴 방지 설정
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

    public void TransitionTo(string sceneName)
    {
        if(sceneName != "")
        {
            targetSceneName = sceneName;
        }
        StartCoroutine(TransitionSequence());
    }

    private IEnumerator TransitionSequence()
{
    var manager = PixelaterAndGreyScaleManager.Instance;
    if (manager == null)
    {
        Debug.LogError("Pixel Manager를 찾을 수 없습니다!");
        yield break;
    }

    manager.IsTransitioning = true;

    yield return SceneManager.LoadSceneAsync(loadingSceneName);

    if (string.IsNullOrEmpty(targetSceneName))
    {
        Debug.LogError("타겟 씬 이름이 비어있습니다!");
        yield break;
    }

    AsyncOperation op = SceneManager.LoadSceneAsync(targetSceneName);
    op.allowSceneActivation = false;

    while (op.progress < 0.9f)
    {
        yield return null;
    }

    manager.targetPixelSize = 200;
    manager.targetGreyscale = 1f;

    op.allowSceneActivation = true;
    while (!op.isDone) yield return null;

    yield return StartCoroutine(FadeEffect(200, 1, 1f, 0f, manager));
    
    manager.IsTransitioning = false;
}

    private IEnumerator FadeEffect(int startPix, int endPix, float startGrey, float endGrey, PixelaterAndGreyScaleManager manager)
    {
        float elapsed = 0f;
        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionTime;

            manager.targetPixelSize = (int)Mathf.Lerp(startPix, endPix, t);
            manager.targetGreyscale = Mathf.Lerp(startGrey, endGrey, t);
            yield return null;
        }
        manager.targetPixelSize = endPix;
        manager.targetGreyscale = endGrey;
    }
}