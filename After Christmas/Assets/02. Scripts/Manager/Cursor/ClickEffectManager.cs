using UnityEngine;

public class CursorClickEffectManager : MonoBehaviour
{
    [Header("Particle")]
    [SerializeField] private ParticleSystem clickParticlePrefab;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float worldDepth = 5f;

    private CursorShapeManager cursorShape;

    private void Start()
    {
        cursorShape = CursorShapeManager.Instance;
        if (targetCamera == null)
            targetCamera = Camera.main;
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
        if (clickParticlePrefab == null)
            return;

        Vector2 mousePos = Input.mousePosition;
        Vector2 hotspot = cursorShape.GetCurrentHotspot();

        // 커서 핫스팟 반영
        Vector2 correctedScreenPos = mousePos - hotspot;

        Vector3 worldPos = targetCamera.ScreenToWorldPoint(
            new Vector3(correctedScreenPos.x, correctedScreenPos.y, worldDepth)
        );

        ParticleSystem ps = Instantiate(clickParticlePrefab, worldPos, Quaternion.identity);
        ps.Play();

        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    }
}
