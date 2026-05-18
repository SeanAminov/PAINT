using UnityEngine;

namespace Slash
{
    // Tank AI: chase, telegraph, slam. The slam hitbox locks in front of the
    // tank at telegraph start so the player can dodge behind the tank to
    // avoid it. The slam bypasses iframes / zip invuln so it cannot be
    // soaked. All timers scale with TimeControl.enemyTimeScale.
    [RequireComponent(typeof(Enemy))]
    public class TankAI : MonoBehaviour
    {
        enum State { Chase, Telegraph, Recover }

        [Header("Wiring")]
        public Transform player;
        public PlayerHealth playerHealth;
        public Transform cart;
        public CarriageHealth cartHealth;
        public CameraFollow cameraFollow;

        [Header("Targeting")]
        public bool targetCartOnly;

        [Header("Movement")]
        public float moveSpeed = 1.6f;
        public float slamRange = 3.25f;

        [Header("Slam")]
        public float slamWidth = 4.4f;
        public float slamHeight = 1.8f;
        // Distance in front of the tank's body where the hitbox starts.
        public float slamForwardOffset = 0f;
        // Hitbox bottom relative to the tank's transform Y. 0 = flush with feet.
        public float slamBottomOffset = 0f;
        public int slamDamage = 1;
        public float telegraphDuration = 0.7f;
        public float recoverDuration = 0.6f;
        public float slamCooldown = 1.4f;

        [Header("Visuals")]
        public Color telegraphColor = new Color(1f, 0.25f, 0.2f, 0.6f);
        public Color slamFlashColor = new Color(1f, 1f, 1f, 0.85f);
        public float telegraphPulseSpeed = 22f;
        public float shakeIntensity = 0.5f;
        public float shakeDuration = 0.25f;

        Enemy _self;
        State _state;
        float _stateRemaining;
        float _cooldownRemaining;
        GameObject _hitboxGO;
        SpriteRenderer _hitboxSr;
        float _slamFacing = 1f;
        Transform _slamTarget;
        bool _slamTargetIsCart;

        void Awake()
        {
            _self = GetComponent<Enemy>();
            BuildHitbox();
        }

        void OnDestroy()
        {
            if (_hitboxGO != null) Destroy(_hitboxGO);
        }

        void Update()
        {
            if (_self == null || !_self.IsAlive)
            {
                if (_hitboxGO != null) _hitboxGO.SetActive(false);
                return;
            }

            float dt = Time.deltaTime * TimeControl.enemyTimeScale;

            switch (_state)
            {
                case State.Chase: TickChase(dt); break;
                case State.Telegraph: TickTelegraph(dt); break;
                case State.Recover: TickRecover(dt); break;
            }
        }

        void TickChase(float dt)
        {
            if (_cooldownRemaining > 0f) _cooldownRemaining -= dt;

            ResolveTarget(out Transform target, out bool targetingCart);
            if (target == null) return;

            float dx = target.position.x - transform.position.x;
            float dist = Mathf.Abs(dx);

            if (dist > slamRange)
            {
                transform.position += new Vector3(Mathf.Sign(dx) * moveSpeed * dt, 0f, 0f);
                return;
            }

            if (_cooldownRemaining <= 0f)
            {
                _slamTarget = target;
                _slamTargetIsCart = targetingCart;
                EnterTelegraph();
            }
        }

        void ResolveTarget(out Transform target, out bool targetingCart)
        {
            target = player;
            targetingCart = false;

            bool cartLive = cart != null && cartHealth != null && cartHealth.IsAlive;
            if (!cartLive) return;
            if (player == null) { target = cart; targetingCart = true; return; }

            // Same side-of-player rule as EnemyAI.
            if (targetCartOnly || transform.position.x <= player.position.x)
            {
                target = cart;
                targetingCart = true;
            }
        }

        void EnterTelegraph()
        {
            _state = State.Telegraph;
            _stateRemaining = telegraphDuration;

            if (_slamTarget != null)
            {
                _slamFacing = (_slamTarget.position.x >= transform.position.x) ? 1f : -1f;
            }

            AudioCues.PlayBigGuy(transform.position);

            if (_hitboxGO == null) return;
            _hitboxGO.SetActive(true);
            _hitboxGO.transform.localScale = new Vector3(slamWidth, slamHeight, 1f);
            UpdateHitboxPosition();
        }

        void TickTelegraph(float dt)
        {
            UpdateHitboxPosition();
            _stateRemaining -= dt;

            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * telegraphPulseSpeed);
            var c = telegraphColor;
            c.a = telegraphColor.a * (0.45f + 0.55f * pulse);
            if (_hitboxSr != null) _hitboxSr.color = c;

            if (_stateRemaining <= 0f) ExecuteSlam();
        }

        void ExecuteSlam()
        {
            _state = State.Recover;
            _stateRemaining = recoverDuration;

            UpdateHitboxPosition();

            if (_slamTarget != null)
            {
                Vector3 hitCenter = GetHitboxWorldCenter();
                float halfW = slamWidth * 0.5f;
                float halfH = slamHeight * 0.5f;

                Vector2 b = _slamTarget.position;
                bool inBox = Mathf.Abs(b.x - hitCenter.x) <= halfW
                    && Mathf.Abs(b.y - hitCenter.y) <= halfH + 0.6f;

                if (inBox)
                {
                    if (_slamTargetIsCart)
                    {
                        if (cartHealth != null && cartHealth.IsAlive)
                            cartHealth.TakeDamage(slamDamage);
                    }
                    else if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(slamDamage, ignoreInvuln: true);
                    }
                }
            }

            if (_hitboxSr != null) _hitboxSr.color = slamFlashColor;
            if (cameraFollow != null) cameraFollow.Shake(shakeIntensity, shakeDuration);

            // Second bark on impact, paired with the telegraph bark on windup.
            AudioCues.PlayBigGuy(transform.position);
        }

        void TickRecover(float dt)
        {
            _stateRemaining -= dt;

            if (_hitboxSr != null)
            {
                var c = _hitboxSr.color;
                c.a = Mathf.Max(0f, c.a - dt * 4f);
                _hitboxSr.color = c;
            }

            if (_stateRemaining <= 0f)
            {
                if (_hitboxGO != null) _hitboxGO.SetActive(false);
                _state = State.Chase;
                _cooldownRemaining = slamCooldown;
            }
        }

        Vector3 GetHitboxWorldCenter()
        {
            float tankHalfWidth = transform.lossyScale.x * 0.5f;
            float forward = tankHalfWidth + slamForwardOffset + slamWidth * 0.5f;
            return new Vector3(
                transform.position.x + _slamFacing * forward,
                transform.position.y + slamBottomOffset + slamHeight * 0.5f,
                transform.position.z);
        }

        void UpdateHitboxPosition()
        {
            if (_hitboxGO == null) return;
            _hitboxGO.transform.position = GetHitboxWorldCenter();
        }

        void BuildHitbox()
        {
            _hitboxGO = new GameObject("SlamHitbox");

            _hitboxSr = _hitboxGO.AddComponent<SpriteRenderer>();
            _hitboxSr.sprite = SpriteUtil.Square;
            _hitboxSr.color = new Color(0f, 0f, 0f, 0f);
            _hitboxSr.sortingOrder = 3;

            _hitboxGO.SetActive(false);
        }
    }
}
