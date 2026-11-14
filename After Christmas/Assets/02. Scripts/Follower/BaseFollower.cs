using UnityEngine;

public abstract class BaseFollower : MonoBehaviour
{
    [SerializeField] protected Transform target;
    [SerializeField] protected float smoothTime = 0.15f;

    protected Vector3 currentVelocity = Vector3.zero;

    protected abstract void FollowLogic();

    protected virtual void FixedUpdate()
    {
        FollowLogic();
    }
}