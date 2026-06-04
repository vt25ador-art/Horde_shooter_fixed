using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class PlayerShoot : MonoBehaviour
{
    public enum WeaponMode { Pistol, Rifle, Shotgun, Uzi }

    [Serializable]
    public struct WeaponSettings
    {
        [Header("Ballistics")]
        public float bulletSpeed;
        public float timeBetweenShots;
        public float spawnOffset;
        public float bulletLifetime;

        [Header("Damage")]
        public float damage;

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
    [SerializeField] private WeaponSettings _uzi;

    [Header("Pool")]
    [SerializeField] private int _poolDefaultCapacity = 32;
    [SerializeField] private int _poolMaxSize = 256;

    [Header("Switching")]
    [SerializeField] private bool _blockSwitchWhileReloading = true;
    [SerializeField] private float _scrollDeadzone = 0.01f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip pistolShootSound;
    [SerializeField] private AudioClip rifleShootSound;
    [SerializeField] private AudioClip shotgunShootSound;
    [SerializeField] private AudioClip uziShootSound;

    [SerializeField] private AudioClip pistolEquipSound;
    [SerializeField] private AudioClip rifleEquipSound;
    [SerializeField] private AudioClip shotgunEquipSound;
    [SerializeField] private AudioClip uziEquipSound;

    [Header("Audio Cooldowns")]
    [SerializeField] private float pistolSoundCooldown = 0.08f;
    [SerializeField] private float rifleSoundCooldown = 0.09f;
    [SerializeField] private float shotgunSoundCooldown = 0.25f;
    [SerializeField] private float uziSoundCooldown = 0.06f;

    [SerializeField] private float shootSoundVolume = 0.8f;
    [SerializeField] private float equipSoundVolume = 0.7f;
    [SerializeField] private float reloadSoundVolume = 0.7f;

    [Header("Unlocked Weapons")]
    [SerializeField] private WeaponMode startingWeapon = WeaponMode.Pistol;

    private bool[] _weaponUnlocked;

    private float _nextShootSoundTime;

    [SerializeField] private AudioClip reloadSound;

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

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

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
        _weaponUnlocked = new bool[_weaponOrder.Length];

        for (int i = 0; i < _weaponOrder.Length; i++)
        {
            var ws = GetSettings(_weaponOrder[i]);
            _ammoByWeapon[i] = new AmmoState
            {
                mag = Mathf.Max(0, ws.magazineSize),
                reserve = Mathf.Max(0, ws.reserveAmmo)
            };
        }

        // Börja bara med startingWeapon upplåst
        for (int i = 0; i < _weaponUnlocked.Length; i++)
            _weaponUnlocked[i] = false;

        int startIndex = Array.IndexOf(_weaponOrder, startingWeapon);
        if (startIndex < 0)
            startIndex = 0;

        _weaponUnlocked[startIndex] = true;

        _weapon = _weaponOrder[startIndex];
        _currentWeaponIndex = startIndex;
        _lastWeapon = _weapon;

        // Load ammo for starting weapon
        LoadAmmoFromState();
        ClampAmmoToCurrentSettings();
    }

    private void Update()
    {
        // 1) Weapon switch FIRST
        HandleScrollWeaponSwitch();

        // 2) Reload input
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            StartReload(manual: true);

        // 3) Fire input
        bool isMousePressed = Mouse.current != null && Mouse.current.leftButton.isPressed;

        if (isMousePressed && !_prevMousePressed)
            _singleShot = true;

        _prevMousePressed = isMousePressed;

        var w = Current;

        bool firePressed;

        // Pistol och Shotgun = semi-auto
        // Uzi och Rifle = automatic
        if (_weapon == WeaponMode.Pistol || _weapon == WeaponMode.Shotgun)
            firePressed = _singleShot;
        else
            firePressed = isMousePressed;

        if (!firePressed)
            return;

        if (Time.time < _nextShotTime)
            return;

        if (_isReloading)
            return;

        // No ammo?
        if (_ammoInMag <= 0)
        {
            if (w.autoReloadOnEmpty)
                StartReload(manual: false);

            _singleShot = false;
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
        if (_weaponOrder == null || _weaponOrder.Length == 0)
            return;

        if (_weaponUnlocked == null || _weaponUnlocked.Length != _weaponOrder.Length)
            return;

        SaveAmmoToState();

        int startIndex = _currentWeaponIndex;
        int index = _currentWeaponIndex;

        for (int i = 0; i < _weaponOrder.Length; i++)
        {
            index += direction;

            if (index < 0)
                index = _weaponOrder.Length - 1;

            if (index >= _weaponOrder.Length)
                index = 0;

            if (_weaponUnlocked[index])
            {
                _currentWeaponIndex = index;
                ApplyWeapon(_weaponOrder[_currentWeaponIndex], resetAmmo: false);
                return;
            }
        }

        _currentWeaponIndex = startIndex;
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
        PlayEquipSound();
    }

    private void PlayEquipSound()
    {
        if (audioSource == null)
            return;

        AudioClip clip = _weapon switch
        {
            WeaponMode.Pistol => pistolEquipSound,
            WeaponMode.Rifle => rifleEquipSound,
            WeaponMode.Shotgun => shotgunEquipSound,
            WeaponMode.Uzi => uziEquipSound,
            _ => null
        };

        if (clip != null)
            audioSource.PlayOneShot(clip, equipSoundVolume);
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
        PlayShootSound();

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

            // Alla vapen kan ha spread nu
            if (w.spreadDegrees > 0f)
            {
                angle = UnityEngine.Random.Range(
                    -w.spreadDegrees * 0.5f,
                     w.spreadDegrees * 0.5f
                );
            }

            Quaternion rot = _spawnT.rotation * Quaternion.Euler(0f, 0f, angle);

            Bullet bullet = _pool.Get();

            if (bullet == null)
                return;

            bullet.transform.SetPositionAndRotation(baseSpawnPos, rot);

            Vector2 vel = (Vector2)(rot * Vector3.up) * w.bulletSpeed;
            bullet.Fire(vel, w.bulletLifetime, _shooterCol);
        }

        // Consume ammo: 1 per trigger pull
        _ammoInMag = Mathf.Max(0, _ammoInMag - 1);

        SaveAmmoToState();
    }

    private void PlayShootSound()
    {
        if (audioSource == null)
            return;

        AudioClip clip = null;
        float cooldown = 0.1f;

        switch (_weapon)
        {
            case WeaponMode.Pistol:
                clip = pistolShootSound;
                cooldown = pistolSoundCooldown;
                break;

            case WeaponMode.Rifle:
                clip = rifleShootSound;
                cooldown = rifleSoundCooldown;
                break;

            case WeaponMode.Shotgun:
                clip = shotgunShootSound;
                cooldown = shotgunSoundCooldown;
                break;

            case WeaponMode.Uzi:
                clip = uziShootSound;
                cooldown = uziSoundCooldown;
                break;
        }

        if (clip == null)
            return;

        if (Time.time < _nextShootSoundTime)
            return;

        audioSource.PlayOneShot(clip, shootSoundVolume);
        _nextShootSoundTime = Time.time + cooldown;
    }

    private void StartReload(bool manual)
    {
        var w = Current;

        if (_isReloading)
            return;

        // If magazine already full, ignore manual reload
        if (manual && _ammoInMag >= w.magazineSize)
            return;

        // If no reserve ammo and weapon does NOT have infinite reserve, cannot reload
        if (!w.infiniteAmmo && _reserveAmmo <= 0 && _ammoInMag == 0)
            return;

        PlayReloadSound();

        _reloadRoutine = StartCoroutine(ReloadCoroutine(w));
    }

    private void PlayReloadSound()
    {
        if (audioSource == null || reloadSound == null)
            return;

        audioSource.PlayOneShot(reloadSound, reloadSoundVolume);
    }


    private IEnumerator ReloadCoroutine(WeaponSettings w)
    {
        _isReloading = true;

        yield return new WaitForSeconds(w.reloadTime);

        int needed = w.magazineSize - _ammoInMag;

        if (needed > 0)
        {
            if (w.infiniteAmmo)
            {
                _ammoInMag = w.magazineSize;
            }
            else
            {
                int taken = Mathf.Min(needed, _reserveAmmo);
                _ammoInMag += taken;
                _reserveAmmo -= taken;
            }
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
        WeaponMode.Uzi => _uzi,
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


    public void UnlockWeapon(WeaponMode weaponToUnlock, bool equipNow = true, bool resetAmmo = true)
    {
        if (_weaponOrder == null || _weaponOrder.Length == 0)
            return;

        if (_weaponUnlocked == null || _weaponUnlocked.Length != _weaponOrder.Length)
            _weaponUnlocked = new bool[_weaponOrder.Length];

        int index = System.Array.IndexOf(_weaponOrder, weaponToUnlock);

        if (index < 0)
        {
            Debug.LogWarning("Weapon not found in weapon order: " + weaponToUnlock);
            return;
        }

        _weaponUnlocked[index] = true;

        if (resetAmmo)
        {
            WeaponSettings ws = GetSettings(weaponToUnlock);

            _ammoByWeapon[index] = new AmmoState
            {
                mag = Mathf.Max(0, ws.magazineSize),
                reserve = Mathf.Max(0, ws.reserveAmmo)
            };
        }

        if (equipNow)
            ApplyWeapon(weaponToUnlock, resetAmmo: false);

        Debug.Log("Unlocked weapon: " + weaponToUnlock);
    }


    public bool HasWeapon(WeaponMode weaponToCheck)
    {
        if (_weaponOrder == null || _weaponUnlocked == null)
            return false;

        int index = Array.IndexOf(_weaponOrder, weaponToCheck);

        if (index < 0)
            return false;

        return _weaponUnlocked[index];
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
