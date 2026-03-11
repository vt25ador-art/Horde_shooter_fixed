using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class BotShoot : MonoBehaviour
{
    public enum WeaponMode { Pistol, Rifle, Shotgun }

    [System.Serializable]
    public struct WeaponSettings
    {
        public float bulletSpeed;
        public float timeBetweenShots;
        public float spawnOffset;
        public float bulletLifetime;

        public int magazineSize;
        public int reserveAmmo;
        public float reloadTime;
        public bool infiniteAmmo;
        public bool autoReloadOnEmpty;

        public int pellets;
        public float spreadDegrees;
    }

    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform gunOffset;

    [Header("Weapon")]
    [SerializeField] private WeaponMode weapon = WeaponMode.Rifle;
    [SerializeField] private WeaponSettings pistol;
    [SerializeField] private WeaponSettings rifle;
    [SerializeField] private WeaponSettings shotgun;

    [Header("Pool")]
    [SerializeField] private int poolDefaultCapacity = 32;
    [SerializeField] private int poolMaxSize = 256;

    private ObjectPool<Bullet> pool;
    private Transform spawnT;
    private Collider2D shooterCol;

    private int ammoInMag;
    private int reserveAmmo;
    private bool isReloading;
    private float nextShotTime;
    private Coroutine reloadRoutine;

    private WeaponSettings Current => weapon switch
    {
        WeaponMode.Pistol => pistol,
        WeaponMode.Rifle => rifle,
        WeaponMode.Shotgun => shotgun,
        _ => rifle
    };

    private void Awake()
    {
        spawnT = gunOffset ? gunOffset : transform;
        shooterCol = GetComponent<Collider2D>();

        pool = new ObjectPool<Bullet>(
            CreateBullet,
            b => { if (b) b.gameObject.SetActive(true); },
            b => { if (b) b.gameObject.SetActive(false); },
            b => { if (b) Destroy(b.gameObject); },
            false,
            poolDefaultCapacity,
            poolMaxSize
        );

        var w = Current;
        ammoInMag = w.magazineSize;
        reserveAmmo = w.reserveAmmo;
    }

    public void BotFireAt(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.001f) return;
        if (isReloading) return;
        if (Time.time < nextShotTime) return;

        var w = Current;

        if (ammoInMag <= 0)
        {
            if (w.autoReloadOnEmpty)
                StartReload();
            return;
        }

        spawnT.up = dir.normalized;
        ShootOnce(w);

        nextShotTime = Time.time + w.timeBetweenShots;

        if (ammoInMag <= 0 && w.autoReloadOnEmpty)
            StartReload();
    }

    private void ShootOnce(WeaponSettings w)
    {
        Vector3 spawnPos = spawnT.position + spawnT.up * w.spawnOffset;
        int shots = weapon == WeaponMode.Shotgun ? Mathf.Max(1, w.pellets) : 1;

        for (int i = 0; i < shots; i++)
        {
            float angle = 0f;

            if (weapon == WeaponMode.Shotgun)
                angle = Random.Range(-w.spreadDegrees * 0.5f, w.spreadDegrees * 0.5f);

            Quaternion rot = spawnT.rotation * Quaternion.Euler(0f, 0f, angle);

            Bullet bullet = pool.Get();
            if (!bullet) return;

            bullet.transform.SetPositionAndRotation(spawnPos, rot);
            Vector2 vel = (Vector2)(rot * Vector3.up) * w.bulletSpeed;
            bullet.Fire(vel, w.bulletLifetime, shooterCol);
        }

        if (!w.infiniteAmmo)
            ammoInMag = Mathf.Max(0, ammoInMag - 1);
    }

    private void StartReload()
    {
        if (isReloading) return;
        reloadRoutine = StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;

        var w = Current;
        yield return new WaitForSeconds(w.reloadTime);

        int needed = w.magazineSize - ammoInMag;
        if (needed > 0)
        {
            if (w.infiniteAmmo)
            {
                ammoInMag = w.magazineSize;
            }
            else
            {
                int taken = Mathf.Min(needed, reserveAmmo);
                ammoInMag += taken;
                reserveAmmo -= taken;
            }
        }

        isReloading = false;
        reloadRoutine = null;
    }

    private Bullet CreateBullet()
    {
        GameObject obj = Instantiate(bulletPrefab);
        Bullet b = obj.GetComponent<Bullet>();

        if (!b)
        {
            Debug.LogError("BotShoot: Bullet prefab missing Bullet component.");
            return null;
        }

        b.Init(ReturnToPool);
        b.gameObject.SetActive(false);
        return b;
    }

    private void ReturnToPool(Bullet b)
    {
        if (pool == null || b == null) return;
        pool.Release(b);
    }
}
