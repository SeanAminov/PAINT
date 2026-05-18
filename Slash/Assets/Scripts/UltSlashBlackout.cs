using UnityEngine;

namespace Slash
{
    // Full screen black overlay that fades in while the player is in the
    // Ult Slash animation. Sits between the world and the player so the
    // player sprite reads as a highlighted silhouette on a black field.
    public class UltSlashBlackout : MonoBehaviour
    {
        [Header("Wiring")]
        public SpriteRenderer overlay;

        [Header("Fade")]
        public float fadeInDuration = 0.05f;
        public float fadeOutDuration = 0.15f;

        bool _visible;
        float _alpha;
        Color _baseColor;

        void Awake()
        {
            if (overlay != null)
            {
                _baseColor = overlay.color;
                overlay.enabled = false;
            }
        }

        public void Show() { _visible = true; }
        public void Hide() { _visible = false; }

        void Update()
        {
            float target = _visible ? 1f : 0f;
            float duration = _visible ? fadeInDuration : fadeOutDuration;
            _alpha = Mathf.MoveTowards(
                _alpha,
                target,
                Time.unscaledDeltaTime / Mathf.Max(0.0001f, duration));

            if (overlay == null) return;

            overlay.enabled = _alpha > 0.001f;
            var c = _baseColor;
            c.a = _baseColor.a * _alpha;
            overlay.color = c;
        }
    }
}
