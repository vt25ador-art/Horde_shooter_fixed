using UnityEngine;

public class RevivePlayer : MonoBehaviour
{
    [SerializeField] private float reviveTime = 3f;
    private float reviveTimer;

    void OnTriggerStay2D(Collider2D other)
    {
        HealthController health = other.GetComponent<HealthController>();

        if (health != null && health.isDowned)
        {
            reviveTimer += Time.deltaTime;

            if (reviveTimer >= reviveTime)
            {
                health.Revive(30f);
                reviveTimer = 0;
            }
        }
    }
}

