using UnityEngine;
using UnityEngine.UI;

namespace Slash
{
    // Renders player HP as a row of heart icons. Each slot swaps between the
    // full and empty heart sprite based on CurrentHP. Briefly pulses a heal
    // tint when HP rises so picking up a heart reads clearly.
    public class HealthUI : MonoBehaviour
    {
        [Header("Wiring")]
        public PlayerHealth health;
        public RectTransform container;
        public Sprite fullHeartSprite;
        public Sprite emptyHeartSprite;

        [Header("Layout")]
        public Vector2 boxSize = new Vector2(64f, 64f);
        public float spacing = 10f;

        [Header("Colors")]
        public Color fullColor = Color.white;
        public Color emptyColor = new Color(1f, 1f, 1f, 0.45f);
        public Color healFlashColor = new Color(0.6f, 1f, 0.6f, 1f);
        public float healFlashDuration = 0.4f;

        Image[] _slots;
        int _lastHP;
        float _flashUntil;

        void Start()
        {
            Build();
            if (health != null) _lastHP = health.CurrentHP;
        }

        void Update()
        {
            if (health == null || _slots == null) return;

            int hp = health.CurrentHP;
            if (hp > _lastHP) _flashUntil = Time.time + healFlashDuration;
            _lastHP = hp;

            float flashT = Mathf.Clamp01((_flashUntil - Time.time) / Mathf.Max(0.0001f, healFlashDuration));
            Color filled = Color.Lerp(fullColor, healFlashColor, flashT);

            for (int i = 0; i < _slots.Length; i++)
            {
                bool live = i < hp;
                _slots[i].sprite = live ? fullHeartSprite : emptyHeartSprite;
                _slots[i].color = live ? filled : emptyColor;
            }
        }

        void Build()
        {
            if (container == null || health == null) return;

            int count = Mathf.Max(1, health.maxHP);
            _slots = new Image[count];

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("HP" + i, typeof(RectTransform));
                go.transform.SetParent(container, false);

                var img = go.AddComponent<Image>();
                img.sprite = fullHeartSprite;
                img.color = fullColor;
                img.raycastTarget = false;
                img.preserveAspect = true;

                var rt = (RectTransform)go.transform;
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.sizeDelta = boxSize;
                rt.anchoredPosition = new Vector2(i * (boxSize.x + spacing), 0f);

                _slots[i] = img;
            }
        }
    }
}
