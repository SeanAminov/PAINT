using UnityEngine;

namespace Slash
{
    // Ranged AI. Kites at a preferred distance, telegraphs the shot with a
    // tint pulse, then lobs an arrow on a parabolic arc with randomized aim,
    // flight time, and gravity so each shot reads differently.
    [RequireComponent(typeof(Enemy))]
    public class RangedAI : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform player;
        public PlayerHealth playerHealth;
        public Transform cart;
        public CarriageHealth cartHealth;

        [Header("Targeting")]
        public bool targetCartOnly;

        [Header("Spacing")]
        public float preferredDistance = 6f;
        public float distanceTolerance = 1.2f;
        public float moveSpeed = 2.6f;
        public float retreatSpeed = 3.2f;

        [Header("Firing")]
        public float fireCooldown = 1.8f;
        // Rangers will not start the telegraph if their target sits beyond
        // this distance; they must walk in closer first.
        public float maxFireDistance = 8f;
        public float telegraphDuration = 0.5f;
        public float telegraphPulseSpeed = 28f;
        public Color telegraphTint = new Color(1f, 0.95f, 0.5f, 1f);

        [Header("Arrow")]
        public Color arrowColor = new Color(0.85f, 0.95f, 1f, 1f);
        public Vector2 arrowSize = new Vector2(0.45f, 1.75f);
        public Vector3 muzzleOffset = new Vector3(0f, 0.45f, 0f);
        public float arrowLifetime = 4f;
        public int arrowDamage = 1;

        [Header("Arc Randomization")]
        public float aimJitterX = 1.6f;
        public float aimJitterY = 0.7f;
        public float flightTimeMin = 1.6f;
        public float flightTimeMax = 2.4f;
        public float gravityMin = 24f;
        public float gravityMax = 34f;
        public float aimLift = 1.2f;

        Enemy _self;
        SpriteRenderer _sr;
        Color _baseColor;
        float _fireCooldownRemaining;
        float _telegraphRemaining;
        bool _telegraphing;

        void Awake()
        {
            _self = GetComponent<Enemy>();
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _baseColor = _sr.color;
        }

        void Update()
        {
            if (_self == null || !_self.IsAlive || player == null) return;

            float dt = Time.deltaTime * TimeControl.enemyTimeScale;

            if (_telegraphing)
            {
                TickTelegraph(dt);
                return;
            }

            RestoreColor();
            TickSpacing(dt);

            if (_fireCooldownRemaining > 0f) _fireCooldownRemaining -= dt;

            if (_fireCooldownRemaining <= 0f && WithinFireRange())
            {
                _telegraphing = true;
                _telegraphRemaining = telegraphDuration;
            }
        }

        bool WithinFireRange()
        {
            var target = ResolveTarget();
            if (target == null) return false;
            return Mathf.Abs(target.position.x - transform.position.x) <= maxFireDistance;
        }

        Transform ResolveTarget()
        {
            bool cartLive = cart != null && cartHealth != null && cartHealth.IsAlive;
            if (!cartLive) return player;
            if (player == null) return cart;

            // Same side-of-player rule as the other AIs.
            if (targetCartOnly || transform.position.x <= player.position.x) return cart;
            return player;
        }

        void TickSpacing(float dt)
        {
            var target = ResolveTarget();
            if (target == null) return;

            float dx = target.position.x - transform.position.x;
            float dist = Mathf.Abs(dx);
            float side = Mathf.Sign(dx);
            if (side == 0f) side = 1f;

            if (dist < preferredDistance - distanceTolerance)
            {
                transform.position += new Vector3(-side * retreatSpeed * dt, 0f, 0f);
            }
            else if (dist > preferredDistance + distanceTolerance)
            {
                transform.position += new Vector3(side * moveSpeed * dt, 0f, 0f);
            }
        }

        void TickTelegraph(float dt)
        {
            _telegraphRemaining -= dt;
            if (_sr != null)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * telegraphPulseSpeed);
                _sr.color = Color.Lerp(_baseColor, telegraphTint, pulse);
            }

            if (_telegraphRemaining <= 0f) Fire();
        }

        void Fire()
        {
            _telegraphing = false;
            _fireCooldownRemaining = fireCooldown;
            RestoreColor();

            var target = ResolveTarget();
            if (target == null) return;

            Vector3 shootPos = transform.position + muzzleOffset;
            AudioCues.PlayArrow(shootPos);

            // Jittered aim, lifted so the arrow flies high and the player has
            // time to read it.
            Vector3 aim = target.position + new Vector3(
                Random.Range(-aimJitterX, aimJitterX),
                Random.Range(-aimJitterY, aimJitterY) + aimLift,
                0f);

            float flightTime = Random.Range(flightTimeMin, flightTimeMax);
            float g = Random.Range(gravityMin, gravityMax);

            float dx = aim.x - shootPos.x;
            float dy = aim.y - shootPos.y;

            // Solve for launch velocity that lands at (dx, dy) after flightTime
            // under gravity g. y(t) = vy*t - 0.5*g*t^2 -> vy = dy/t + 0.5*g*t.
            Vector2 launchVel = new Vector2(
                dx / flightTime,
                dy / flightTime + 0.5f * g * flightTime);

            SpawnArrow(shootPos, launchVel, g);
        }

        void SpawnArrow(Vector3 pos, Vector2 velocity, float gravity)
        {
            var go = new GameObject("EnemyArrow");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(arrowSize.x, arrowSize.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteUtil.Square;
            sr.color = arrowColor;
            sr.sortingOrder = 3;

            var proj = go.AddComponent<EnemyProjectile>();
            proj.player = player;
            proj.playerHealth = playerHealth;
            proj.cart = cart;
            proj.cartHealth = cartHealth;
            proj.damage = arrowDamage;
            proj.lifetime = arrowLifetime;
            proj.velocity = velocity;
            proj.gravity = gravity;
            proj.curlPerSecond = 0f;
            proj.spinSpeed = 0f;
            proj.alignToVelocity = true;
        }

        void RestoreColor()
        {
            if (_sr != null) _sr.color = _baseColor;
        }
    }
}
