using System;
using UnityEngine;

namespace Slash
{
    // Player HP. Damage is skipped during the iframe window, during a zip,
    // and during ult mode. Ult is a true god window that even the tank slam's
    // ignoreInvuln cannot punch through.
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Wiring")]
        public PlayerController controller;
        public UltSystem ult;

        [Header("Stats")]
        public int maxHP = 3;
        public float invulnOnHit = 1.25f;

        public int CurrentHP { get; private set; }
        public bool IsAlive => CurrentHP > 0;
        public event Action OnDamaged;

        public bool IsInvulnerable
        {
            get
            {
                if (Time.time < _invulnUntil) return true;
                if (controller != null && controller.IsZipping) return true;
                if (ult != null && ult.IsActive) return true;
                return false;
            }
        }

        float _invulnUntil;
        SpriteRenderer _sr;
        Color _baseColor;
        float _flashUntil;

        void Awake()
        {
            CurrentHP = maxHP;
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _baseColor = _sr.color;
        }

        void Update()
        {
            if (_sr == null || _flashUntil <= 0f) return;

            if (Time.time < _flashUntil)
            {
                _sr.color = Color.red;
            }
            else
            {
                _sr.color = _baseColor;
                _flashUntil = 0f;
            }
        }

        public void TakeDamage(int amount) { TakeDamage(amount, false); }

        public void TakeDamage(int amount, bool ignoreInvuln)
        {
            if (!IsAlive) return;
            // Ult is a hard invuln that overrides ignoreInvuln.
            if (ult != null && ult.IsActive) return;
            if (!ignoreInvuln && IsInvulnerable) return;

            CurrentHP = Mathf.Max(0, CurrentHP - amount);
            _invulnUntil = Time.time + invulnOnHit;
            _flashUntil = Time.time + 0.08f;
            AudioCues.PlayPlayerHurt(transform.position);
            OnDamaged?.Invoke();
        }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
        }
    }
}
