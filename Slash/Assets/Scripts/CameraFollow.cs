using UnityEngine;

namespace Slash
{
    // Follows target on X with SmoothDamp; Y and Z stay locked at the start
    // values. Includes a Perlin-noise shake helper for hit feedback.
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float smoothTime = 0.12f;
        public float offsetX = 0f;

        [Header("Screen Shake")]
        public float shakeIntensity = 0.3f;
        public float shakeDuration = 0.15f;
        public float shakeFrequency = 25f;

        Vector3 _velocity;
        float _baseY;
        float _baseZ;
        float _shakeUntil;
        float _shakeStrength;

        void Start()
        {
            _baseY = transform.position.y;
            _baseZ = transform.position.z;
        }

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = new Vector3(target.position.x + offsetX, _baseY, _baseZ);
            Vector3 pos = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);

            if (Time.unscaledTime < _shakeUntil)
            {
                float remaining = (_shakeUntil - Time.unscaledTime) / shakeDuration;
                float seed = Time.unscaledTime * shakeFrequency;
                pos.x += (Mathf.PerlinNoise(seed, 0f) - 0.5f) * 2f * _shakeStrength * remaining;
                pos.y += (Mathf.PerlinNoise(0f, seed) - 0.5f) * 2f * _shakeStrength * remaining;
            }

            transform.position = pos;
        }

        public void Shake()
        {
            Shake(shakeIntensity, shakeDuration);
        }

        public void Shake(float intensity, float duration)
        {
            _shakeStrength = intensity;
            _shakeUntil = Time.unscaledTime + duration;
        }
    }
}
