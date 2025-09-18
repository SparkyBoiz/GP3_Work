using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerLook : MonoBehaviour
    {
        [Header("Look Settings")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float sensitivity = 2f;
        [SerializeField] private float verticalClamp = 80f;
        [Header("FOV Settings")]
        [SerializeField] private float normalFOV = 60f;
        [SerializeField] private float sprintFOV = 75f;
        [SerializeField] private float fovLerpSpeed = 8f;

        private InputSystem_Actions _inputActions;
        private bool _ownsInput = true;
        private bool _inputBound = false;
        private Vector2 _lookInput;
        private float _xRotation = 0f;
        private Camera _camera;
        private PlayerMovement _movement;

        public void SetInputActions(InputSystem_Actions actions)
        {
            if (actions == null || _inputActions != null) return;
            _inputActions = actions;
            _ownsInput = false;
        }

        private void BindInputActions()
        {
            if (_inputActions == null || _inputBound) return;

            _inputActions.Player.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Look.canceled += _ => _lookInput = Vector2.zero;

            _inputBound = true;
        }

        private void Awake()
        {
            if (_inputActions == null)
            {
                _inputActions = new InputSystem_Actions();
                _ownsInput = true;
            }
            BindInputActions();

            if (cameraTransform != null)
            {
                _camera = cameraTransform.GetComponent<Camera>();
            }
            if (_camera == null && Camera.main != null)
            {
                _camera = Camera.main;
                if (cameraTransform == null) cameraTransform = _camera.transform;
            }

            _movement = GetComponent<PlayerMovement>();

            if (_camera != null)
            {
                _camera.fieldOfView = normalFOV;
            }
        }

        private void OnEnable()
        {
            _inputActions?.Enable();
            LockCursor(true);
        }

        private void OnDisable()
        {
            _inputActions?.Disable();
            LockCursor(false);
        }

        private void OnDestroy()
        {
            if (_ownsInput && _inputActions != null)
            {
                _inputActions.Dispose();
                _inputActions = null;
            }
        }

        private void Update()
        {
            Look();
            UpdateFOV();
            
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                bool isLocked = Cursor.lockState == CursorLockMode.Locked;
                LockCursor(!isLocked);
            }
        }

        private void Look()
        {
            float mouseX = _lookInput.x * sensitivity;
            float mouseY = _lookInput.y * sensitivity;
            
            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -verticalClamp, verticalClamp);
            cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            
            transform.Rotate(Vector3.up * mouseX);
        }

        private void UpdateFOV()
        {
            if (_camera == null) return;
            bool sprinting = _movement != null && _movement.IsSprinting;
            float target = sprinting ? sprintFOV : normalFOV;
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, target, Time.deltaTime * fovLerpSpeed);
        }

        private void LockCursor(bool shouldLock)
        {
            Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !shouldLock;
        }
    }
}