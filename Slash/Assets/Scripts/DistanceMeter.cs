using UnityEngine;
using UnityEngine.UI;

namespace Slash
{
    // Tracks the furthest X of the wired transform and shows it as meters.
    // Endless points this at the cart so the score reflects cart progress,
    // not how far the player has rushed ahead.
    public class DistanceMeter : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform tracking;
        public Text label;

        [Header("Format")]
        public string format = "Distance {0:F1} m";
        public float metersPerUnit = 1f;

        float _startX;
        float _maxX;
        bool _initialized;

        public float GetMeters()
        {
            if (!_initialized) return 0f;
            return Mathf.Max(0f, (_maxX - _startX) * metersPerUnit);
        }

        void Update()
        {
            if (tracking == null) return;

            if (!_initialized)
            {
                _startX = tracking.position.x;
                _maxX = _startX;
                _initialized = true;
            }

            if (tracking.position.x > _maxX) _maxX = tracking.position.x;

            if (label != null) label.text = string.Format(format, GetMeters());
        }
    }
}
