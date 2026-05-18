using System;
using System.Collections.Generic;
using UnityEngine;

namespace Slash
{
    // Generic enemy: HP, hit flash, self-registers into Enemy.All so the
    // player can query the nearest cheaply, and broadcasts OnAnyEnemyDied so
    // coin and heart spawners can react without direct references.
    public class Enemy : MonoBehaviour
    {
        public static readonly List<Enemy> All = new List<Enemy>();
        public static event Action<Vector3> OnAnyEnemyDied;

        [Header("Stats")]
        public int maxHP = 1;
        public bool invincible = false;

        [Header("Audio Tags")]
        // Tanks route non-fatal hits through the big guy hurt cue. Death
        // always uses the generic enemy death cue.
        public bool isBigGuy = false;

        public int CurrentHP { get; private set; }
        public bool IsAlive => invincible || CurrentHP > 0;

        SpriteRenderer _sr;
        Color _baseColor;
        float _flashUntil;
        bool _hpInitialized;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _baseColor = _sr.color;
        }

        void Start()
        {
            EnsureHPInitialized();
        }

        void EnsureHPInitialized()
        {
            if (_hpInitialized) return;
            CurrentHP = maxHP;
            _hpInitialized = true;
        }

        void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        void OnDisable() { All.Remove(this); }

        void Update()
        {
            if (_sr == null || _flashUntil <= 0f) return;

            if (Time.time < _flashUntil)
            {
                _sr.color = Color.white;
            }
            else
            {
                _sr.color = _baseColor;
                _flashUntil = 0f;
            }
        }

        public void TakeDamage(int amount)
        {
            EnsureHPInitialized();

            if (invincible)
            {
                Flash();
                return;
            }

            CurrentHP -= amount;
            Flash();

            if (CurrentHP <= 0)
            {
                Die();
            }
            else if (isBigGuy)
            {
                AudioCues.PlayBigGuyHurt(transform.position);
            }
        }

        void Flash()
        {
            _flashUntil = Time.time + 0.06f;
        }

        void Die()
        {
            AudioCues.PlayEnemyDeath(transform.position);
            OnAnyEnemyDied?.Invoke(transform.position);
            Destroy(gameObject);
        }
    }
}
