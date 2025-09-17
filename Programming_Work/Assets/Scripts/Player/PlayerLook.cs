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

        private InputSystem_Actions _inputActions;
        private Vector2 _lookInput;
        private float _xRotation = 0f;

        private void Awake()
        {
            _inputActions = new InputSystem_Actions();

            _inputActions.Player.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Look.canceled += _ => _lookInput = Vector2.zero;
        }

        private void OnEnable()
        {
            _inputActions.Enable();
            LockCursor(true);
        }

        private void OnDisable()
        {
            _inputActions.Disable();
            LockCursor(false);
        }

        private void Update()
        {
            Look();
            
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

        private void LockCursor(bool shouldLock)
        {
            Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !shouldLock;
        }
    }
}