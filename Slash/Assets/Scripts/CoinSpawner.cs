using UnityEngine;

namespace Slash
{
    // Drops a small burst of coins at every enemy death. Subscribes to
    // Enemy.OnAnyEnemyDied; each coin arcs outward then settles into bob.
    public class CoinSpawner : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform player;

        [Header("Drop")]
        public int coinsPerEnemy = 3;
        public int coinValue = 1;

        [Header("Visual")]
        public Color coinColor = new Color(1f, 0.85f, 0.2f, 1f);
        public float coinSize = 0.45f;

        [Header("Launch")]
        public float launchSpeedMin = 3f;
        public float launchSpeedMax = 6f;
        public float upwardBias = 2.5f;

        static Sprite _sharedSprite;

        void OnEnable() { Enemy.OnAnyEnemyDied += OnEnemyDied; }
        void OnDisable() { Enemy.OnAnyEnemyDied -= OnEnemyDied; }

        void OnEnemyDied(Vector3 pos)
        {
            for (int i = 0; i < coinsPerEnemy; i++)
            {
                SpawnCoin(pos);
            }
        }

        void SpawnCoin(Vector3 pos)
        {
            var go = new GameObject("Coin");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(coinSize, coinSize, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetSprite();
            sr.color = coinColor;
            sr.sortingOrder = 2;

            var coin = go.AddComponent<Coin>();
            coin.player = player;
            coin.value = coinValue;

            Vector2 dir = new Vector2(Random.Range(-1f, 1f), Random.Range(0.4f, 1f)).normalized;
            float speed = Random.Range(launchSpeedMin, launchSpeedMax);
            coin.initialVelocity = new Vector2(dir.x * speed, dir.y * speed + upwardBias);
        }

        static Sprite GetSprite()
        {
            if (_sharedSprite != null) return _sharedSprite;
            var tex = new Texture2D(32, 32);
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    float dx = x - 15.5f;
                    float dy = y - 15.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    tex.SetPixel(x, y, d <= 15f ? Color.white : new Color(0, 0, 0, 0));
                }
            }
            tex.filterMode = FilterMode.Bilinear;
            tex.Apply();
            _sharedSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
            return _sharedSprite;
        }
    }
}
