using System;
using System.Collections.Generic;
using UnityEngine;

namespace Slash
{
    // Wave projectile for the standing slash and the ult charge release.
    // passThrough off: despawn on first hit. passThrough on: clear every
    // enemy in its path.
    public class PlayerSlashWave : MonoBehaviour
    {
        public float speed = 14f;
        public float range = 3f;
        public float radius = 0.7f;
        public int damage = 1;
        public Vector2 direction = new Vector2(1f, 0f);
        public bool passThrough;

        public event Action<Enemy, Vector3, Vector2> OnHit;

        float _traveled;
        SpriteRenderer _sr;
        Color _baseColor;
        readonly HashSet<Enemy> _alreadyHit = new HashSet<Enemy>();

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _baseColor = _sr.color;
        }

        void Update()
        {
            float step = speed * Time.deltaTime;
            transform.position += new Vector3(direction.x, direction.y, 0f) * step;
            _traveled += step;

            // X-only check so chest-height waves still hit feet-pivot enemies.
            for (int i = 0; i < Enemy.All.Count; i++)
            {
                var e = Enemy.All[i];
                if (e == null || !e.IsAlive) continue;
                if (passThrough && _alreadyHit.Contains(e)) continue;

                if (Mathf.Abs(transform.position.x - e.transform.position.x) <= radius)
                {
                    e.TakeDamage(damage);
                    OnHit?.Invoke(e, transform.position, direction);

                    if (!passThrough)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    _alreadyHit.Add(e);
                }
            }

            if (_sr != null)
            {
                float t = Mathf.Clamp01(_traveled / Mathf.Max(0.001f, range));
                var c = _baseColor; c.a = _baseColor.a * (1f - t);
                _sr.color = c;
            }

            if (_traveled >= range) Destroy(gameObject);
        }
    }
}
