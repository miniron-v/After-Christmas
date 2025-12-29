using System.Collections.Generic;
using UnityEngine;

public class PlayerColliderHandler : MonoBehaviour
{
    [SerializeField] private LayerMask itemLayerMask;
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private BoxCollider glowTrigger;
    private GameObject player;

    // 트리거 안에 들어온 Glowable 관리
    private readonly List<IGlowable> glowables = new();
    private readonly Dictionary<IGlowable, Transform> glowableTransforms = new();
    private readonly Dictionary<IGlowable, float> glowStartDistances = new();

    private void OnTriggerEnter(Collider other)
    {
        if (glowTrigger == null || !glowTrigger.enabled)
            return;

        if ((itemLayerMask.value & (1 << other.gameObject.layer)) == 0)
            return;

        IGlowable glowable = other.GetComponentInParent<IGlowable>();
        if (glowable != null && !glowables.Contains(glowable))
        {
            float startDist = Vector3.Distance(transform.position, other.transform.position);

            Debug.Log(
                $"[Glow Enter]\n" +
                $"- Glowable : {other.GetComponentInParent<MonoBehaviour>().gameObject.name}\n" +
                $"- Start Distance : {startDist}"
            );

            glowables.Add(glowable);
            glowableTransforms[glowable] = other.transform;
            glowStartDistances[glowable] = startDist;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (glowTrigger == null || !glowTrigger.enabled)
            return;

        IGlowable glowable = other.GetComponentInParent<IGlowable>();
        if (glowable != null)
        {
            Debug.Log($"[Glow Exit] {other.name}");

            glowable.SetGlowAmount(0f);
            glowables.Remove(glowable);
            glowableTransforms.Remove(glowable);
            glowStartDistances.Remove(glowable);
        }
    }

    private void Update()
    {
        if (!PlayerStateManager.Instance.IsPlayerControllable())
            return;

        UpdateGlowAmounts();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void UpdateGlowAmounts()
    {
        foreach (var glowable in glowables)
        {
            if (!glowableTransforms.TryGetValue(glowable, out Transform target))
                continue;

            if (!glowStartDistances.TryGetValue(glowable, out float startDist))
                continue;

            float currentDist = Vector3.Distance(transform.position, target.position);

            // 트리거 진입 시 거리 기준 정규화
            float t = 1f - (currentDist / startDist);

            // 0 ~ 2
            float glowValue = Mathf.Clamp(t * 2f, 0f, 2f);
            glowable.SetGlowAmount(glowValue);
        }
    }

    private void TryInteract()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, itemLayerMask);

        if (hits.Length == 0)
            return;

        Collider closest = null;
        float closestDist = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = hit;
            }
        }

        if (closest != null)
        {
            IInteractable interactable = closest.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(gameObject);
            }
        }
    }
}
