using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAwarness_old01 : MonoBehaviour
{
    [SerializeField] float awarenessDistance = 12f;

    //lager för att specificera vilka lager som räknas som väggar i raycasten
    [SerializeField] LayerMask wallLayer;
    [SerializeField] Transform player;

    public bool AwarePlayer { get; private set; }
    public Vector2 DirectionToPlayer { get; private set; }
    public float DistanceToPlayer { get; private set; }

    float distSqr;

    void Awake()
    {
        //om player inte är satt i inspektorn, försök hitta den via taggen "Player"
        if (!player)
            player = GameObject.FindWithTag("Player")?.transform;

        // beräkna kvadraten av medvetandeavståndet för att undvika att behöva använda Mathf.Sqrt i Update
        distSqr = awarenessDistance * awarenessDistance;
    }

    void Update()
    {
        if (!player) return;

        //vector från fienden till spelaren
        Vector2 toPlayer = player.position - transform.position;
        float sqr = toPlayer.sqrMagnitude;

        //direktion och avstånd till spelaren
        DirectionToPlayer = toPlayer.normalized;
        DistanceToPlayer = Mathf.Sqrt(sqr);

        //aware om spelaren är inom medvetandeavståndet och det inte finns några väggar i vägen
        AwarePlayer = sqr <= distSqr &&
                      !Physics2D.Raycast(transform.position, DirectionToPlayer, DistanceToPlayer, wallLayer);
    }

    void OnValidate()
    {
        //validera att medvetandeavståndet är inte negativt och uppdatera den kvadrerade distansen
        awarenessDistance = Mathf.Max(0f, awarenessDistance);
        distSqr = awarenessDistance * awarenessDistance;
    }
}
