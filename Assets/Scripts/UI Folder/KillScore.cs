using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class KillScore : MonoBehaviour
{
    public static KillScore Instance;
    [SerializeField] private TextMeshProUGUI Killtext;

    private int kills;


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
        Killtext.text = "Kills: " + kills;
    }

}


