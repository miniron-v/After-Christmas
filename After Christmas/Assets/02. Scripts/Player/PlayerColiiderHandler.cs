using UnityEngine;
using static UnityEditor.Progress;

public class PlayerColliderHandler : MonoBehaviour
{
    [SerializeField] private LayerMask itemLayerMask;
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private LayerMask interactableMask;


    private void OnTriggerEnter(Collider other)
    {
        if ((itemLayerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            Item itemScript = other.GetComponent<Item>();
            if (itemScript != null)
            {
                itemScript.Glow(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((itemLayerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            Item itemScript = other.GetComponent<Item>();
            if (itemScript != null)
            {
                itemScript.Glow(false);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, interactableMask);

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
                ITeleportable interactable = closest.GetComponent<ITeleportable>();
                if (interactable != null)
                {
                    interactable.Teleport();
                }
            }
        }
    }
}