using UnityEngine;

public class BotAI : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followDistance = 2.5f;
    [SerializeField] private float speed = 3f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        FollowPlayer();
    }

    void FollowPlayer()
    {
        if (player == null) return;

        Vector2 direction = player.position - transform.position;
        float distance = direction.magnitude;

        if (distance > followDistance)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
