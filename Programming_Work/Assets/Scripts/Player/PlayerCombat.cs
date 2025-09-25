using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Player.Weapons;

namespace Player
{
    [DisallowMultipleComponent]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Weapons")] 
        [Tooltip("Optional: If empty, will auto-find weapons in children.")]
        [SerializeField] private List<WeaponBase> weapons = new List<WeaponBase>();
        [SerializeField] private int startIndex = 0;

        [Header("Hit Masks Defaults")]
        [SerializeField] private LayerMask defaultHitMask = ~0;

        private int _currentIndex = -1;
        private WeaponBase _current;

        // Input System handling (same pattern as other player scripts)
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

            _inputActions.Player.Attack.performed += _ => Attack();
            _inputActions.Player.Previous.performed += _ => PreviousWeapon();
            _inputActions.Player.Next.performed += _ => NextWeapon();

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

            if (weapons == null || weapons.Count == 0)
            {
                weapons = new List<WeaponBase>(GetComponentsInChildren<WeaponBase>(true));
            }

            // Ensure attackOrigin and masks are set
            foreach (var w in weapons)
            {
                if (w == null) continue;
                var field = typeof(WeaponBase).GetField("attackOrigin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null && field.GetValue(w) == null && Camera.main != null)
                {
                    field.SetValue(w, Camera.main.transform);
                }

                var maskField = typeof(WeaponBase).GetField("hitMask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (maskField != null)
                {
                    var maskVal = (LayerMask)maskField.GetValue(w);
                    if (maskVal.value == 0) maskField.SetValue(w, defaultHitMask);
                }

                w.gameObject.SetActive(false);
            }

            Equip(Mathf.Clamp(startIndex, 0, Mathf.Max(0, weapons.Count - 1)));
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

        public void Attack()
        {
            if (_current == null) return;
            _current.TryAttack();
        }

        public void NextWeapon()
        {
            if (weapons == null || weapons.Count == 0) return;
            int next = (_currentIndex + 1) % weapons.Count;
            Equip(next);
        }

        public void PreviousWeapon()
        {
            if (weapons == null || weapons.Count == 0) return;
            int prev = (_currentIndex - 1 + weapons.Count) % weapons.Count;
            Equip(prev);
        }

        public void Equip(int index)
        {
            if (weapons == null || weapons.Count == 0) return;
            index = Mathf.Clamp(index, 0, weapons.Count - 1);
            if (index == _currentIndex && _current != null)
            {
                // Ensure only the current stays visible
                for (int i = 0; i < weapons.Count; i++)
                {
                    if (weapons[i] == null) continue;
                    if (i != _currentIndex) weapons[i].OnUnequipped();
                }
                _current.OnEquipped();
                return;
            }

            // Hide all other weapons to avoid visibility leaks
            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i] == null) continue;
                if (i != index) weapons[i].OnUnequipped();
            }

            // Switch current
            _currentIndex = index;
            _current = weapons[_currentIndex];

            if (_current != null)
            {
                _current.OnEquipped();
            }
        }

        // Helper to programmatically add a weapon at runtime
        public void AddWeapon(WeaponBase weapon, bool equip = false)
        {
            if (weapon == null) return;
            if (weapons == null) weapons = new List<WeaponBase>();
            weapons.Add(weapon);
            weapon.gameObject.SetActive(false);
            if (equip) Equip(weapons.Count - 1);
        }

        public WeaponBase CurrentWeapon => _current;
        public int CurrentIndex => _currentIndex;
        public IReadOnlyList<WeaponBase> Weapons => weapons;
    }
}