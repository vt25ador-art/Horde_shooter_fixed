using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform Player;
    public float smoothspeed = 0.125f;

    float camOffsetZ;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Player == null)
        {
            Player = GameObject.FindWithTag("Player")?.transform;
        }

        if (Player == null)
        {
            Debug.LogWarning("CameraFollow: Player not found. Please assign the Player transform in the inspector or tag the player object with 'Player'.");
            camOffsetZ = transform.position.z;
            return;
        }

        camOffsetZ = transform.position.z - Player.position.z;

    }

    private void LateUpdate()
    {
        if (Player == null) return;


        Vector3 targetpos = new Vector3(Player.position.x, transform.position.y, Player.position.z + camOffsetZ);
        Vector3 smoothedPos = Vector3.Lerp(transform.position, targetpos, smoothspeed);
        transform.position = smoothedPos;

    }
}
