using UnityEngine;

namespace Slash
{
    // World space HP bar above the cart. Fades out shortly after the cart is
    // back to full so it does not clutter the screen during normal play.
    public class CarriageHealthBar : MonoBehaviour
    {
        [Header("Wiring")]
        public CarriageHealth health;
        public Transform follow;

        [Header("Look")]
        public Vector2 size = new Vector2(2.5f, 0.25f);
        public Vector3 worldOffset = new Vector3(0f, 3.5f, 0f);
        public Color backColor = new Color(0.1f, 0.05f, 0.05f, 0.85f);
        public Color fillColor = new Color(0.4f, 0.95f, 0.45f, 1f);

        [Header("Fade")]
        public float fullFadeDelay = 1.5f;
        public float fullFadeDuration = 0.5f;

        SpriteRenderer _back;
        SpriteRenderer _fill;

        void Awake()
        {
            _back = MakeChild("Back", backColor, 10);
            _fill = MakeChild("Fill", fillColor, 11);
            // Hidden until LateUpdate proves we should be visible. Prevents a
            // one-frame flash before the first sizing/fade pass runs.
            _back.enabled = false;
            _fill.enabled = false;
        }

        void LateUpdate()
        {
            if (health == null) return;

            if (follow != null) transform.position = follow.position + worldOffset;

            float fraction = health.Fraction;

            _back.transform.localPosition = Vector3.zero;
            _back.transform.localScale = new Vector3(size.x, size.y, 1f);

            float fillWidth = size.x * fraction;
            float xOffset = -(size.x - fillWidth) * 0.5f;
            _fill.transform.localPosition = new Vector3(xOffset, 0f, 0f);
            _fill.transform.localScale = new Vector3(fillWidth, size.y, 1f);

            float alpha = 1f;
            if (fraction >= 1f)
            {
                float t = health.SecondsSinceDamage;
                if (t > fullFadeDelay)
                {
                    alpha = 1f - Mathf.Clamp01((t - fullFadeDelay) / Mathf.Max(0.0001f, fullFadeDuration));
                }
            }

            bool visible = alpha > 0.001f;
            _back.enabled = visible;
            _fill.enabled = visible;

            var b = backColor; b.a *= alpha; _back.color = b;
            var f = fillColor; f.a *= alpha; _fill.color = f;
        }

        SpriteRenderer MakeChild(string name, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteUtil.Square;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }
    }
}
