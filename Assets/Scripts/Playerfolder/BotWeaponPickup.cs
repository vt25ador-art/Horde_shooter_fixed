using UnityEngine;

public class BotWeaponPickup : MonoBehaviour
{
    [SerializeField] private BotShoot.WeaponMode weaponToGive = BotShoot.WeaponMode.Rifle;
    [SerializeField] private bool onlyIfBetter = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        BotShoot botShoot = other.GetComponent<BotShoot>();

        if (botShoot == null)
            return;

        if (onlyIfBetter && !botShoot.IsBetterWeaponAvailable(weaponToGive))
            return;

        botShoot.EquipWeapon(weaponToGive);

        Destroy(gameObject);
    }
}