using UnityEngine;

public class DoorUnlockByKills : MonoBehaviour
{
    [SerializeField] private int killsNeeded = 20;
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private SpriteRenderer doorSprite;
    [SerializeField] private Color unlockedColor = Color.green;
    [SerializeField] private bool destroyDoorOnUnlock = false;

    private bool unlocked;

    private void Awake()
    {
        if (doorCollider == null)
            doorCollider = GetComponent<Collider2D>();

        if (doorSprite == null)
            doorSprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (unlocked) return;
        if (KillScore.Instance == null) return;

        if (KillScore.Instance.Kills >= killsNeeded)
            UnlockDoor();
    }

    private void UnlockDoor()
    {
        unlocked = true;

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (doorSprite != null)
            doorSprite.color = unlockedColor;

        if (destroyDoorOnUnlock)
            Destroy(gameObject);
    }
}
