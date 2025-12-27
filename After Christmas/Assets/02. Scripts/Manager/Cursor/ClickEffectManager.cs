using UnityEngine;

public class CursorClickEffectManager : MonoBehaviour
{
    [Header("UI Particle")]
    [SerializeField] private ParticleSystem clickParticlePrefab;
    [SerializeField] private RectTransform effectRoot;
    [SerializeField] private Canvas canvas;

    [Header("Camera Scale Reference")]
    [SerializeField] private float referenceCameraSize = 8f; // 기준값

    private CursorShapeManager cursorShape;
    private Camera targetCamera;

    private void Start()
    {
        cursorShape = CursorShapeManager.Instance;

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        targetCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera
            ? canvas.worldCamera
            : Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PlayClickParticle();
        }
    }

    private void PlayClickParticle()
    {
        if (clickParticlePrefab == null || effectRoot == null || targetCamera == null)
            return;

        Vector2 mousePos = Input.mousePosition;
        Vector2 hotspot = cursorShape.GetCurrentHotspot();
        Vector2 correctedScreenPos = mousePos - hotspot;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            effectRoot,
            correctedScreenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCamera,
            out Vector2 localPos
        );

        ParticleSystem ps = Instantiate(clickParticlePrefab, effectRoot);
        ps.transform.localPosition = localPos;

        // 카메라 size 비례 스케일 보정
        float scaleFactor = targetCamera.orthographicSize / referenceCameraSize;
        ps.transform.localScale = Vector3.one * scaleFactor;

        ps.Play();
        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    }
}
