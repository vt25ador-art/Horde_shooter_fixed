using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class PlayerShoot : MonoBehaviour
{

    [Header("Reference")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _GunOffset;


    [Header("Tuning")]
    [SerializeField] private float _bulletSpeed = 10f;
    [SerializeField] private float _timeBetweenShots = 0.15f;
    [SerializeField] private float _spawnOffset = 0.2f;
    [SerializeField] private float _bulletLifetime = 3f;


    [Header("Pool")]
    [SerializeField] private int _poolDefaultCapacity = 32;
    [SerializeField] private int _poolMaxSize = 256;

    private ObjectPool<Bullet> _pool;


    private bool _fireHeld;
    private bool _singleShot;
    private float _nextShotTime;

    private Transform _spawnT;
    private Collider2D _shooterCol;

    private bool _prevMousePressed;

    public bool IsReloading { get; set; }
    public bool WeaponName { get; set; }

    public bool AmmoInMag { get; set; }

    public bool ReserveAmmo { get; set; }




    private void Awake()
    {
        _spawnT = (_GunOffset != null) ? _GunOffset : transform;
        _shooterCol = GetComponent<Collider2D>();


        _pool = new ObjectPool<Bullet>(createFunc: CreateBullet, actionOnGet: 
            OnGetBullet, actionOnRelease:
            OnReleaseBullet, actionOnDestroy: 
            OnDestroyBullet, collectionCheck: 
            false, defaultCapacity: 
            _poolDefaultCapacity, maxSize: _poolMaxSize);

        if (_bulletPrefab == null)
            Debug.LogError("Bullet prefab is not assigned.");
    }

    private Bullet CreateBullet()
    {
        GameObject bulletObj = Instantiate(_bulletPrefab);
        Bullet b = bulletObj.GetComponent<Bullet>();
        if (b == null)
        {
            Debug.LogError("Bullet prefab does not have a Bullet component.");
            return null;
        }

        b.Init(ReturnToPool);
        b.gameObject.SetActive(false);
        return b;
    }


    private void OnGetBullet(Bullet B)
    {
        if (B == null) return;
        B.gameObject.SetActive(true);
    }
    private void OnReleaseBullet(Bullet B)
    {
        if (B == null) return; 
        B.gameObject.SetActive(false);
    }

    private void OnDestroyBullet(Bullet B)
    {
        if (B == null) return;
        Destroy(B.gameObject);
    }


    private void ReturnToPool(Bullet B)
    {
        if (_pool == null || B == null) return;
        _pool.Release(B);
    }

    private void Update()
    {
        bool isMousePressed = Mouse.current != null && Mouse.current.leftButton.isPressed;

        if (isMousePressed && ! _prevMousePressed)
            _singleShot = true; 
        _prevMousePressed = isMousePressed;
        
        bool firePressed = _fireHeld || isMousePressed;


        if (!firePressed && !_singleShot) return;
        if (Time.time < _nextShotTime) return;

        ShootOnce();
        _nextShotTime = Time.time + _timeBetweenShots;
        _singleShot = false;
    }


    private void ShootOnce()
    {
        if (_pool == null)
        {
            Debug.LogError("Bullet pool is not initialized."); return;
        }

        Vector3 spawnPos = _spawnT.position + _spawnT.up * _spawnOffset;
        Quaternion rot = _spawnT.rotation; 

        Bullet bullet = _pool.Get();
        if (bullet == null)
        {
            Debug.LogError("Failed to get bullet from pool."); return;
        }


        bullet.transform.SetPositionAndRotation(spawnPos, rot);
        Vector2 vel = (Vector2)_spawnT.up * _bulletSpeed;
        bullet.Fire(vel, _bulletLifetime, _shooterCol);
    }
}
