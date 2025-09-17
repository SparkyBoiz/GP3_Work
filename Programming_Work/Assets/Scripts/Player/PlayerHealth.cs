using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        public int maxHealth = 100;
        public int currentHealth;

        [Header("Events")]
        public UnityEvent onHealthChanged;
        public UnityEvent onDeath;

        private bool _isDead = false;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (_isDead) return;

            currentHealth -= amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            onHealthChanged?.Invoke();

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void RestoreHealth(int amount)
        {
            if (_isDead) return;

            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            onHealthChanged?.Invoke();
        }

        private void Die()
        {
            if (_isDead) return;

            _isDead = true;
            Debug.Log("Player Died");
            onDeath?.Invoke();
            
            var movement = GetComponent<PlayerMovement>();
            if (movement != null)
                movement.enabled = false;

            var look = GetComponent<PlayerLook>();
            if (look != null)
                look.enabled = false;
            
        }
    }
}