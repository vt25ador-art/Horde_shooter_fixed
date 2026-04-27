using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private SpriteRenderer doorSprite;
    [SerializeField] private bool hideSpriteWhenOpen = true;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (doorCollider == null)
            doorCollider = GetComponent<Collider2D>();

        if (doorSprite == null)
            doorSprite = GetComponent<SpriteRenderer>();

        CloseDoor();
    }

    public void OpenDoor()
    {
        IsOpen = true;

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (doorSprite != null && hideSpriteWhenOpen)
            doorSprite.enabled = false;
    }

    public void CloseDoor()
    {
        IsOpen = false;

        if (doorCollider != null)
            doorCollider.enabled = true;

        if (doorSprite != null)
            doorSprite.enabled = true;
    }
}

