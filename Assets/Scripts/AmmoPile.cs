using System.Collections;
using UnityEngine;

public class AmmoPile : MonoBehaviour
{
    [SerializeField] private int ammoAmount = 60;
    [SerializeField] private float cooldown = 5f;

    private bool available = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Something entered ammo pile: " + other.name);

        if (!available) return;

        PlayerShoot player = other.GetComponent<PlayerShoot>();
        if (player == null)
        {
            Debug.Log("No PlayerShoot found on: " + other.name);
            return;
        }

        BotShoot botAI = other.GetComponent<BotShoot>();
        if (botAI == null)
        {
            Debug.Log("No BotShoot found on: " + other.name);
            return;
        }

        Debug.Log("Ammo given to player");
        player.AddAmmo(ammoAmount);
        StartCoroutine(Refill());
    }

    private IEnumerator Refill()
    {
        available = false;
        yield return new WaitForSeconds(cooldown);
        available = true;
    }
}
