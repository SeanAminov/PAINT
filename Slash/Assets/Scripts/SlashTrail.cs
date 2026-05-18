using System.Collections.Generic;
using UnityEngine;

namespace Slash
{
    // Afterimage ghosts during dashes. PlayerController toggles boosted mode
    // during ult so the streak reads more clearly.
    public class SlashTrail : MonoBehaviour
    {
        [Header("Afterimage")]
        public Color trailColor = new Color(0.3f, 0.8f, 1f, 0.5f);
        public float fadeDuration = 0.2f;
        public float spawnInterval = 0.015f;
        public int sortingOrder = -1;

        [Header("Boosted (Ult)")]
        public Color boostedTrailColor = new Color(0.9f, 0.5f, 1f, 0.75f);
        public float boostedFadeDuration = 0.45f;
        public float boostedSpawnInterval = 0.005f;
        public bool boosted;

        float _nextSpawn;
        SpriteRenderer _sr;
        readonly List<Ghost> _ghosts = new List<Ghost>();

        struct Ghost
        {
            public SpriteRenderer renderer;
            public float spawnTime;
        }

        float CurrentFade => boosted ? boostedFadeDuration : fadeDuration;
        Color CurrentColor => boosted ? boostedTrailColor : trailColor;
        float CurrentSpawnInterval => boosted ? boostedSpawnInterval : spawnInterval;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            float fade = CurrentFade;
            Color tint = CurrentColor;

            for (int i = _ghosts.Count - 1; i >= 0; i--)
            {
                var ghost = _ghosts[i];
                float age = Time.time - ghost.spawnTime;

                if (age >= fade)
                {
                    Destroy(ghost.renderer.gameObject);
                    _ghosts.RemoveAt(i);
                    continue;
                }

                float alpha = Mathf.Lerp(tint.a, 0f, age / Mathf.Max(0.0001f, fade));
                var c = ghost.renderer.color;
                c.a = alpha;
                ghost.renderer.color = c;
            }
        }

        public void SpawnGhost()
        {
            if (_sr == null || Time.time < _nextSpawn) return;
            _nextSpawn = Time.time + CurrentSpawnInterval;

            var go = new GameObject("TrailGhost");
            go.transform.position = transform.position;
            go.transform.localScale = transform.lossyScale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _sr.sprite;
            sr.color = CurrentColor;
            sr.sortingOrder = sortingOrder;

            _ghosts.Add(new Ghost { renderer = sr, spawnTime = Time.time });
        }
    }
}
