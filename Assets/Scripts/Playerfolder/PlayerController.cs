using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    private Rigidbody2D _rigidbody;
    private Vector2 _moveInput;
    private float _desiredAngle;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // apply input each physics step
        _rigidbody.linearVelocity = _moveInput * _speed;


        _rigidbody.SetRotation(_desiredAngle);
    }

    private void Update()
    {
        // calculate the desired angle based on the movement input
        if (Camera.main == null) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        float camZ = Camera.main.transform.position.z;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, -camZ));
        Vector2 dir  = (Vector2)(mouseWorld - transform.position);


        if (dir.sqrMagnitude > 0.001f)
        {
            _desiredAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        }
    }


    private void OnMove(InputValue inputValue)
    {
        // store the input so FixedUpdate can use it
        _moveInput = inputValue.Get<Vector2>();
    }
}



