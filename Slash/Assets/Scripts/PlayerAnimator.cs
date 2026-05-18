using UnityEngine;

namespace Slash
{
    // Drives the player's Animator parameters and per-state scaling. Also
    // shows/hides the ult slash blackout overlay and shakes the camera at the
    // moment of impact so the slash reads outside the animation itself.
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("Wiring")]
        public PlayerController controller;
        public PlayerHealth health;
        public SpriteRenderer spriteRenderer;
        public UltSystem ult;
        public UltSlashBlackout blackout;
        public CameraFollow cameraFollow;

        [Header("Tuning")]
        public float movementSpeedThreshold = 0.5f;

        [Header("Death")]
        public Vector3 deathScale = new Vector3(0.4f, 0.4f, 1f);

        [Header("Per-State Scales")]
        // Normalizes minor sprite-size differences between MC controller states.
        // PPU is the primary tuning knob; these stay at 1 unless an art swap
        // needs compensation.
        public float normalScale = 1f;     // Mc Idle, Mc walk
        public float ultModeScale = 1f;    // Ult idle, Ult walk
        public float ultSlashScale = 1f;
        public float basicSlashScale = 1f;

        [Header("Ult Slash Impact")]
        public float ultSlashShake = 0.55f;
        public float ultSlashShakeDuration = 0.08f;

        static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        static readonly int DeadHash = Animator.StringToHash("Dead");
        static readonly int IsUltHash = Animator.StringToHash("IsUlt");

        Animator _anim;
        Vector3 _lastPosition;
        float _facing = 1f;
        bool _deadFired;

        void Awake()
        {
            _anim = GetComponent<Animator>();
            _lastPosition = transform.position;
        }

        void Update()
        {
            if (_anim == null) return;

            if (health != null && !health.IsAlive)
            {
                if (!_deadFired)
                {
                    _anim.SetBool(IsMovingHash, false);
                    _anim.SetTrigger(DeadHash);
                    _deadFired = true;
                    transform.localScale = Vector3.Scale(transform.localScale, deathScale);
                }
                _lastPosition = transform.position;
                return;
            }

            float dt = Mathf.Max(0.0001f, Time.deltaTime);
            Vector3 delta = transform.position - _lastPosition;
            _lastPosition = transform.position;

            float speed = Mathf.Abs(delta.x) / dt;
            _anim.SetBool(IsMovingHash, speed > movementSpeedThreshold);
            _anim.SetBool(IsUltHash, ult != null && ult.IsActive);

            if (Mathf.Abs(delta.x) > 0.001f) _facing = Mathf.Sign(delta.x);
            if (spriteRenderer != null) spriteRenderer.flipX = _facing < 0f;
        }

        void LateUpdate()
        {
            if (_anim == null || _deadFired) return;

            var stateInfo = _anim.GetCurrentAnimatorStateInfo(0);
            bool inUltSlash = stateInfo.IsName("Ult Slash");
            float scale;
            if (inUltSlash) scale = ultSlashScale;
            else if (stateInfo.IsName("Basic Slash")) scale = basicSlashScale;
            else if (stateInfo.IsName("Ult idle") || stateInfo.IsName("Ult walk")) scale = ultModeScale;
            else scale = normalScale;

            transform.localScale = new Vector3(scale, scale, 1f);

            if (blackout != null)
            {
                if (inUltSlash) blackout.Show();
                else blackout.Hide();
            }

            if (inUltSlash && cameraFollow != null)
            {
                cameraFollow.Shake(ultSlashShake, ultSlashShakeDuration);
            }
        }
    }
}
