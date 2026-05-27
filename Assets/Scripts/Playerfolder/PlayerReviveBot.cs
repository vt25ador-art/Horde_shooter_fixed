using UnityEngine;

public class PlayerReviveBot : MonoBehaviour
{
    [Header("Revive")]
    [SerializeField] private float reviveDistance = 1.5f;
    [SerializeField] private float reviveTime = 5f;
    [SerializeField] private float reviveHealth = 35f;
    [SerializeField] private KeyCode reviveKey = KeyCode.E;

    [Header("Search")]
    [SerializeField] private LayerMask botLayer;

    private HealthController currentBot;
    private float reviveTimer;

    private void Update()
    {
        FindDownedBot();

        if (currentBot == null)
        {
            reviveTimer = 0f;
            return;
        }

        if (Input.GetKey(reviveKey))
        {
            reviveTimer += Time.deltaTime;

            if (reviveTimer >= reviveTime)
            {
                currentBot.Revive(reviveHealth);
                reviveTimer = 0f;
                currentBot = null;
            }
        }
        else
        {
            reviveTimer = 0f;
        }
    }

    private void FindDownedBot()
    {
        currentBot = null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, reviveDistance, botLayer);

        for (int i = 0; i < hits.Length; i++)
        {
            HealthController health = hits[i].GetComponent<HealthController>();

            if (health != null && health.isDowned)
            {
                currentBot = health;
                return;
            }
        }
    }
}