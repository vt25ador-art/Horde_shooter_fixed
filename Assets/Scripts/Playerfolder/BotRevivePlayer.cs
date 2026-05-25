using UnityEngine;

public class BotRevivePlayer : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private HealthController playerHealth;

    [Header("Revive")]
    [SerializeField] private float reviveDistance = 1.5f;
    [SerializeField] private float reviveTime = 5f;
    [SerializeField] private float reviveHealth = 35f;

    private bool reviving;
    private float reviveTimer;

    public bool ShouldRevivePlayer
    {
        get
        {
            return playerHealth != null &&
                   playerHealth.isDowned &&
                   !playerHealth.IsDead;
        }
    }

    public Vector3 ReviveTargetPosition
    {
        get
        {
            if (playerHealth == null)
                return transform.position;

            return playerHealth.transform.position;
        }
    }

    private void Awake()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                playerHealth = player.GetComponent<HealthController>();
        }
    }

    private void Update()
    {
        if (!ShouldRevivePlayer)
        {
            CancelRevive();
            return;
        }

        float distance = Vector2.Distance(transform.position, playerHealth.transform.position);

        if (distance > reviveDistance)
        {
            CancelRevive();
            return;
        }

        ReviveTick();
    }

    private void ReviveTick()
    {
        reviving = true;
        reviveTimer += Time.deltaTime;

        Debug.Log("Bot reviving player: " + reviveTimer.ToString("0.0"));

        if (reviveTimer >= reviveTime)
        {
            playerHealth.Revive(reviveHealth);
            CancelRevive();
        }
    }

    private void CancelRevive()
    {
        reviving = false;
        reviveTimer = 0f;
    }
}
