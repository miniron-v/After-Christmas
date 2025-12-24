using UnityEngine;

public class PixelaterAndGreyScaleManager : MonoBehaviour
{
    public static PixelaterAndGreyScaleManager Instance { get; private set; }

    [Header("Feature Reference")]
    public PixelaterAndGreyScaleFeature pixelFeature;

    [Header("Current Status")]
    [Range(1, 200)] public int targetPixelSize = 1;
    [Range(0, 1)]   public float targetGreyscale = 0f;

    [Header("Settings")]
    public bool resetOnStart = true;

    [HideInInspector]
    public bool IsTransitioning { get; set; }

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
            return;
        }

        // ScriptableObject 기반 데이터이므로 초기화 필요
        if (resetOnStart && pixelFeature != null)
        {
            pixelFeature.settings.pixelate = 1;
            pixelFeature.settings.greyscale = 0f;
            pixelFeature.SetActive(true);
            
            targetPixelSize = 1;
            targetGreyscale = 0f;
        }
    }

    private void Update()
    {
        if (pixelFeature != null)
        {
            pixelFeature.settings.pixelate = targetPixelSize;
            pixelFeature.settings.greyscale = targetGreyscale;
        }
    }
}