using UnityEngine;

namespace Slash
{
    // Heart pickup. Same launch / bob / magnet flow as Coin with a pulse on
    // top. Heals the player by healAmount on contact.
    public class HeartDrop : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform player;
        public PlayerHealth playerHealth;

        [Header("Heal")]
        public int healAmount = 1;

        [Header("Launch")]
        public Vector2 initialVelocity;
        public float launchGravity = 12f;
        public float launchDuration = 0.55f;
        public float launchDamping = 0.92f;

        [Header("Bob and Pulse")]
        public float bobAmplitude = 0.18f;
        public float bobSpeed = 2.5f;
        public float pulseAmplitude = 0.12f;
        public float pulseSpeed = 5f;

        [Header("Magnet")]
        public float attractRadius = 5f;
        public float startSpeed = 3f;
        public float acceleration = 60f;
        public float pickupRadius = 0.55f;

        float _spawnTime;
        float _baseY;
        Vector2 _velocity;
        float _magnetSpeed;
        bool _attracting;
        Vector3 _baseScale;

        void Start()
        {
            _spawnTime = Time.time;
            _velocity = initialVelocity;
            _baseY = transform.position.y;
            _baseScale = transform.localScale;
        }

        void Update()
        {
            float age = Time.time - _spawnTime;
            float pulse = 1f + Mathf.Sin(age * pulseSpeed) * pulseAmplitude;
            transform.localScale = _baseScale * pulse;

            if (_attracting && player != null)
            {
                TickMagnet();
                return;
            }

            if (age < launchDuration)
            {
                TickLaunch();
                return;
            }

            TickBob(age - launchDuration);

            if (player != null
                && Vector2.Distance(transform.position, player.position) <= attractRadius)
            {
                _attracting = true;
                _magnetSpeed = startSpeed;
            }
        }

        void TickLaunch()
        {
            transform.position += new Vector3(_velocity.x, _velocity.y, 0f) * Time.deltaTime;
            _velocity.y -= launchGravity * Time.deltaTime;
            _velocity *= Mathf.Pow(launchDamping, Time.deltaTime * 60f);
            _baseY = transform.position.y;
        }

        void TickBob(float bobTime)
        {
            var p = transform.position;
            p.y = _baseY + Mathf.Sin(bobTime * bobSpeed) * bobAmplitude;
            transform.position = p;
        }

        void TickMagnet()
        {
            Vector3 dir = (player.position - transform.position).normalized;
            _magnetSpeed += acceleration * Time.deltaTime;
            transform.position += dir * _magnetSpeed * Time.deltaTime;

            if (Vector2.Distance(transform.position, player.position) <= pickupRadius)
            {
                AudioCues.PlayHeartGain(transform.position);
                if (playerHealth != null) playerHealth.Heal(healAmount);
                Destroy(gameObject);
            }
        }
    }
}
