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

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
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
            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            onHealthChanged?.Invoke();
        }

        private void Die()
        {
            Debug.Log("Player Died");
            onDeath?.Invoke();
        }
    }
}