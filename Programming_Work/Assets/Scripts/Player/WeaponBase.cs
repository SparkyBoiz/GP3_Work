using UnityEngine;
using Combat;

namespace Player.Weapons
{
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("Common Weapon Settings")]
        [SerializeField] protected int damage = 10;
        [SerializeField] protected float cooldown = 0.4f;
        [SerializeField] protected Transform attackOrigin; // typically camera or muzzle
        [SerializeField] protected LayerMask hitMask = ~0; // default everything

        protected float _lastAttackTime = -999f;

        protected virtual void OnValidate()
        {
            if (attackOrigin == null && Camera.main != null)
                attackOrigin = Camera.main.transform;
        }

        protected virtual void Awake()
        {
            if (attackOrigin == null && Camera.main != null)
                attackOrigin = Camera.main.transform;
        }

        public bool TryAttack()
        {
            if (Time.time - _lastAttackTime < cooldown)
                return false;

            bool fired = PerformAttack();
            if (fired)
            {
                _lastAttackTime = Time.time;
            }
            return fired;
        }

        protected abstract bool PerformAttack();

        protected static bool TryGetDamageable(Collider c, out IDamageable damageable)
        {
            damageable = c.GetComponentInParent<IDamageable>();
            return damageable != null;
        }

        public virtual void OnEquipped() { gameObject.SetActive(true); }
        public virtual void OnUnequipped() { gameObject.SetActive(false); }
    }
}
