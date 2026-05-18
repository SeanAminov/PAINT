using UnityEngine;

namespace Slash
{
    // Endless mode forward pressure. Drifts right at moveSpeed and clamps both
    // the player and any live enemies to the cart's right edge, so retreating
    // past the cart is impossible. Half width comes from the SpriteRenderer's
    // bounds (or halfWidthOverride if set) so the clamp tracks the visible
    // body rather than the transform's middle.
    public class Carriage : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform player;
        public PlayerHealth playerHealth;
        public CarriageHealth cartHealth;

        [Header("Movement")]
        public float moveSpeed = 2.5f;
        public bool slowedByTimeStop = false;

        [Header("Push")]
        // Gap between the cart's right edge and the player clamp line.
        public float pushLineOffset = 1.5f;
        // Negative falls back to SpriteRenderer bounds. Set positive to pin
        // the half width manually when the visible body and collider disagree.
        public float halfWidthOverride = -1f;

        [Header("Startup")]
        public float startDelay = 1f;

        float _startTime;
        SpriteRenderer _sr;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            _startTime = Time.time;
        }

        void Update()
        {
            if (!CanRun()) return;
            if (Time.time < _startTime + startDelay) return;

            float dt = slowedByTimeStop
                ? Time.deltaTime * TimeControl.enemyTimeScale
                : Time.deltaTime;

            transform.position += new Vector3(moveSpeed * dt, 0f, 0f);
        }

        void LateUpdate()
        {
            if (!CanRun()) return;

            float halfWidth = ResolveHalfWidth();
            float pushLineX = transform.position.x + halfWidth + pushLineOffset;

            if (player != null && player.position.x < pushLineX)
            {
                var p = player.position;
                p.x = pushLineX;
                player.position = p;
            }

            // The cart is a wall: no enemy slips past the right edge.
            float enemyClampX = transform.position.x + halfWidth;
            for (int i = 0; i < Enemy.All.Count; i++)
            {
                var e = Enemy.All[i];
                if (e == null || !e.IsAlive) continue;
                if (e.transform.position.x < enemyClampX)
                {
                    var ep = e.transform.position;
                    ep.x = enemyClampX;
                    e.transform.position = ep;
                }
            }
        }

        bool CanRun()
        {
            if (playerHealth != null && !playerHealth.IsAlive) return false;
            if (cartHealth != null && !cartHealth.IsAlive) return false;
            return true;
        }

        float ResolveHalfWidth()
        {
            if (halfWidthOverride > 0f) return halfWidthOverride;
            if (_sr != null && _sr.sprite != null) return _sr.bounds.size.x * 0.5f;
            return transform.lossyScale.x * 0.5f;
        }
    }
}
