using UnityEngine;

namespace Slash
{
    // Enemy projectile. Velocity can curl (spirals), fall under gravity
    // (arrows), and optionally rotate to face travel direction. Hit detection
    // overlaps the arrow against the target's Collider2D bounds so chest /
    // head / cart body shots all register. Honors player invuln and
    // TimeControl.enemyTimeScale.
    public class EnemyProjectile : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform player;
        public PlayerHealth playerHealth;
        public Transform cart;
        public CarriageHealth cartHealth;
        // Forgiveness padding around the cart bounds. Cart collider already
        // covers the body so this stays small.
        public float cartHitPadding = 0.4f;

        [Header("Motion")]
        public Vector2 velocity;
        public float curlPerSecond;
        public float gravity;
        public float spinSpeed = 0f;
        public bool alignToVelocity;

        [Header("Damage")]
        public int damage = 1;
        // Forgiveness padding around the player bounds.
        public float hitRadius = 0.45f;
        public float lifetime = 3f;

        [Header("Body Fallback")]
        // Used only when a target has no Collider2D.
        public float playerBodyHeight = 1.5f;
        public float cartBodyHeight = 1.5f;

        [Header("Ground")]
        // Arrow breaks at this world Y to prevent tunneling into the platform.
        public float groundY = -2f;

        float _lifeRemaining;
        Collider2D _playerCol;
        Collider2D _cartCol;
        bool _resolvedColliders;

        void Start()
        {
            _lifeRemaining = lifetime;
            ResolveColliders();
        }

        void ResolveColliders()
        {
            if (_resolvedColliders) return;
            _resolvedColliders = true;
            if (player != null) _playerCol = player.GetComponent<Collider2D>();
            if (cart != null) _cartCol = cart.GetComponent<Collider2D>();
        }

        void Update()
        {
            float dt = Time.deltaTime * TimeControl.enemyTimeScale;

            if (curlPerSecond != 0f)
            {
                float r = curlPerSecond * dt * Mathf.Deg2Rad;
                float cos = Mathf.Cos(r);
                float sin = Mathf.Sin(r);
                velocity = new Vector2(
                    velocity.x * cos - velocity.y * sin,
                    velocity.x * sin + velocity.y * cos);
            }

            if (gravity != 0f) velocity.y -= gravity * dt;

            transform.position += new Vector3(velocity.x, velocity.y, 0f) * dt;

            if (alignToVelocity && velocity.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            else if (spinSpeed != 0f)
            {
                transform.Rotate(0f, 0f, spinSpeed * dt);
            }

            ResolveColliders();

            if (player != null && playerHealth != null && !playerHealth.IsInvulnerable
                && OverlapsTarget(player, _playerCol, hitRadius, playerBodyHeight))
            {
                playerHealth.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            if (cart != null && cartHealth != null && cartHealth.IsAlive
                && OverlapsTarget(cart, _cartCol, cartHitPadding, cartBodyHeight))
            {
                cartHealth.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            if (transform.position.y <= groundY)
            {
                Destroy(gameObject);
                return;
            }

            _lifeRemaining -= dt;
            if (_lifeRemaining <= 0f) Destroy(gameObject);
        }

        // Prefers Collider2D bounds (full body coverage), falls back to a
        // distance check lifted to body height when no collider is wired.
        bool OverlapsTarget(Transform target, Collider2D col, float padding, float fallbackBodyHeight)
        {
            if (col != null)
            {
                Bounds b = col.bounds;
                b.Expand(padding * 2f);
                Vector3 p = transform.position;
                p.z = b.center.z;
                return b.Contains(p);
            }

            Vector3 bodyCenter = target.position + new Vector3(0f, fallbackBodyHeight * 0.5f, 0f);
            float halfH = Mathf.Max(0.1f, fallbackBodyHeight * 0.5f);
            float dx = transform.position.x - bodyCenter.x;
            float dy = transform.position.y - bodyCenter.y;
            return Mathf.Abs(dx) <= padding && Mathf.Abs(dy) <= halfH + padding;
        }
    }
}
