using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class AmmoPile : MonoBehaviour
{
    [SerializeField] private int _ammoAmount = 60;
    [SerializeField] private float cooldown = 5f;
    private bool available = true;

    private void OnTriggerExit2D(Collider2D other)
    {
      if (!available) return;
        PlayerShoot player = other.GetComponent<PlayerShoot>();
        if (player != null)
        {
            player.AddAmmo(_ammoAmount);
            StartCoroutine(refill());
        }
    }


    IEnumerator refill()
    {
        available = false;
        yield return new WaitForSeconds(cooldown);
        available = true;
    }
}
