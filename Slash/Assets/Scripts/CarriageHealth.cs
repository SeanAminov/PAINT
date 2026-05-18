using System;
using UnityEngine;

namespace Slash
{
    // Cart HP. Below max, regens 1 HP every regenInterval. Damage does not
    // reset the timer, so the cart keeps recovering under sustained pressure.
    public class CarriageHealth : MonoBehaviour
    {
        [Header("Stats")]
        public int maxHP = 10;
        public float regenInterval = 5f;
        public float damageFlashDuration = 0.12f;

        public int CurrentHP { get; private set; }
        public bool IsAlive => CurrentHP > 0;
        public event Action OnDamaged;
        public event Action OnDestroyed;

        float _lastDamagedAt = -100f;
        float _regenAccumulator;
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
            if (_sr != null && _flashUntil > 0f)
            {
                if (Time.time < _flashUntil) _sr.color = Color.red;
                else { _sr.color = _baseColor; _flashUntil = 0f; }
            }

            // Hold the accumulator at 0 while at full HP so the next damage
            // event starts the regen clock fresh.
            if (CurrentHP >= maxHP)
            {
                _regenAccumulator = 0f;
                return;
            }

            _regenAccumulator += Time.deltaTime;
            if (_regenAccumulator >= regenInterval)
            {
                _regenAccumulator -= regenInterval;
                CurrentHP = Mathf.Min(maxHP, CurrentHP + 1);
            }
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || amount <= 0) return;

            CurrentHP = Mathf.Max(0, CurrentHP - amount);
            _lastDamagedAt = Time.time;
            _flashUntil = Time.time + damageFlashDuration;

            AudioCues.PlayPrincessHurt();

            OnDamaged?.Invoke();
            if (CurrentHP <= 0) OnDestroyed?.Invoke();
        }

        public float Fraction => (float)CurrentHP / Mathf.Max(1, maxHP);
        public float SecondsSinceDamage => Time.time - _lastDamagedAt;
    }
}
