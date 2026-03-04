using UnityEngine;
// Add the correct using directive for PlayerMovement if it exists in another namespace
// using YourNamespaceForPlayerMovement;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField]
    private float _damageAmount;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            var HealthController = collision.gameObject.GetComponent<HealthController>();

            HealthController.TakeDamage(_damageAmount);
        }
    }
}
