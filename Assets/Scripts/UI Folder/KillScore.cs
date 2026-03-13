using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class KillScore : MonoBehaviour
{
    public static KillScore Instance;
    [SerializeField] private TextMeshProUGUI Killtext;

    private int kills = 0;

    public int Kills => kills;


    private void Awake()
    {
      Instance = this;
        UpdateUI();
    }

    public void AddKill()
    {
        kills++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (Killtext != null)
        {
            Killtext.text = "Kills: " + kills;
        }
        
        Killtext.text = "Kills: " + kills;
    }

}


