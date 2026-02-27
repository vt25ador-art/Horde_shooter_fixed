using Unity.VisualScripting;
using UnityEngine;

public class PlayerAwarness : MonoBehaviour
{
    public bool AwarePlayer { get; private set; }

    public Vector2 DirectionToPlayer { get; private set; }

    [SerializeField]
    private float _playerAwarenessDistance;

    private Transform _player;


    private void Awake()
    {
        _player = GameObject.FindAnyObjectByType<PlayerController>().transform;
    }


    // Update is called once per frame
    void Update()
    {
        Vector2 enemyToPlayerVector = _player.position - transform.position;
        DirectionToPlayer = enemyToPlayerVector.normalized;
        
        if (enemyToPlayerVector.magnitude <= _playerAwarenessDistance)
        {
            AwarePlayer = true;
        }
         else
        {
            AwarePlayer = false;
        }
    }
}
