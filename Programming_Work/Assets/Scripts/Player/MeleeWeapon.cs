using System.Collections.Generic;
using UnityEngine;
using Managers;

namespace Player.Weapons
{
    public class MeleeWeapon : WeaponBase
    {
        [Header("Melee Settings")]
        [SerializeField] private float range = 2.2f; // forward reach
        [SerializeField] private float radius = 0.6f; // swing radius
        [SerializeField] private int maxHits = 4;

        protected override bool PerformAttack()
        {
            if (attackOrigin == null) return false;

            Vector3 origin = attackOrigin.position;
            Vector3 dir = attackOrigin.forward;

            // Use a short sphere cast to simulate a swing
            RaycastHit[] hits = Physics.SphereCastAll(origin, radius, dir, range, hitMask, QueryTriggerInteraction.Ignore);
            if (hits.Length == 0) return true; // attack still occurred, just missed

            int applied = 0;
            HashSet<Component> processed = new HashSet<Component>();

            foreach (var hit in hits)
            {
                // Skip hitting the player
                if (hit.collider.CompareTag(PlayerService.PlayerTag))
                    continue;

                if (TryGetDamageable(hit.collider, out var dmg))
                {
                    if (processed.Contains(dmg as Component)) continue;
                    dmg.TakeDamage(damage);
                    processed.Add(dmg as Component);
                    applied++;
                    if (applied >= maxHits) break;
                }
            }
            return true;
        }
    }
}
