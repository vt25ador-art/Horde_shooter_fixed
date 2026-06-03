using UnityEngine;

public class PlayerTeleportToBot : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode teleportKey = KeyCode.T;
    [SerializeField] private float holdTimeRequired = 1f;

    [Header("Target")]
    [SerializeField] private Transform bot;
    [SerializeField] private string botTag = "Bot";

    [Header("Teleport")]
    [SerializeField] private float teleportOffsetDistance = 1.5f;
    [SerializeField] private bool zeroVelocityOnTeleport = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    [Header("2D Position")]
    [SerializeField] private float fixedZPosition = -0.65f;

    private float holdTimer;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (bot == null)
        {
            GameObject foundBot = GameObject.FindGameObjectWithTag(botTag);

            if (foundBot != null)
                bot = foundBot.transform;
        }
    }

    private void Update()
    {
        if (bot == null)
            return;

        if (Input.GetKey(teleportKey))
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdTimeRequired)
            {
                TeleportToBot();
                holdTimer = 0f;
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    private void TeleportToBot()
    {
        Vector2 botPos = bot.position;

        Vector2 offsetDirection = -bot.up;

        if (offsetDirection.sqrMagnitude < 0.01f)
            offsetDirection = Vector2.down;

        Vector2 targetPos2D = botPos + offsetDirection.normalized * teleportOffsetDistance;

        Vector3 targetPos = new Vector3(
            targetPos2D.x,
            targetPos2D.y,
            fixedZPosition
        );

        if (zeroVelocityOnTeleport && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = targetPos2D;
        }

        transform.position = targetPos;

        if (debugLogs)
            Debug.Log("Player teleported to bot at Z: " + fixedZPosition);
    }




    public float GetHoldProgress()
    {
        return Mathf.Clamp01(holdTimer / holdTimeRequired);
    }
}
