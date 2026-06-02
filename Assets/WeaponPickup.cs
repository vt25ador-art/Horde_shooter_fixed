using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private PlayerShoot.WeaponMode weaponToGive = PlayerShoot.WeaponMode.Rifle;
    [SerializeField] private bool equipImmediately = true;
    [SerializeField] private bool resetAmmoOnPickup = true;

    [Header("Rules")]
    [SerializeField] private bool destroyAfterPickup = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerShoot playerShoot = other.GetComponent<PlayerShoot>();

        if (playerShoot == null)
            return;

        playerShoot.UnlockWeapon(weaponToGive, equipImmediately, resetAmmoOnPickup);

        if (destroyAfterPickup)
            Destroy(gameObject);
    }
}