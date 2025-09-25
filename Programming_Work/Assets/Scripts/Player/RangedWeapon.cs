using UnityEngine;
using Managers;

namespace Player.Weapons
{
    public class RangedWeapon : WeaponBase
    {
        [Header("Ranged Settings")]
        [SerializeField] private float range = 50f;
        [SerializeField] private float impactForce = 5f;

        protected override bool PerformAttack()
        {
            if (attackOrigin == null) return false;

            Vector3 origin = attackOrigin.position;
            Vector3 dir = attackOrigin.forward;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                // Ignore player self-hit
                if (!hit.collider.CompareTag(PlayerService.PlayerTag))
                {
                    if (TryGetDamageable(hit.collider, out var dmg))
                    {
                        dmg.TakeDamage(damage);
                    }

                    var rb = hit.rigidbody;
                    if (rb != null)
                    {
                        rb.AddForce(dir * impactForce, ForceMode.Impulse);
                    }
                }
            }
            return true;
        }
    }
}
