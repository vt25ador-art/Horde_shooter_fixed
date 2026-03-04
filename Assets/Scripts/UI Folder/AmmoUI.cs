using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private TMP_Text ammoText;

    private void Awake()
    {
        if (playerShoot == null)
            playerShoot = FindAnyObjectByType<PlayerShoot>();

        if (ammoText == null)
            ammoText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (playerShoot == null || ammoText == null) return;

        string reload = playerShoot.IsReloading ? "  RELOADING" : "";
        ammoText.text = $"{playerShoot.WeaponName}  {playerShoot.AmmoInMag}/{playerShoot.ReserveAmmo}{reload}";
    }
}