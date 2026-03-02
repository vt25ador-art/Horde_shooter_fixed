using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothTime = 0.15f;

    // Valfritt: lås Y eller lås axlar
    public bool lockY = true;

    // Valfritt: world bounds (om du VILL ha gränser)
    public bool useBounds = false;
    public Vector2 minXZ;
    public Vector2 maxXZ;

    private Vector3 velocity;
    private float offsetZ;

    void Start()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogWarning("CameraFollow: Player not found.");
            return;
        }

        offsetZ = transform.position.z - player.position.z;
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 target = new Vector3(
            player.position.x,
            lockY ? transform.position.y : player.position.y,
            player.position.z + offsetZ
        );

        if (useBounds)
        {
            target.x = Mathf.Clamp(target.x, minXZ.x, maxXZ.x);
            target.z = Mathf.Clamp(target.z, minXZ.y, maxXZ.y);
        }

        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime);
    }
}
