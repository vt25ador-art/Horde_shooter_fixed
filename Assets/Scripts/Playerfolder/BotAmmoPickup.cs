using UnityEngine;

public class BotAmmoPickup : MonoBehaviour
{
    [SerializeField] private int ammoAmount = 60;

    private void OnTriggerEnter2D(Collider2D other)
    {
        BotShoot botShoot = other.GetComponent<BotShoot>();

        if (botShoot == null)
            return;

        botShoot.AddReserveAmmo(ammoAmount);
    }
}