using UnityEngine;
using UnityEngine.Events;
using Player;

namespace Collectable
{
    public class HealthPickup : MonoBehaviour
    {
        [Header("Collectable Settings")]
        public bool requiresInput = false;
        public KeyCode collectKey = KeyCode.E;

        [Tooltip("Tag of the player GameObject.")]
        public string playerTag = "Player";

        [Header("Health Restore")]
        public int restoreAmount = 20;

        [Header("Events")]
        public UnityEvent onCollected;

        private bool _playerInRange;
        private GameObject _player;

        private void Update()
        {
            if (requiresInput && _playerInRange && Input.GetKeyDown(collectKey))
            {
                Collect();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                _playerInRange = true;
                _player = other.gameObject;

                if (!requiresInput)
                {
                    Collect();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                _playerInRange = false;
                _player = null;
            }
        }

        private void Collect()
        {
            if (_player != null)
            {
                PlayerHealth health = _player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.RestoreHealth(restoreAmount);
                }
            }
            
            onCollected?.Invoke();
            
            Destroy(gameObject);
        }
    }
}