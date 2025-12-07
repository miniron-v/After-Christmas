using UnityEngine;
using UnityEngine.SceneManagement;

public class TempSceneMoveManager : MonoBehaviour
{
    public static TempSceneMoveManager Instance { get; private set; }

    [Header("Assign scene names")]
    public string scene1Name;
    public string scene2Name;
    public string scene3Name;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // 키 입력 처리 (씬 이동)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            LoadScene(scene1Name);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            LoadScene(scene2Name);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            LoadScene(scene3Name);
        }
    }

    private void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name not assigned!");
        }
    }
}
