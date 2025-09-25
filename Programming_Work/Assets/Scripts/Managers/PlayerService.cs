using UnityEngine;
using Core;
using Player;

namespace Managers
{
    /// <summary>
    /// Centralized access point for the Player and its components.
    /// Provides cached references with lazy resolution to avoid repeated scene searches.
    /// </summary>
    public class PlayerService : Singleton<PlayerService>
    {
        public const string PlayerTag = "Player";

        private Transform _playerTransform;
        private GameObject _playerGO;
        private PlayerHealth _playerHealth;
        private PlayerMovement _playerMovement;
        private PlayerLook _playerLook;
        private PlayerDash _playerDash;

        protected override void Awake()
        {
            base.Awake();
            // Try to resolve immediately in case the player already exists in the scene.
            TryResolvePlayer();
        }

        private bool TryResolvePlayer()
        {
            if (_playerGO != null)
            {
                // Cleanup destroyed references
                if (_playerGO == null)
                {
                    ClearCached();
                }
                else
                {
                    return true;
                }
            }

            var go = GameObject.FindGameObjectWithTag(PlayerTag);
            if (go == null)
            {
                ClearCached();
                return false;
            }

            _playerGO = go;
            _playerTransform = go.transform;
            _playerHealth = go.GetComponent<PlayerHealth>();
            _playerMovement = go.GetComponent<PlayerMovement>();
            _playerLook = go.GetComponent<PlayerLook>();
            _playerDash = go.GetComponent<PlayerDash>();
            return true;
        }

        private void ClearCached()
        {
            _playerGO = null;
            _playerTransform = null;
            _playerHealth = null;
            _playerMovement = null;
            _playerLook = null;
            _playerDash = null;
        }

        public GameObject PlayerGO
        {
            get
            {
                if (_playerGO == null) TryResolvePlayer();
                return _playerGO;
            }
        }

        public Transform PlayerTransform
        {
            get
            {
                if (_playerTransform == null) TryResolvePlayer();
                return _playerTransform;
            }
        }

        public PlayerHealth Health
        {
            get
            {
                if (_playerHealth == null) TryResolvePlayer();
                return _playerHealth;
            }
        }

        public PlayerMovement Movement
        {
            get
            {
                if (_playerMovement == null) TryResolvePlayer();
                return _playerMovement;
            }
        }

        public PlayerLook Look
        {
            get
            {
                if (_playerLook == null) TryResolvePlayer();
                return _playerLook;
            }
        }

        public PlayerDash Dash
        {
            get
            {
                if (_playerDash == null) TryResolvePlayer();
                return _playerDash;
            }
        }

        /// <summary>
        /// Force re-query the scene for the player and refresh cached references.
        /// </summary>
        public void Refresh()
        {
            ClearCached();
            TryResolvePlayer();
        }
    }
}
