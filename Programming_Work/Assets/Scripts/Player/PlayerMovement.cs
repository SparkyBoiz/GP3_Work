using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float groundCheckDistance = 1f;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private float groundCheckTolerance = 0.1f;
        [SerializeField] private float groundCheckStartOffset = 0.1f;
        [SerializeField] private float acceleration = 100f;
        [Header("Jump Settings")]
        [SerializeField] private int extraAirJumps = 1;
        [Header("Sprint Settings")]
        [SerializeField] private float sprintMultiplier = 1.8f;
        [SerializeField] private bool requireGroundForSprint = true;
        [SerializeField] private LayerMask groundLayer;

        private Rigidbody _rb;
        private Vector2 _moveInput;
        private bool _jumpInput;
        private bool _isGrounded;
        private int _airJumpsRemaining;
        private bool _sprintHeld;
        private bool _isSprinting;

        private InputSystem_Actions _inputActions;
        private bool _ownsInput = true;
        private bool _inputBound = false;

        public void SetInputActions(InputSystem_Actions actions)
        {
            if (actions == null || _inputActions != null) return;
            _inputActions = actions;
            _ownsInput = false;
        }

        private void BindInputActions()
        {
            if (_inputActions == null || _inputBound) return;

            _inputActions.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Move.canceled += _ => _moveInput = Vector2.zero;
            _inputActions.Player.Jump.performed += _ => { _jumpInput = true; };
            _inputActions.Player.Sprint.performed += _ => { _sprintHeld = true; };
            _inputActions.Player.Sprint.canceled += _ => { _sprintHeld = false; };

            _inputBound = true;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if (_inputActions == null)
            {
                _inputActions = new InputSystem_Actions();
                _ownsInput = true;
            }
            BindInputActions();
        }

        private void OnEnable()
        {
            _inputActions?.Enable();
        }

        private void OnDisable()
        {
            _inputActions?.Disable();
        }

        private void OnDestroy()
        {
            if (_ownsInput && _inputActions != null)
            {
                _inputActions.Dispose();
                _inputActions = null;
            }
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
            bool canSprint = _sprintHeld && (!requireGroundForSprint || _isGrounded) && _moveInput.sqrMagnitude > 0.01f;
            _isSprinting = canSprint;
            float speed = canSprint ? moveSpeed * sprintMultiplier : moveSpeed;
            Vector3 targetVelocity = direction * speed;

            Vector3 currentVelocity = _rb.linearVelocity;
            float ax = acceleration * Time.fixedDeltaTime;
            float newX = Mathf.MoveTowards(currentVelocity.x, targetVelocity.x, ax);
            float newZ = Mathf.MoveTowards(currentVelocity.z, targetVelocity.z, ax);
            _rb.linearVelocity = new Vector3(newX, currentVelocity.y, newZ);
        }

    
        private void Jump()
        {
            Vector3 origin = transform.position + Vector3.up * groundCheckStartOffset;
            float distance = groundCheckDistance + groundCheckStartOffset + groundCheckTolerance;
            _isGrounded = Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out _, distance, groundLayer);
            if (_isGrounded)
            {
                _airJumpsRemaining = extraAirJumps;
            }


            if (_jumpInput)
            {
                if (_isGrounded)
                {
                    _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                    _airJumpsRemaining = extraAirJumps;
                }
                else if (_airJumpsRemaining > 0)
                {
                    _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                    _airJumpsRemaining--;
                }
            }
        
            _jumpInput = false;
        }

        public bool IsSprinting => _isSprinting;
    }
}