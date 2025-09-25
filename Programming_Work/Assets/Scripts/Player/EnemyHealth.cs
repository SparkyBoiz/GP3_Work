using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using Combat;

namespace Enemies
{
    /// <summary>
    /// Basic enemy health receiver implementing IDamageable.
    /// </summary>
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        public int maxHealth = 50;
        public int currentHealth;
        public UnityEvent onDamaged;
        public UnityEvent onDeath;

        private bool _dead;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (_dead) return;
            currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
            onDamaged?.Invoke();
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            if (_dead) return;
            _dead = true;
            onDeath?.Invoke();

            var agent = GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = false;
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Destroy(gameObject, 2f);
        }
    }
}
