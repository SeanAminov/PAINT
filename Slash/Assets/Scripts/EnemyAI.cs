using UnityEngine;

namespace Slash
{
    // Fodder AI. Walks toward the player or the cart and deals contact damage
    // to the player on a cooldown. Cart damage is owned by CarriageDamageZone.
    // Motion and cooldowns scale with TimeControl.enemyTimeScale so time stop
    // slows the AI tick the same as visuals.
    [RequireComponent(typeof(Enemy))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform player;
        public PlayerHealth playerHealth;
        public Transform cart;
        public CarriageHealth cartHealth;

        [Header("Targeting")]
        // Saboteurs ignore the player and march straight at the cart.
        public bool targetCartOnly;

        [Header("Movement")]
        public float moveSpeed = 3.5f;
        public float stopDistance = 0.05f;

        [Header("Contact Damage")]
        public int contactDamage = 1;
        public float contactCooldown = 0.7f;
        public float contactRange = 0.75f;

        float _hitCooldownRemaining;
        Enemy _self;
        Animator _anim;
        SpriteRenderer _sr;

        static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

        void Awake()
        {
            _self = GetComponent<Enemy>();
            _anim = GetComponent<Animator>();
            _sr = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            if (_self == null || !_self.IsAlive) return;

            float dt = Time.deltaTime * TimeControl.enemyTimeScale;

            ResolveTarget(out Transform target, out bool targetingCart);
            if (target == null) return;

            float dx = target.position.x - transform.position.x;
            float dist = Mathf.Abs(dx);
            bool moving = dist > stopDistance;

            if (moving)
            {
                float step = Mathf.Sign(dx) * moveSpeed * dt;
                if (Mathf.Abs(step) > dist - stopDistance)
                {
                    step = Mathf.Sign(dx) * (dist - stopDistance);
                }
                transform.position += new Vector3(step, 0f, 0f);
            }

            if (_anim != null) _anim.SetBool(IsMovingHash, moving);
            if (_sr != null && Mathf.Abs(dx) > 0.001f) _sr.flipX = dx < 0f;

            if (_hitCooldownRemaining > 0f) _hitCooldownRemaining -= dt;

            if (_hitCooldownRemaining <= 0f
                && player != null
                && playerHealth != null
                && !playerHealth.IsInvulnerable)
            {
                float playerDist = Mathf.Abs(player.position.x - transform.position.x);
                if (playerDist <= contactRange)
                {
                    playerHealth.TakeDamage(contactDamage);
                    _hitCooldownRemaining = contactCooldown;
                }
            }
        }

        void ResolveTarget(out Transform target, out bool targetingCart)
        {
            target = player;
            targetingCart = false;

            bool cartLive = cart != null && cartHealth != null && cartHealth.IsAlive;
            if (!cartLive) return;
            if (player == null) { target = cart; targetingCart = true; return; }

            // Past the player on X => commit to the cart. Ahead of the
            // player => stay on the player. Saboteurs always pick the cart.
            if (targetCartOnly || transform.position.x <= player.position.x)
            {
                target = cart;
                targetingCart = true;
            }
        }
    }
}
