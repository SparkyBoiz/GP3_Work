using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerDash : MonoBehaviour
    {
        [Header("Dash Settings")]
        [SerializeField] private float dashForce = 10f;
        [SerializeField] private float dashCooldown = 1f;
        [SerializeField] private float dashDuration = 0.2f;

        private Rigidbody _rb;
        private InputSystem_Actions _inputActions;
        private bool _ownsInput = true;
        private bool _inputBound = false;

        private bool _canDash = true;
        private bool _isDashing;
        private float _dashTimeRemaining;

        public void SetInputActions(InputSystem_Actions actions)
        {
            if (actions == null || _inputActions != null) return;
            _inputActions = actions;
            _ownsInput = false;
        }

        private void BindInputActions()
        {
            if (_inputActions == null || _inputBound) return;
            _inputActions.Player.Dash.performed += _ => TryDash();
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
            if (_isDashing)
            {
                _dashTimeRemaining -= Time.fixedDeltaTime;
                if (_dashTimeRemaining <= 0f)
                {
                    _isDashing = false;
                }
            }
        }

        private void TryDash()
        {
            if (!_canDash || _isDashing)
                return;

            StartCoroutine(Dash());
        }

        private System.Collections.IEnumerator Dash()
        {
            _canDash = false;
            _isDashing = true;
            _dashTimeRemaining = dashDuration;

            Vector3 dashDirection = transform.forward;
            _rb.AddForce(dashDirection * dashForce, ForceMode.VelocityChange);

            yield return new WaitForSeconds(dashCooldown);

            _canDash = true;
        }

        public bool IsDashing => _isDashing;
    }
}