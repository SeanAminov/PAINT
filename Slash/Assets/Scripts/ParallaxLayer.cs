using UnityEngine;

namespace Slash
{
    // Tiled, infinitely-wrapping sprite strip that scrolls with a tracked
    // transform. parallaxFactor 0 = world-locked (foreground), 1 = camera-
    // locked (sticks to screen). Backgrounds usually want 0.7 to 0.95.
    public class ParallaxLayer : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform tracking;
        public Sprite sprite;

        [Header("Parallax")]
        public float parallaxFactor = 0.85f;

        [Header("Layout")]
        public int segmentCount = 5;
        public float yOffset = 0f;
        public float targetHeight = 14f;
        public int sortingOrder = -10;
        public Color tint = Color.white;

        Transform[] _segments;
        float _segmentWidth;
        float _lastTrackingX;

        void Start()
        {
            if (sprite == null) return;

            Vector2 native = sprite.bounds.size;
            float scale = targetHeight / Mathf.Max(0.0001f, native.y);
            _segmentWidth = native.x * scale;

            _segments = new Transform[Mathf.Max(2, segmentCount)];
            int half = _segments.Length / 2;
            float trackingX = tracking != null ? tracking.position.x : 0f;
            _lastTrackingX = trackingX;

            for (int i = 0; i < _segments.Length; i++)
            {
                var go = new GameObject("Segment" + i);
                go.transform.SetParent(transform, false);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.color = tint;
                sr.sortingOrder = sortingOrder;

                go.transform.localScale = new Vector3(scale, scale, 1f);
                go.transform.position = new Vector3(
                    trackingX + (i - half) * _segmentWidth,
                    yOffset,
                    0f);

                _segments[i] = go.transform;
            }
        }

        void LateUpdate()
        {
            if (tracking == null || _segments == null) return;

            float trackingX = tracking.position.x;
            float deltaX = trackingX - _lastTrackingX;
            _lastTrackingX = trackingX;

            float drift = deltaX * parallaxFactor;
            float wrap = _segmentWidth * _segments.Length;
            float halfWrap = wrap * 0.5f;

            for (int i = 0; i < _segments.Length; i++)
            {
                var t = _segments[i];
                Vector3 pos = t.position;
                pos.x += drift;

                float diff = pos.x - trackingX;
                while (diff < -halfWrap)
                {
                    pos.x += wrap;
                    diff = pos.x - trackingX;
                }
                while (diff > halfWrap)
                {
                    pos.x -= wrap;
                    diff = pos.x - trackingX;
                }

                pos.y = yOffset;
                t.position = pos;
            }
        }
    }
}
