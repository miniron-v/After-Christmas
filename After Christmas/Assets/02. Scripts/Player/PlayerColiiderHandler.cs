using System.Collections.Generic;
using UnityEngine;

public class PlayerColliderHandler : MonoBehaviour
{
    [SerializeField] private LayerMask itemLayerMask;
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private BoxCollider glowTrigger;

    // 트리거 안에 들어온 Glowable 관리
    private readonly List<IGlowable> glowables = new();
    private readonly Dictionary<IGlowable, Transform> glowableTransforms = new();
    private readonly Dictionary<IGlowable, float> glowStartDistances = new();

    private Vector3 lastPosition;
    [SerializeField] private float teleportThreshold = 5f;

    private bool canInteract = true;

    private void OnTriggerEnter(Collider other)
    {
        if (glowTrigger == null || !glowTrigger.enabled)
            return;

        if ((itemLayerMask.value & (1 << other.gameObject.layer)) == 0)
            return;

        IGlowable glowable = other.GetComponentInParent<IGlowable>();
        if (glowable != null && !glowables.Contains(glowable))
        {
            Vector3 targetPosition = new Vector3(other.transform.position.x, 0, other.transform.position.z);
            float startDist = Vector3.Distance(transform.position, targetPosition);

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

    public void OnGameStateChanged(GameState oldState, GameState newState)
    {
        canInteract = (newState == GameState.Play);
    }

    private void ResetGlowables()
    {
        foreach (var glowable in glowables)
        {
            glowable?.SetGlowAmount(0f);
        }
        glowables.Clear();
        glowableTransforms.Clear();
        glowStartDistances.Clear();
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, lastPosition) > teleportThreshold)
        {
            ResetGlowables();
        }

        lastPosition = transform.position;

        if (!canInteract)
        {   
            return;
        }

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

            Vector3 targetPosition = new Vector3(target.position.x, 0, target.position.z);
            float currentDist = Vector3.Distance(transform.position, targetPosition);

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
