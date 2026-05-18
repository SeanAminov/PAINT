using UnityEngine;

namespace Slash
{
    // Chance-based heart drop on enemy death. Only fires when the player is
    // below max HP. Heart sprite is built procedurally from a pixel pattern.
    public class HeartSpawner : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform player;
        public PlayerHealth playerHealth;

        [Header("Drop")]
        public float dropChance = 0.18f;
        public int healAmount = 1;

        [Header("Visual")]
        public Color heartColor = new Color(1f, 0.25f, 0.4f, 1f);
        public float heartSize = 0.975f;

        [Header("Launch")]
        public float launchSpeedMin = 3f;
        public float launchSpeedMax = 5.5f;
        public float upwardBias = 3f;

        static Sprite _heartSprite;

        void OnEnable() { Enemy.OnAnyEnemyDied += OnEnemyDied; }
        void OnDisable() { Enemy.OnAnyEnemyDied -= OnEnemyDied; }

        void OnEnemyDied(Vector3 pos)
        {
            if (playerHealth == null) return;
            if (playerHealth.CurrentHP >= playerHealth.maxHP) return;
            if (Random.value > dropChance) return;
            SpawnHeart(pos);
        }

        void SpawnHeart(Vector3 pos)
        {
            AudioCues.PlayHeartDrop(pos);

            var go = new GameObject("HeartDrop");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(heartSize, heartSize, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetHeartSprite();
            sr.color = heartColor;
            sr.sortingOrder = 2;

            var heart = go.AddComponent<HeartDrop>();
            heart.player = player;
            heart.playerHealth = playerHealth;
            heart.healAmount = healAmount;

            Vector2 dir = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1f)).normalized;
            float speed = Random.Range(launchSpeedMin, launchSpeedMax);
            heart.initialVelocity = new Vector2(dir.x * speed, dir.y * speed + upwardBias);
        }

        static readonly int[] HeartPattern =
        {
            0,1,1,0,1,1,0,
            1,1,1,1,1,1,1,
            1,1,1,1,1,1,1,
            0,1,1,1,1,1,0,
            0,0,1,1,1,0,0,
            0,0,0,1,0,0,0,
        };
        const int PatternW = 7;
        const int PatternH = 6;
        const int CellPx = 8;

        static Sprite GetHeartSprite()
        {
            if (_heartSprite != null) return _heartSprite;

            int w = PatternW * CellPx;
            int h = PatternH * CellPx;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var transparent = new Color(0f, 0f, 0f, 0f);
            var solid = Color.white;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int patX = x / CellPx;
                    int patY = (PatternH - 1) - (y / CellPx);
                    bool on = HeartPattern[patY * PatternW + patX] == 1;
                    tex.SetPixel(x, y, on ? solid : transparent);
                }
            }

            tex.filterMode = FilterMode.Point;
            tex.Apply();

            _heartSprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, w, h),
                new Vector2(0.5f, 0.5f),
                w);
            return _heartSprite;
        }
    }
}
