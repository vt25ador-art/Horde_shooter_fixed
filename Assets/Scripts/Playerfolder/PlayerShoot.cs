using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class PlayerShoot : MonoBehaviour
{
    public enum WeaponMode { Pistol, Rifle, Shotgun }

    [Serializable]
    public struct WeaponSettings
    {
        [Header("Ballistics")]
        public float bulletSpeed;
        public float timeBetweenShots;
        public float spawnOffset;
        public float bulletLifetime;

        [Header("Ammo / Reload")]
        public int magazineSize;
        public int reserveAmmo;
        public float reloadTime;
        public bool infiniteAmmo;
        public bool autoReloadOnEmpty;

        [Header("Shotgun only")]
        public int pellets;         // antal projektiler per skott
        public float spreadDegrees; // total spridning i grader
    }

    [Serializable]
    private struct AmmoState
    {
        public int mag;
        public int reserve;
    }

    [Header("References")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _GunOffset;

    [Header("Weapon Select (Inspector)")]
    [SerializeField] private WeaponMode _weapon = WeaponMode.Rifle;

    [Header("Weapon Settings")]
    [SerializeField] private WeaponSettings _pistol;
    [SerializeField] private WeaponSettings _rifle;
    [SerializeField] private WeaponSettings _shotgun;

    [Header("Pool")]
    [SerializeField] private int _poolDefaultCapacity = 32;
    [SerializeField] private int _poolMaxSize = 256;

    [Header("Switching")]
    [SerializeField] private bool _blockSwitchWhileReloading = true;
    [SerializeField] private float _scrollDeadzone = 0.01f;

    private ObjectPool<Bullet> _pool;

    private Transform _spawnT;
    private Collider2D _shooterCol;

    // Fire state
    private bool _singleShot;
    private float _nextShotTime;
    private bool _prevMousePressed;

    // Weapon order + per-weapon ammo
    private WeaponMode[] _weaponOrder;
    private AmmoState[] _ammoByWeapon;
    private int _currentWeaponIndex;

    // Current ammo (loaded from state)
    private int _ammoInMag;
    private int _reserveAmmo;

    // Reload state
    private bool _isReloading;
    private Coroutine _reloadRoutine;

    // Inspector live switch support
    private WeaponMode _lastWeapon;

    // Public getters for UI
    public int AmmoInMag => _ammoInMag;
    public int ReserveAmmo => _reserveAmmo;
    public bool IsReloading => _isReloading;
    public string WeaponName => _weapon.ToString();

    private WeaponSettings Current => GetSettings(_weapon);

    private void Awake()
    {
        _spawnT = (_GunOffset != null) ? _GunOffset : transform;
        _shooterCol = GetComponent<Collider2D>();

        if (_bulletPrefab == null)
            Debug.LogError("PlayerShoot: Bullet prefab is not assigned.");

        _pool = new ObjectPool<Bullet>(
            createFunc: CreateBullet,
            actionOnGet: OnGetBullet,
            actionOnRelease: OnReleaseBullet,
            actionOnDestroy: OnDestroyBullet,
            collectionCheck: false,
            defaultCapacity: _poolDefaultCapacity,
            maxSize: _poolMaxSize
        );

        // Build weapon order + init ammo states
        _weaponOrder = (WeaponMode[])Enum.GetValues(typeof(WeaponMode));
        _ammoByWeapon = new AmmoState[_weaponOrder.Length];

        for (int i = 0; i < _weaponOrder.Length; i++)
        {
            var ws = GetSettings(_weaponOrder[i]);
            _ammoByWeapon[i] = new AmmoState
            {
                mag = Mathf.Max(0, ws.magazineSize),
                reserve = Mathf.Max(0, ws.reserveAmmo)
            };
        }

        _currentWeaponIndex = Array.IndexOf(_weaponOrder, _weapon);
        if (_currentWeaponIndex < 0) _currentWeaponIndex = 0;

        _lastWeapon = _weapon;

        // Load ammo for starting weapon
        LoadAmmoFromState();
        ClampAmmoToCurrentSettings();
    }

    private void Update()
    {
        // 1) Weapon switch FIRST (must be before any early returns)
        HandleScrollWeaponSwitch();

        // 2) Reload input
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            StartReload(manual: true);

        // 3) Fire input (mouse)
        bool isMousePressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
        if (isMousePressed && !_prevMousePressed) _singleShot = true;
        _prevMousePressed = isMousePressed;

        bool firePressed = isMousePressed;

        if (!firePressed && !_singleShot) return;
        if (Time.time < _nextShotTime) return;
        if (_isReloading) return;

        var w = Current;

        // No ammo?
        if (_ammoInMag <= 0)
        {
            if (w.autoReloadOnEmpty) StartReload(manual: false);
            return;
        }

        ShootOnce(w);

        _nextShotTime = Time.time + w.timeBetweenShots;
        _singleShot = false;

        // Auto reload if emptied
        if (_ammoInMag <= 0 && w.autoReloadOnEmpty)
            StartReload(manual: false);
    }

    private void HandleScrollWeaponSwitch()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) <= _scrollDeadzone) return;

        if (_blockSwitchWhileReloading && _isReloading) return;

        if (scroll > 0f) SwitchWeapon(+1);
        else if (scroll < 0f) SwitchWeapon(-1);
    }

    private void SwitchWeapon(int direction)
    {
        if (_weaponOrder == null || _weaponOrder.Length == 0) return;

        // Save current weapon ammo
        SaveAmmoToState();

        _currentWeaponIndex += direction;
        if (_currentWeaponIndex < 0) _currentWeaponIndex = _weaponOrder.Length - 1;
        if (_currentWeaponIndex >= _weaponOrder.Length) _currentWeaponIndex = 0;

        ApplyWeapon(_weaponOrder[_currentWeaponIndex], resetAmmo: false);
    }

    public void ApplyWeapon(WeaponMode mode, bool resetAmmo)
    {
        // Save current weapon ammo before switching (if initialized)
        SaveAmmoToState();

        _weapon = mode;
        _currentWeaponIndex = (_weaponOrder != null) ? Array.IndexOf(_weaponOrder, _weapon) : 0;
        if (_currentWeaponIndex < 0) _currentWeaponIndex = 0;

        // Stop reload on switch
        if (_reloadRoutine != null) StopCoroutine(_reloadRoutine);
        _reloadRoutine = null;
        _isReloading = false;

        if (resetAmmo)
        {
            // Reset this weapon's stored ammo from settings
            var ws = GetSettings(_weapon);
            _ammoByWeapon[_currentWeaponIndex] = new AmmoState
            {
                mag = Mathf.Max(0, ws.magazineSize),
                reserve = Mathf.Max(0, ws.reserveAmmo)
            };
        }

        // Load ammo for new weapon
        LoadAmmoFromState();
        ClampAmmoToCurrentSettings();

        _lastWeapon = _weapon;
    }

    private void ClampAmmoToCurrentSettings()
    {
        var w = Current;
        _ammoInMag = Mathf.Clamp(_ammoInMag, 0, Mathf.Max(0, w.magazineSize));
        _reserveAmmo = Mathf.Max(0, _reserveAmmo);

        // If infinite ammo, keep mag full (nice for UI/feel)
        if (w.infiniteAmmo)
            _ammoInMag = Mathf.Max(_ammoInMag, Mathf.Max(0, w.magazineSize));

        SaveAmmoToState();
    }

    private void ShootOnce(WeaponSettings w)
    {
        if (_pool == null)
        {
            Debug.LogError("PlayerShoot: Bullet pool not initialized.");
            return;
        }

        Vector3 baseSpawnPos = _spawnT.position + _spawnT.up * w.spawnOffset;

        int shots = (_weapon == WeaponMode.Shotgun) ? Mathf.Max(1, w.pellets) : 1;

        for (int i = 0; i < shots; i++)
        {
            float angle = 0f;

            if (_weapon == WeaponMode.Shotgun && w.spreadDegrees > 0f)
            {
                angle = UnityEngine.Random.Range(-w.spreadDegrees * 0.5f, w.spreadDegrees * 0.5f);
            }

            Quaternion rot = _spawnT.rotation * Quaternion.Euler(0f, 0f, angle);

            Bullet bullet = _pool.Get();
            if (bullet == null) return;

            bullet.transform.SetPositionAndRotation(baseSpawnPos, rot);

            Vector2 vel = (Vector2)(rot * Vector3.up) * w.bulletSpeed;
            bullet.Fire(vel, w.bulletLifetime, _shooterCol);
        }

        // Consume ammo: 1 per trigger pull (även shotgun)
        if (!w.infiniteAmmo)
            _ammoInMag = Mathf.Max(0, _ammoInMag - 1);

        SaveAmmoToState();
    }

    private void StartReload(bool manual)
    {
        var w = Current;
        if (_isReloading) return;

        // Infinite ammo: bara fyll mag direkt (eller ignorera)
        if (w.infiniteAmmo)
        {
            _ammoInMag = Mathf.Max(0, w.magazineSize);
            SaveAmmoToState();
            return;
        }

        // If magazine already full, ignore manual reload
        if (manual && _ammoInMag >= w.magazineSize) return;

        // If no reserve ammo and mag empty -> can't reload
        if (_reserveAmmo <= 0 && _ammoInMag == 0) return;

        _reloadRoutine = StartCoroutine(ReloadCoroutine(w));
    }

    private IEnumerator ReloadCoroutine(WeaponSettings w)
    {
        _isReloading = true;

        yield return new WaitForSeconds(w.reloadTime);

        int needed = w.magazineSize - _ammoInMag;
        if (needed > 0)
        {
            int taken = Mathf.Min(needed, _reserveAmmo);
            _ammoInMag += taken;
            _reserveAmmo -= taken;
        }

        SaveAmmoToState();

        _isReloading = false;
        _reloadRoutine = null;
    }

    // ---------- Ammo state helpers ----------

    private void SaveAmmoToState()
    {
        if (_ammoByWeapon == null || _ammoByWeapon.Length == 0) return;
        if (_currentWeaponIndex < 0 || _currentWeaponIndex >= _ammoByWeapon.Length) return;

        _ammoByWeapon[_currentWeaponIndex] = new AmmoState
        {
            mag = _ammoInMag,
            reserve = _reserveAmmo
        };
    }

    private void LoadAmmoFromState()
    {
        if (_ammoByWeapon == null || _ammoByWeapon.Length == 0) return;
        if (_currentWeaponIndex < 0 || _currentWeaponIndex >= _ammoByWeapon.Length) return;

        var s = _ammoByWeapon[_currentWeaponIndex];
        _ammoInMag = s.mag;
        _reserveAmmo = s.reserve;
    }

    // ---------- Settings helper ----------

    private WeaponSettings GetSettings(WeaponMode mode) => mode switch
    {
        WeaponMode.Pistol => _pistol,
        WeaponMode.Rifle => _rifle,
        WeaponMode.Shotgun => _shotgun,
        _ => _rifle
    };

    // ---------- Pool ----------

    private Bullet CreateBullet()
    {
        GameObject bulletObj = Instantiate(_bulletPrefab);
        Bullet b = bulletObj.GetComponent<Bullet>();
        if (b == null)
        {
            Debug.LogError("PlayerShoot: Bullet prefab does not have a Bullet component.");
            return null;
        }

        b.Init(ReturnToPool);
        b.gameObject.SetActive(false);
        return b;
    }

    private void OnGetBullet(Bullet b)
    {
        if (b == null) return;
        b.gameObject.SetActive(true);
    }

    private void OnReleaseBullet(Bullet b)
    {
        if (b == null) return;
        b.gameObject.SetActive(false);
    }

    private void OnDestroyBullet(Bullet b)
    {
        if (b == null) return;
        Destroy(b.gameObject);
    }

    private void ReturnToPool(Bullet b)
    {
        if (_pool == null || b == null) return;
        _pool.Release(b);
    }


    public void AddAmmo(int amount)
    {
       var w = Current;
        if (w.infiniteAmmo) return;

        int maxreserve = w.reserveAmmo;

        if (_reserveAmmo >= maxreserve) return;

        _reserveAmmo = Mathf.Min(_reserveAmmo + amount, maxreserve);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Live switch in inspector during Play Mode
        if (!Application.isPlaying) return;

        if (_weapon != _lastWeapon)
        {
            // Ensure arrays exist (in case OnValidate fires weirdly)
            if (_weaponOrder == null || _weaponOrder.Length == 0)
                _weaponOrder = (WeaponMode[])Enum.GetValues(typeof(WeaponMode));

            if (_ammoByWeapon == null || _ammoByWeapon.Length != _weaponOrder.Length)
            {
                _ammoByWeapon = new AmmoState[_weaponOrder.Length];
                for (int i = 0; i < _weaponOrder.Length; i++)
                {
                    var ws = GetSettings(_weaponOrder[i]);
                    _ammoByWeapon[i] = new AmmoState { mag = ws.magazineSize, reserve = ws.reserveAmmo };
                }
            }

            ApplyWeapon(_weapon, resetAmmo: false);
        }
    }

    [ContextMenu("Refill All Weapons From Settings")]
    private void RefillAllWeaponsFromSettings()
    {
        if (_weaponOrder == null || _weaponOrder.Length == 0)
            _weaponOrder = (WeaponMode[])Enum.GetValues(typeof(WeaponMode));

        _ammoByWeapon = new AmmoState[_weaponOrder.Length];

        for (int i = 0; i < _weaponOrder.Length; i++)
        {
            var ws = GetSettings(_weaponOrder[i]);
            _ammoByWeapon[i] = new AmmoState
            {
                mag = Mathf.Max(0, ws.magazineSize),
                reserve = Mathf.Max(0, ws.reserveAmmo)
            };
        }

        _currentWeaponIndex = Array.IndexOf(_weaponOrder, _weapon);
        if (_currentWeaponIndex < 0) _currentWeaponIndex = 0;

        LoadAmmoFromState();
        ClampAmmoToCurrentSettings();
    }
#endif
}
