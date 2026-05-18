using UnityEngine;

namespace Slash
{
    // Endless mode spawner. Tightens the cadence and raises the alive cap as
    // the run grows. Ranged unlocks first, then tanks, so the player learns
    // one threat at a time before the mix arrives.
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform player;
        public PlayerHealth playerHealth;
        public CameraFollow cameraFollow;
        public Transform cart;
        public CarriageHealth cartHealth;
        public RuntimeAnimatorController fodderAnimator;
        public RuntimeAnimatorController swordsmanAnimator;
        public RuntimeAnimatorController tankAnimator;
        public RuntimeAnimatorController rangedAnimator;

        [Header("Pacing")]
        public float firstSpawnDelay = 2f;
        public float baseSpawnInterval = 1.8f;
        public float minSpawnInterval = 0.45f;
        public float intervalRampPerSecond = 0.012f;
        public int baseMaxAlive = 4;
        public int maxAliveCap = 14;
        public float maxAliveRampSeconds = 25f;
        public float spawnDistance = 24f;
        public bool forwardOnlySpawns = false;
        // Behind spawns need the player far enough ahead that the rear point
        // still sits off screen and right of the cart.
        public float behindSpawnChance = 0.35f;
        public float behindSpawnExtraGap = 6f;
        // Fraction of spawns that bypass the player and head straight for the
        // cart, so the cart is always under some pressure.
        public float cartSaboteurChance = 0.35f;

        [Header("Type Unlocks")]
        public float rangedUnlockTime = 20f;
        public float tankUnlockTime = 50f;
        public float maxRangedWeight = 0.55f;
        public float maxTankWeight = 0.4f;
        public float rangedRampPerSecond = 0.012f;
        public float tankRampPerSecond = 0.008f;

        [Header("Fodder")]
        public Color fodderColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        public Vector2 fodderSize = new Vector2(0.45f, 0.7f);
        public int fodderHP = 1;
        public float fodderSpeed = 3.5f;
        public int fodderContactDamage = 1;

        [Header("Swordsman (Fodder 2)")]
        public float swordsmanSpawnWeight = 0.35f;
        public Vector2 swordsmanSize = new Vector2(0.55f, 0.85f);
        public int swordsmanHP = 1;
        public float swordsmanSpeed = 4f;
        public int swordsmanContactDamage = 1;

        [Header("Tank")]
        public Color tankColor = new Color(0.4f, 0.25f, 0.55f, 1f);
        public Vector2 tankSize = new Vector2(1.4f, 2.2f);
        public int tankHP = 3;

        [Header("Ranged")]
        public Color rangedColor = new Color(0.3f, 0.7f, 0.45f, 1f);
        public Vector2 rangedSize = new Vector2(0.6f, 0.93f);
        public int rangedHP = 1;

        float _startTime;
        float _nextSpawnAt;

        void Start()
        {
            _startTime = Time.time;
            _nextSpawnAt = Time.time + firstSpawnDelay;
        }

        void Update()
        {
            if (player == null) return;

            float elapsed = Time.time - _startTime;
            int maxAlive = Mathf.Clamp(
                baseMaxAlive + Mathf.FloorToInt(elapsed / maxAliveRampSeconds),
                baseMaxAlive,
                maxAliveCap);
            float interval = Mathf.Max(
                minSpawnInterval,
                baseSpawnInterval - elapsed * intervalRampPerSecond);

            if (Time.time < _nextSpawnAt) return;

            if (Enemy.All.Count >= maxAlive)
            {
                _nextSpawnAt = Time.time + 0.3f;
                return;
            }

            SpawnRandom(elapsed);
            _nextSpawnAt = Time.time + interval;
        }

        void SpawnRandom(float elapsed)
        {
            // Only allow types with animators wired so no placeholder blocks
            // ever leak into the run.
            float fodderW = (fodderAnimator != null || swordsmanAnimator != null) ? 1f : 0f;
            float rangedW = 0f;
            float tankW = 0f;

            if (elapsed >= rangedUnlockTime && rangedAnimator != null)
            {
                rangedW = Mathf.Min(maxRangedWeight, (elapsed - rangedUnlockTime) * rangedRampPerSecond);
            }
            if (elapsed >= tankUnlockTime && tankAnimator != null)
            {
                tankW = Mathf.Min(maxTankWeight, (elapsed - tankUnlockTime) * tankRampPerSecond);
            }

            float total = fodderW + rangedW + tankW;
            if (total <= 0f) return;

            Vector3 pos = ChooseSpawnPos();
            float r = Random.value * total;

            if (r < fodderW) SpawnFodder(pos);
            else if (r < fodderW + rangedW) SpawnRangedEnemy(pos);
            else SpawnTankEnemy(pos);
        }

        Vector3 ChooseSpawnPos()
        {
            Vector3 forward = new Vector3(player.position.x + spawnDistance, player.position.y, 0f);

            if (forwardOnlySpawns || cart == null) return forward;

            float gap = player.position.x - cart.position.x;
            float minGap = spawnDistance + behindSpawnExtraGap;
            if (gap > minGap && Random.value < behindSpawnChance)
            {
                return new Vector3(player.position.x - spawnDistance, player.position.y, 0f);
            }

            return forward;
        }

        void SpawnFodder(Vector3 pos)
        {
            // Roll between basic fodder and the faster swordsman variant.
            bool useSwordsman = swordsmanAnimator != null
                && Random.value < swordsmanSpawnWeight;

            string label = useSwordsman ? "Swordsman" : "Fodder";
            Vector2 size = useSwordsman ? swordsmanSize : fodderSize;
            int hp = useSwordsman ? swordsmanHP : fodderHP;
            float speed = useSwordsman ? swordsmanSpeed : fodderSpeed;
            int touchDamage = useSwordsman ? swordsmanContactDamage : fodderContactDamage;
            Color color = useSwordsman ? Color.white : fodderColor;
            var controller = useSwordsman ? swordsmanAnimator : fodderAnimator;

            var go = MakeBlock(label, pos, size, color);
            AttachAnimatorIfAvailable(go, controller);

            var enemy = go.AddComponent<Enemy>();
            enemy.maxHP = hp;

            var ai = go.AddComponent<EnemyAI>();
            ai.player = player;
            ai.playerHealth = playerHealth;
            ai.cart = cart;
            ai.cartHealth = cartHealth;
            ai.moveSpeed = speed;
            ai.contactDamage = touchDamage;
            ai.targetCartOnly = cart != null && Random.value < cartSaboteurChance;
        }

        void SpawnTankEnemy(Vector3 pos)
        {
            Color color = tankAnimator != null ? Color.white : tankColor;
            var go = MakeBlock("Tank", pos, tankSize, color);
            AttachAnimatorIfAvailable(go, tankAnimator);

            var enemy = go.AddComponent<Enemy>();
            enemy.maxHP = tankHP;
            enemy.isBigGuy = true;

            var ai = go.AddComponent<TankAI>();
            ai.player = player;
            ai.playerHealth = playerHealth;
            ai.cart = cart;
            ai.cartHealth = cartHealth;
            ai.cameraFollow = cameraFollow;
            ai.targetCartOnly = cart != null && Random.value < cartSaboteurChance;
        }

        void SpawnRangedEnemy(Vector3 pos)
        {
            Color color = rangedAnimator != null ? Color.white : rangedColor;
            var go = MakeBlock("Ranged", pos, rangedSize, color);
            AttachAnimatorIfAvailable(go, rangedAnimator);

            var enemy = go.AddComponent<Enemy>();
            enemy.maxHP = rangedHP;

            var ai = go.AddComponent<RangedAI>();
            ai.player = player;
            ai.playerHealth = playerHealth;
            ai.cart = cart;
            ai.cartHealth = cartHealth;
            // Rangers stay focused on the player. The side-of-player rule
            // still flips them to the cart if the player blows past them.
            ai.targetCartOnly = false;
        }

        void AttachAnimatorIfAvailable(GameObject go, RuntimeAnimatorController controller)
        {
            if (controller == null) return;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.white;
            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;
        }

        GameObject MakeBlock(string name, Vector3 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteUtil.SquareBottom;
            sr.color = color;
            sr.sortingOrder = 1;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            rb.gravityScale = 0f;

            var bc = go.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;
            bc.size = Vector2.one;
            bc.offset = new Vector2(0f, 0.5f);

            return go;
        }
    }
}
