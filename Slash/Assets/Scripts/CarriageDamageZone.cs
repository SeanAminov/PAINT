using System.Collections.Generic;
using UnityEngine;

namespace Slash
{
    // Cart trigger zone. Any enemy inside deals damagePerTick on damageInterval.
    // The cart's BoxCollider2D is the real hitbox so it can be tuned in the
    // inspector without code changes.
    [RequireComponent(typeof(Collider2D))]
    public class CarriageDamageZone : MonoBehaviour
    {
        [Header("Wiring")]
        public CarriageHealth cartHealth;

        [Header("Damage")]
        public int damagePerTick = 1;
        public float damageInterval = 1.6f;

        readonly HashSet<Enemy> _inside = new HashSet<Enemy>();
        float _nextDamageAt;

        void OnTriggerEnter2D(Collider2D other)
        {
            var e = other.GetComponent<Enemy>();
            if (e != null) _inside.Add(e);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            var e = other.GetComponent<Enemy>();
            if (e != null) _inside.Remove(e);
        }

        void Update()
        {
            _inside.RemoveWhere(e => e == null || !e.IsAlive);

            if (cartHealth == null || !cartHealth.IsAlive) return;
            if (_inside.Count == 0) return;
            if (Time.time < _nextDamageAt) return;

            cartHealth.TakeDamage(damagePerTick);
            _nextDamageAt = Time.time + damageInterval;
        }
    }
}
