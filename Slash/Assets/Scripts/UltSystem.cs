using UnityEngine;
using UnityEngine.InputSystem;

namespace Slash
{
    // Ultimate charge bar. Fills on enemy kills. Press G when full to enter
    // ult mode, which unlocks the moves on UltAbilities for a fixed duration.
    public class UltSystem : MonoBehaviour
    {
        [Header("Charge")]
        public float chargePerKill = 0.15f;
        public float ultDuration = 15f;

        public float Charge { get; private set; }
        public bool IsActive => Time.time < _ultEndsAt;
        public bool IsReady => Charge >= 1f && !IsActive;
        public float ActiveTimeRemaining => Mathf.Max(0f, _ultEndsAt - Time.time);
        public float ActiveFraction => IsActive ? ActiveTimeRemaining / ultDuration : 0f;

        float _ultEndsAt;

        void OnEnable() { Enemy.OnAnyEnemyDied += OnEnemyDied; }
        void OnDisable() { Enemy.OnAnyEnemyDied -= OnEnemyDied; }

        void Update()
        {
            if (IsActive) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.vKey.wasPressedThisFrame && IsReady)
            {
                Activate();
            }
        }

        void OnEnemyDied(Vector3 _)
        {
            if (IsActive) return;
            Charge = Mathf.Clamp01(Charge + chargePerKill);
        }

        void Activate()
        {
            Charge = 0f;
            _ultEndsAt = Time.time + ultDuration;
        }
    }
}
