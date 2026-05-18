using UnityEngine;
using UnityEngine.InputSystem;

namespace Slash
{
    // Ult-only Z-charge wave. Locks the player for chargeDuration with a
    // ramping camera shake. On release: explosion on the player, two waves
    // out left and right, monochrome flash on the screen.
    public class UltChargeAttack : MonoBehaviour
    {
        [Header("Wiring")]
        public UltSystem ult;
        public PlayerController controller;
        public MonochromeFlash flash;
        public CameraFollow cameraFollow;
        public Animator playerAnimator;
        public Sprite waveSprite;
        public Sprite[] explosionFrames;

        [Header("Charge")]
        public float chargeDuration = 0.9f;
        public float chargeCooldown = 5f;
        public float chargeShakeStart = 0.05f;
        public float chargeShakeEnd = 0.45f;

        [Header("Wave")]
        public float waveRange = 25f;
        public float waveTravelSpeed = 22f;
        public float waveHitRadius = 1.4f;
        public int waveDamage = 999;
        public Color waveColor = new Color(1f, 1f, 1f, 0.95f);
        public float waveScale = 1.6f;
        public Vector3 waveSpawnOffset = new Vector3(1f, 1.4f, 0f);

        [Header("Explosion")]
        public Color explosionColor = Color.white;
        public float explosionScale = 2.5f;
        public float explosionFps = 18f;
        public float explosionRadius = 4f;
        public int explosionDamage = 999;
        // Vertical nudge above the player sprite center for the explosion art.
        public float explosionExtraY = 0f;

        [Header("Feedback")]
        public float waveShakeIntensity = 0.7f;
        public float waveShakeDuration = 0.3f;

        public bool IsCharging { get; private set; }
        public bool IsOnCooldown => _cooldownRemaining > 0f;
        public float CooldownFraction => Mathf.Clamp01(_cooldownRemaining / Mathf.Max(0.001f, chargeCooldown));
        public float ChargeFraction =>
            IsCharging ? 1f - Mathf.Clamp01(_chargeRemaining / Mathf.Max(0.001f, chargeDuration)) : 0f;

        static readonly int SlashHash = Animator.StringToHash("Slash");

        float _chargeRemaining;
        float _cooldownRemaining;
        SpriteRenderer _playerSr;

        void Awake()
        {
            _playerSr = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            if (_cooldownRemaining > 0f) _cooldownRemaining -= Time.deltaTime;

            if (IsCharging)
            {
                _chargeRemaining -= Time.deltaTime;
                ApplyChargeShake();
                if (_chargeRemaining <= 0f) Release();
                return;
            }

            if (ult == null || !ult.IsActive) return;
            if (_cooldownRemaining > 0f) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.zKey.wasPressedThisFrame) StartCharge();
        }

        void ApplyChargeShake()
        {
            if (cameraFollow == null) return;
            float intensity = Mathf.Lerp(chargeShakeStart, chargeShakeEnd, ChargeFraction);
            cameraFollow.Shake(intensity, 0.08f);
        }

        void StartCharge()
        {
            IsCharging = true;
            _chargeRemaining = chargeDuration;
            if (playerAnimator != null) playerAnimator.SetTrigger(SlashHash);
        }

        void Release()
        {
            IsCharging = false;
            _cooldownRemaining = chargeCooldown;

            Vector3 playerCenter = ResolvePlayerCenter();
            SpawnExplosion(playerCenter);
            SpawnWave(-1);
            SpawnWave(1);

            if (flash != null) flash.Trigger();
            if (cameraFollow != null) cameraFollow.Shake(waveShakeIntensity, waveShakeDuration);
        }

        Vector3 ResolvePlayerCenter()
        {
            // Sprite bounds keep the explosion centered regardless of which
            // animation sprite is active.
            if (_playerSr != null && _playerSr.sprite != null)
            {
                Vector3 c = _playerSr.bounds.center;
                c.y += explosionExtraY;
                return c;
            }
            return transform.position + new Vector3(0f, 1f + explosionExtraY, 0f);
        }

        void SpawnExplosion(Vector3 origin)
        {
            for (int i = Enemy.All.Count - 1; i >= 0; i--)
            {
                var e = Enemy.All[i];
                if (e == null || !e.IsAlive) continue;
                if (Vector2.Distance(e.transform.position, origin) <= explosionRadius)
                {
                    e.TakeDamage(explosionDamage);
                }
            }

            if (explosionFrames == null || explosionFrames.Length == 0) return;

            var go = new GameObject("ExplosionBurst");
            // Bottom-pivot the burst at the player's feet so it rises through.
            float feetY = _playerSr != null && _playerSr.sprite != null
                ? _playerSr.bounds.min.y
                : transform.position.y;
            go.transform.position = new Vector3(origin.x, feetY, 0f);
            go.transform.localScale = new Vector3(explosionScale, explosionScale, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = explosionFrames[0];
            sr.color = explosionColor;
            sr.sortingOrder = 7;

            var burst = go.AddComponent<SpriteAnimationBurst>();
            burst.frames = explosionFrames;
            burst.fps = explosionFps;
        }

        void SpawnWave(int direction)
        {
            if (waveSprite == null) return;

            var go = new GameObject("WaveBurst");
            Vector3 spawnPos = transform.position + new Vector3(
                direction * waveSpawnOffset.x,
                waveSpawnOffset.y,
                waveSpawnOffset.z);
            go.transform.position = spawnPos;
            go.transform.localScale = new Vector3(waveScale, waveScale, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = waveSprite;
            sr.color = waveColor;
            sr.sortingOrder = 6;
            // Swipe art faces left; flip when traveling right.
            sr.flipX = direction > 0;

            var wave = go.AddComponent<PlayerSlashWave>();
            wave.direction = new Vector2(direction, 0f);
            wave.speed = waveTravelSpeed;
            wave.range = waveRange;
            wave.damage = waveDamage;
            wave.radius = waveHitRadius;
            wave.passThrough = true;
        }
    }
}
