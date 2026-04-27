using UnityEngine;

public class DoorUnlockByKills : MonoBehaviour
{
    [SerializeField] private int killsNeeded = 20;
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private SpriteRenderer doorSprite;
    [SerializeField] private Color unlockedColor = Color.green;
    [SerializeField] private Color lockedColor = Color.white;
    [SerializeField] private bool destroyDoorOnUnlock = false;

    [Header("Unlock Mode")]
    [SerializeField] private bool unlockByGlobalKills = true;

    private bool unlocked;

    public bool IsUnlocked => unlocked;

    private void Awake()
    {
        if (doorCollider == null)
            doorCollider = GetComponent<Collider2D>();

        if (doorSprite == null)
            doorSprite = GetComponent<SpriteRenderer>();

        LockDoor();
    }

    private void Update()
    {
        if (!unlockByGlobalKills)
            return;

        if (unlocked)
            return;

        if (KillScore.Instance == null)
            return;

        if (KillScore.Instance.Kills >= killsNeeded)
            UnlockDoor();
    }

    public void UnlockDoor()
    {
        unlocked = true;

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (doorSprite != null)
            doorSprite.color = unlockedColor;

        if (destroyDoorOnUnlock)
            Destroy(gameObject);
    }

    public void LockDoor()
    {
        unlocked = false;

        if (doorCollider != null)
            doorCollider.enabled = true;

        if (doorSprite != null)
        {
            doorSprite.enabled = true;
            doorSprite.color = lockedColor;
        }
    }
}





