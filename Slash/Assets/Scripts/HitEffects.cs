using UnityEngine;
using UnityEngine.UI;

namespace Slash
{
    // Hit feedback orchestrator: blood spray and chunks, screen flash, brief
    // time freeze, and camera shake. Subscribes to PlayerController.OnHitLanded.
    public class HitEffects : MonoBehaviour
    {
        [Header("Wiring")]
        public PlayerController player;
        public CameraFollow cameraFollow;
        public Image flashOverlay;
        public UltSystem ult;

        [Header("Shake")]
        // Kills outside ult feel lighter; ult kills get the full shake.
        public float nonUltShakeMultiplier = 0.45f;

        [Header("Screen Flash")]
        public Color flashColor = new Color(1f, 1f, 1f, 0.25f);
        public float flashDuration = 0.05f;

        [Header("Blood Spray")]
        public int sprayCount = 14;
        public float spraySpeedMin = 3f;
        public float spraySpeedMax = 7f;
        public float sprayConeDegrees = 55f;
        public float sprayLifetimeMin = 0.25f;
        public float sprayLifetimeMax = 0.55f;
        public float spraySizeMin = 0.08f;
        public float spraySizeMax = 0.18f;
        public Color bloodColor = new Color(0.8f, 0.1f, 0.1f, 1f);

        [Header("Blood Chunks")]
        public int chunkCount = 5;
        public float chunkSpeedMin = 2f;
        public float chunkSpeedMax = 5f;
        public float chunkSpreadDegrees = 160f;
        public float chunkLifetimeMin = 0.45f;
        public float chunkLifetimeMax = 0.9f;
        public float chunkSizeMin = 0.2f;
        public float chunkSizeMax = 0.35f;

        [Header("Physics")]
        public float bloodGravity = 3.5f;
        public float positionJitter = 0.15f;

        [Header("Hit Freeze")]
        public float freezeDuration = 0.03f;

        ParticleSystem _bloodPS;
        float _flashUntil;
        float _freezeUntil;
        float _savedTimeScale;

        void Start()
        {
            if (player != null) player.OnHitLanded += OnHit;
            _bloodPS = CreateBloodSystem();
        }

        void OnDestroy()
        {
            if (player != null) player.OnHitLanded -= OnHit;
        }

        void Update()
        {
            UpdateFlash();
            UpdateFreeze();
        }

        void UpdateFlash()
        {
            if (flashOverlay == null) return;

            if (Time.unscaledTime < _flashUntil)
            {
                float t = 1f - ((_flashUntil - Time.unscaledTime) / flashDuration);
                var c = flashColor;
                c.a = Mathf.Lerp(flashColor.a, 0f, t);
                flashOverlay.color = c;
                flashOverlay.enabled = true;
            }
            else
            {
                flashOverlay.enabled = false;
            }
        }

        void UpdateFreeze()
        {
            if (_freezeUntil > 0f && Time.unscaledTime >= _freezeUntil)
            {
                Time.timeScale = _savedTimeScale;
                _freezeUntil = 0f;
            }
        }

        void OnHit(Vector3 position, Vector2 direction)
        {
            if (_bloodPS != null)
            {
                EmitSpray(position, direction);
                EmitChunks(position, direction);
            }

            _flashUntil = Time.unscaledTime + flashDuration;

            if (cameraFollow != null)
            {
                bool ultActive = ult != null && ult.IsActive;
                float scale = ultActive ? 1f : nonUltShakeMultiplier;
                cameraFollow.Shake(cameraFollow.shakeIntensity * scale, cameraFollow.shakeDuration);
            }

            if (freezeDuration > 0f)
            {
                _savedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                _freezeUntil = Time.unscaledTime + freezeDuration;
            }
        }

        void EmitSpray(Vector3 position, Vector2 direction)
        {
            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            for (int i = 0; i < sprayCount; i++)
            {
                float angle = (baseAngle + Random.Range(-sprayConeDegrees, sprayConeDegrees)) * Mathf.Deg2Rad;
                float speed = Random.Range(spraySpeedMin, spraySpeedMax);
                EmitParticle(position, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed,
                    Random.Range(sprayLifetimeMin, sprayLifetimeMax),
                    Random.Range(spraySizeMin, spraySizeMax));
            }
        }

        void EmitChunks(Vector3 position, Vector2 direction)
        {
            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            for (int i = 0; i < chunkCount; i++)
            {
                float angle = (baseAngle + Random.Range(-chunkSpreadDegrees, chunkSpreadDegrees) * 0.5f) * Mathf.Deg2Rad;
                float speed = Random.Range(chunkSpeedMin, chunkSpeedMax);
                EmitParticle(position, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed,
                    Random.Range(chunkLifetimeMin, chunkLifetimeMax),
                    Random.Range(chunkSizeMin, chunkSizeMax));
            }
        }

        void EmitParticle(Vector3 position, Vector2 velocity, float lifetime, float size)
        {
            Vector3 jitter = (Vector3)(Random.insideUnitCircle * positionJitter);
            var ep = new ParticleSystem.EmitParams
            {
                position = position + jitter,
                velocity = new Vector3(velocity.x, velocity.y, 0f),
                startLifetime = lifetime,
                startSize = size,
                startColor = bloodColor,
            };
            _bloodPS.Emit(ep, 1);
        }

        ParticleSystem CreateBloodSystem()
        {
            var go = new GameObject("BloodSplatter");
            go.transform.SetParent(transform, worldPositionStays: false);

            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 0.5f;
            main.startSpeed = 0f;
            main.startSize = 0.15f;
            main.startColor = bloodColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = bloodGravity;
            main.maxParticles = 500;

            var emission = ps.emission;
            emission.enabled = false;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.05f;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0f, 1f, 1f, 0f));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.sortingOrder = 5;

            return ps;
        }
    }
}
