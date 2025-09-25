using UnityEngine;
using Player;
using Managers;

namespace Traps
{
    [RequireComponent(typeof(Collider))]
    public class TrapDamage : MonoBehaviour
    {
        [Header("Trap Settings")]
        public int damageAmount = 10;
        public bool damageOnEnter = true;
        public bool damageOverTime = false;
        public float damageInterval = 1f;

        [Tooltip("Tag of the player GameObject.")]
        public string playerTag = "Player";

        private float _timeSinceLastDamage;

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (!col.isTrigger)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            string tagToCheck = string.IsNullOrEmpty(playerTag) ? Managers.PlayerService.PlayerTag : playerTag;
            if (other.CompareTag(tagToCheck))
            {
                if (damageOnEnter)
                {
                    ApplyDamage(other);
                }
                _timeSinceLastDamage = 0f;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            string tagToCheck = string.IsNullOrEmpty(playerTag) ? Managers.PlayerService.PlayerTag : playerTag;
            if (damageOverTime && other.CompareTag(tagToCheck))
            {
                _timeSinceLastDamage += Time.deltaTime;
                if (_timeSinceLastDamage >= damageInterval)
                {
                    ApplyDamage(other);
                    _timeSinceLastDamage = 0f;
                }
            }
        }

        private void ApplyDamage(Collider playerCollider)
        {
            PlayerHealth health = playerCollider.GetComponent<PlayerHealth>();
            if (health == null && PlayerService.Instance != null)
            {
                health = PlayerService.Instance.Health;
            }
            if (health != null)
            {
                health.TakeDamage(damageAmount);
                Debug.Log($"Player damaged by trap: {damageAmount}");
            }
        }
    }
}