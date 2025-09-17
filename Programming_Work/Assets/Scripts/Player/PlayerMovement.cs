using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        private const float MoveSpeed = 5f;
        private const float JumpForce = 5f;
        private const float GroundCheckDistance = 1f;
        public LayerMask groundLayer;

        private Rigidbody _rb;
        private Vector2 _moveInput;
        private bool _jumpInput;
        private bool _isGrounded;

        private InputSystem_Actions _inputActions;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _inputActions = new InputSystem_Actions();

            _inputActions.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Move.canceled += _ => _moveInput = Vector2.zero;

            _inputActions.Player.Jump.performed += _ =>
            {
                _jumpInput = true;
            };
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        private void FixedUpdate()
        {
            Move();
            Jump();
        }

        private void Move()
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            Vector3 direction = (right * _moveInput.x + forward * _moveInput.y).normalized;
            Vector3 velocity = direction * MoveSpeed;

            Vector3 currentVelocity = _rb.linearVelocity;
            _rb.linearVelocity = new Vector3(velocity.x, currentVelocity.y, velocity.z);
        }

    
        private void Jump()
        {
            _isGrounded = Physics.Raycast(transform.position, Vector3.down, GroundCheckDistance + 0.1f, groundLayer);

            Debug.DrawRay(transform.position, Vector3.down * (GroundCheckDistance + 0.1f), _isGrounded ? Color.green : Color.red);

            if (_jumpInput && _isGrounded)
            {
                _rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
            }
        
            _jumpInput = false;
        }
    }
}