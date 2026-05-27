using UnityEngine;

public class FirstAidPickup : MonoBehaviour
{
    [SerializeField] private int amount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        FirstAidUser firstAidUser = other.GetComponent<FirstAidUser>();

        if (firstAidUser == null)
            return;

        bool pickedUp = firstAidUser.AddFirstAid(amount);

        if (pickedUp)
        {
            Destroy(gameObject);
        }
    }
}

