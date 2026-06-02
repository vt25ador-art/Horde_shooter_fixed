using UnityEngine;

public class PlayerMelee : MonoBehaviour
{
    public enum MeleeWeaponType
    {
        None,
        Bat,
        Axe,
        Katana,
        Crowbar
    }

    [Header("Melee State")]
    [SerializeField] private bool hasMeleeWeapon = true;
    [SerializeField] private MeleeWeaponType currentMeleeWeapon = MeleeWeaponType.Bat;

    [Header("Input")]
    [SerializeField] private KeyCode meleeKey = KeyCode.F;

    [Header("Attack")]
    [SerializeField] private float damage = 75f;
    [SerializeField] private float range = 1.2f;
    [SerializeField] private float radius = 0.8f;
    [SerializeField] private float cooldown = 0.65f;

    [Header("Layers")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Optional")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioClip hitSound;

    private float nextAttackTime;
    private readonly Collider2D[] hits = new Collider2D[32];

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (attackPoint == null)
            attackPoint = transform;
    }

    private void Update()
    {
        if (!hasMeleeWeapon)
            return;

        if (Input.GetKeyDown(meleeKey))
        {
            Debug.Log("MELEE KEY PRESSED");
            TryMeleeAttack();
        }
    }

    private void TryMeleeAttack()
    {
        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + cooldown;

        if (audioSource != null && swingSound != null)
            audioSource.PlayOneShot(swingSound);

        Vector2 center = GetAttackCenter();

        int hitCount = Physics2D.OverlapCircleNonAlloc(
            center,
            radius,
            hits,
            enemyLayer
        );

        Debug.Log("Melee hits found: " + hitCount);

        bool hitSomething = false;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hits[i];

            if (hit == null)
                continue;

            Debug.Log("Melee touched: " + hit.name);

            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();

            if (enemyHealth == null)
                enemyHealth = hit.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null && !enemyHealth.IsDead)
            {
                enemyHealth.TakeDamage(damage);
                hitSomething = true;

                Debug.Log("Melee damaged enemy: " + enemyHealth.name);
            }
            else
            {
                Debug.LogWarning("No EnemyHealth found on: " + hit.name);
            }

            hits[i] = null;
        }

        if (hitSomething && audioSource != null && hitSound != null)
            audioSource.PlayOneShot(hitSound);
    }

    private Vector2 GetAttackCenter()
    {
        Vector2 forward = transform.up;

        if (attackPoint != null)
            return (Vector2)attackPoint.position + forward * range;

        return (Vector2)transform.position + forward * range;
    }

    public void GiveMeleeWeapon(MeleeWeaponType weaponType)
    {
        hasMeleeWeapon = true;
        currentMeleeWeapon = weaponType;

        switch (weaponType)
        {
            case MeleeWeaponType.Bat:
                damage = 65f;
                cooldown = 0.55f;
                range = 1.25f;
                radius = 0.75f;
                break;

            case MeleeWeaponType.Axe:
                damage = 100f;
                cooldown = 0.9f;
                range = 1.3f;
                radius = 0.8f;
                break;

            case MeleeWeaponType.Katana:
                damage = 85f;
                cooldown = 0.45f;
                range = 1.45f;
                radius = 0.85f;
                break;

            case MeleeWeaponType.Crowbar:
                damage = 70f;
                cooldown = 0.6f;
                range = 1.2f;
                radius = 0.7f;
                break;
        }

        Debug.Log("Picked up melee weapon: " + weaponType);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 forward = transform.up;
        Vector2 center = (Vector2)transform.position + forward * range;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, radius);
    }
}