using UnityEngine;
using UnityEngine.InputSystem;

namespace Slash
{
    // Ult-only moveset. X triggers time stop, which slows enemies via
    // TimeControl.enemyTimeScale and pops the TimeStopVFX shockwave.
    public class UltAbilities : MonoBehaviour
    {
        [Header("Wiring")]
        public UltSystem ult;
        public TimeStopVFX vfx;

        [Header("Time Stop")]
        public float timeStopDuration = 3f;
        public float timeStopCooldown = 5f;
        public float slowedScale = 0.1f;

        float _timeStopEndsAt;
        float _timeStopReadyAt;

        public bool TimeStopActive => Time.time < _timeStopEndsAt;
        public float TimeStopCooldownFraction
        {
            get
            {
                if (Time.time >= _timeStopReadyAt) return 0f;
                return Mathf.Clamp01((_timeStopReadyAt - Time.time) / timeStopCooldown);
            }
        }

        void Update()
        {
            TimeControl.enemyTimeScale = TimeStopActive ? slowedScale : 1f;

            if (ult == null || !ult.IsActive) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.xKey.wasPressedThisFrame && Time.time >= _timeStopReadyAt)
            {
                TriggerTimeStop();
            }
        }

        void OnDisable()
        {
            TimeControl.enemyTimeScale = 1f;
        }

        void TriggerTimeStop()
        {
            _timeStopEndsAt = Time.time + timeStopDuration;
            _timeStopReadyAt = Time.time + timeStopCooldown;
            AudioCues.PlayTimeStop(transform.position);
            if (vfx != null) vfx.Trigger(timeStopDuration);
        }
    }
}
