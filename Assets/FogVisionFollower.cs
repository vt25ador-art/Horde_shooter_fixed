using UnityEngine;

public class FogVisionFollower : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow")]
    [SerializeField] private bool followRotation = false;
    [SerializeField] private Vector3 offset;

    private void LateUpdate()
    {
        if (target == null)
            return;

        transform.position = target.position + offset;

        if (followRotation)
            transform.rotation = target.rotation;
    }
}
