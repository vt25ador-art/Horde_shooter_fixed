using UnityEngine;

public class MeleeWeaponPickup : MonoBehaviour
{
    [Header("Melee")]
    [SerializeField] private PlayerMelee.MeleeWeaponType weaponType = PlayerMelee.MeleeWeaponType.Bat;

    [Header("Pickup")]
    [SerializeField] private bool destroyAfterPickup = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerMelee playerMelee = other.GetComponent<PlayerMelee>();

        if (playerMelee == null)
            return;

        playerMelee.GiveMeleeWeapon(weaponType);

        if (destroyAfterPickup)
            Destroy(gameObject);
    }
}