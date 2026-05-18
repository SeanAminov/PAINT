using UnityEngine;
using UnityEngine.UI;

namespace Slash
{
    // Quick screen overlay for the wave attack impact frame. Flips white then
    // black for a moment with no time freeze.
    public class MonochromeFlash : MonoBehaviour
    {
        [Header("Wiring")]
        public Image overlay;

        [Header("Flash")]
        public Color firstColor = new Color(1f, 1f, 1f, 0.55f);
        public Color secondColor = new Color(0f, 0f, 0f, 0.55f);
        public float duration = 0.18f;

        float _endsAt;
        bool _flipped;

        void Awake()
        {
            if (overlay != null) overlay.enabled = false;
        }

        void Update()
        {
            if (overlay == null || !overlay.enabled) return;

            if (Time.unscaledTime >= _endsAt)
            {
                overlay.enabled = false;
                return;
            }

            float remaining = _endsAt - Time.unscaledTime;
            if (!_flipped && remaining < duration * 0.5f)
            {
                overlay.color = secondColor;
                _flipped = true;
            }
        }

        public void Trigger()
        {
            if (overlay == null) return;
            overlay.color = firstColor;
            overlay.enabled = true;
            _endsAt = Time.unscaledTime + duration;
            _flipped = false;
        }
    }
}
