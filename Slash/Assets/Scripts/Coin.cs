using UnityEngine;

namespace Slash
{
    // Coin: launch arc, bob idle, then magnet to the player on close range.
    public class Coin : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform player;

        [Header("Value")]
        public int value = 1;

        [Header("Launch")]
        public Vector2 initialVelocity;
        public float launchGravity = 14f;
        public float launchDuration = 0.45f;
        public float launchDamping = 0.9f;

        [Header("Bob")]
        public float bobAmplitude = 0.15f;
        public float bobSpeed = 3f;
        public float spinSpeed = 220f;

        [Header("Magnet")]
        public float attractRadius = 4.5f;
        public float startSpeed = 3f;
        public float acceleration = 55f;
        public float pickupRadius = 0.4f;

        float _spawnTime;
        float _baseY;
        Vector2 _velocity;
        float _magnetSpeed;
        bool _attracting;

        void Start()
        {
            _spawnTime = Time.time;
            _velocity = initialVelocity;
            _baseY = transform.position.y;
        }

        void Update()
        {
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

            if (_attracting && player != null)
            {
                TickMagnet();
                return;
            }

            float age = Time.time - _spawnTime;
            if (age < launchDuration)
            {
                TickLaunch();
            }
            else
            {
                TickBob(age - launchDuration);
                if (player != null)
                {
                    float d = Vector2.Distance(transform.position, player.position);
                    if (d <= attractRadius)
                    {
                        _attracting = true;
                        _magnetSpeed = startSpeed;
                    }
                }
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
                AudioCues.PlayCoin(transform.position);
                CoinEvents.RaisePickup(value);
                Destroy(gameObject);
            }
        }
    }
}
