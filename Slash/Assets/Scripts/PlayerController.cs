using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Slash
{
    // Player movement and attacks. Attack mode switches on ult state.
    // Normal: standing slash that throws a short-range wave hitting one enemy.
    // Ult: lock-on dash to the nearest enemy with a brief startup pause.
    public class PlayerController : MonoBehaviour
    {
        [Header("Wiring")]
        public UltSystem ult;
        public UltChargeAttack chargeAttack;
        public Sprite waveSprite;

        [Header("Movement")]
        public float moveSpeed = 8f;

        [Header("Normal Slash")]
        public float normalSlashRange = 3f;
        public float normalSlashCooldown = 0.35f;
        public float normalSlashSpeed = 16f;
        public float normalSlashRadius = 1.2f;
        public Vector2 normalSlashSize = new Vector2(1f, 1f);
        public Color normalSlashColor = Color.white;
        public Vector3 normalSlashSpawnOffset = new Vector3(0.55f, 1.55f, 0f);
        public int normalSlashDamage = 1;
        // Swipe art is authored facing left; flip when the player faces right.
        public bool flipNormalSwipeX = true;

        [Header("Ult Dash")]
        public float ultDashRange = 14f;
        public float ultDashDuration = 0.05f;
        public float ultDashCooldown = 0.08f;
        public float ultDashLandOffset = 0.6f;
        public float ultDashStartupPause = 0.02f;
        public int ultDashDamage = 1;

        [Header("Hit Visual")]
        public float hitPositionLift = 1f;

        public event Action<Vector3, Vector2> OnHitLanded;

        static readonly int BasicSlashHash = Animator.StringToHash("BasicSlash");
        Animator _animator;

        public int ComboCount => _comboCount;
        public bool IsZipping => _zipping;

        float _attackReadyAt;
        bool _zipping;
        int _comboCount;
        SlashTrail _trail;
        PlayerHealth _health;
        int _lastFacing = 1;

        void Awake()
        {
            _trail = GetComponent<SlashTrail>();
            _health = GetComponent<PlayerHealth>();
            _animator = GetComponent<Animator>();
            if (_health != null) _health.OnDamaged += BreakCombo;
        }

        void OnDestroy()
        {
            if (_health != null) _health.OnDamaged -= BreakCombo;
        }

        void Update()
        {
            if (_trail != null) _trail.boosted = ult != null && ult.IsActive;

            if (_health != null && !_health.IsAlive) return;
            if (chargeAttack != null && chargeAttack.IsCharging) return;
            if (_zipping) return;

            HandleMovement();
            HandleAttack();
        }

        void HandleMovement()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            float x = 0f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;

            if (x > 0f) _lastFacing = 1;
            else if (x < 0f) _lastFacing = -1;

            transform.position += new Vector3(x * moveSpeed * Time.deltaTime, 0f, 0f);
        }

        void HandleAttack()
        {
            if (!AttackPressed()) return;
            if (Time.time < _attackReadyAt) return;

            // Fires on the swing, not on contact, so whiffs play too.
            AudioCues.PlaySlash();

            if (ult != null && ult.IsActive) DoUltDash();
            else DoNormalSlash();
        }

        bool AttackPressed()
        {
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) return true;

            var kb = Keyboard.current;
            if (kb != null && kb.cKey.wasPressedThisFrame) return true;

            return false;
        }

        void DoNormalSlash()
        {
            SpawnSlashWave();
            _attackReadyAt = Time.time + normalSlashCooldown;
            if (_animator != null) _animator.SetTrigger(BasicSlashHash);
        }

        void SpawnSlashWave()
        {
            var go = new GameObject("SlashWave");
            Vector3 spawnOffset = new Vector3(
                normalSlashSpawnOffset.x * _lastFacing,
                normalSlashSpawnOffset.y,
                normalSlashSpawnOffset.z);
            go.transform.position = transform.position + spawnOffset;
            go.transform.localScale = new Vector3(normalSlashSize.x, normalSlashSize.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = waveSprite != null ? waveSprite : SpriteUtil.Square;
            sr.color = normalSlashColor;
            sr.sortingOrder = 5;
            sr.flipX = flipNormalSwipeX ^ (_lastFacing < 0);

            var wave = go.AddComponent<PlayerSlashWave>();
            wave.direction = new Vector2(_lastFacing, 0f);
            wave.speed = normalSlashSpeed;
            wave.range = normalSlashRange;
            wave.damage = normalSlashDamage;
            wave.radius = normalSlashRadius;
            wave.OnHit += OnWaveHit;
        }

        void OnWaveHit(Enemy target, Vector3 hitWorld, Vector2 dir)
        {
            RegisterHit(target);
        }

        void DoUltDash()
        {
            var target = FindNearestEnemyInRange(ultDashRange);
            if (target == null)
            {
                _attackReadyAt = Time.time + ultDashCooldown;
                return;
            }

            int approachDir = target.transform.position.x >= transform.position.x ? 1 : -1;
            StartCoroutine(UltDashTo(target, approachDir));
        }

        IEnumerator UltDashTo(Enemy target, int approachDir)
        {
            _zipping = true;
            _lastFacing = approachDir;

            if (ultDashStartupPause > 0f)
            {
                float t = 0f;
                while (t < ultDashStartupPause)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
            }

            Vector3 start = transform.position;
            Vector3 end = target != null ? target.transform.position : start + new Vector3(approachDir * 4f, 0f, 0f);
            end.x -= approachDir * ultDashLandOffset;
            end.y = start.y;

            float zt = 0f;
            while (zt < ultDashDuration)
            {
                zt += Time.deltaTime;
                transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(zt / ultDashDuration));
                if (_trail != null) _trail.SpawnGhost();
                yield return null;
            }
            transform.position = end;

            if (target != null && target.IsAlive)
            {
                target.TakeDamage(ultDashDamage);
                RegisterHit(target);
            }

            _attackReadyAt = Time.time + ultDashCooldown;
            _zipping = false;
        }

        Enemy FindNearestEnemyInRange(float range)
        {
            Enemy best = null;
            float bestDist = float.MaxValue;
            foreach (var e in Enemy.All)
            {
                if (e == null || !e.IsAlive) continue;
                float d = Vector2.Distance(transform.position, e.transform.position);
                if (d > range) continue;
                if (d < bestDist) { bestDist = d; best = e; }
            }
            return best;
        }

        void RegisterHit(Enemy target)
        {
            _comboCount++;

            Vector2 dir = (Vector2)(target.transform.position - transform.position);
            if (dir.sqrMagnitude < 0.0001f) dir = new Vector2(_lastFacing, 0f);
            else dir.Normalize();

            Vector3 hitPos = target.transform.position + new Vector3(0f, hitPositionLift, 0f);
            OnHitLanded?.Invoke(hitPos, dir);
        }

        public void BreakCombo() { _comboCount = 0; }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, ultDashRange);
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.55f);
            Gizmos.DrawWireSphere(transform.position, normalSlashRange);
        }
    }
}
