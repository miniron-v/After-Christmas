using UnityEngine;

public class CameraFollower : BaseFollower
{
    [Header("Camera Follow Settings")]
    [SerializeField] private Vector3 cameraOffset;

    private bool isFollowing = true;

    private void Awake()
    {
        if (!target)
        {
            enabled = false;
            return;
        }
    }

    public void StartFollow()
    {
        isFollowing = true;
    }

    public void StopFollow()
    {
        isFollowing = false;
        currentVelocity = Vector3.zero;
    }

    protected override void FollowLogic()
    {
        if (!isFollowing) return;

        Vector3 desiredPos = target.position + cameraOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref currentVelocity,
            smoothTime
        );
    }
}