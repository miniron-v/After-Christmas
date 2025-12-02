using UnityEngine;
using static UnityEditor.Progress;

public class PlayerColliderHandler : MonoBehaviour
{
    [SerializeField] private LayerMask itemLayerMask;
    [SerializeField] private float interactRange = 2f;


    private void OnTriggerEnter(Collider other)
    {
        if ((itemLayerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            IGlowable glowObject = other.GetComponent<IGlowable>();
            if (glowObject != null)
            {
                glowObject.Glow(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((itemLayerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            IGlowable glowObject = other.GetComponent<IGlowable>();
            if (glowObject != null)
            {
                glowObject.Glow(false);
            }
        }
    }

    private void Update()
    {
        if (!PlayerStateManager.Instance.IsPlayerControllable())
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }


    private void TryInteract()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, itemLayerMask);

        if (hits.Length > 0)
        {
            // 가장 가까운 오브젝트 찾기
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
                // IInteractable 구현체 찾기
                IInteractable interactable = closest.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
    }
}