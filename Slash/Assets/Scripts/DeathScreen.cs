using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Slash
{
    // Fades in a full screen panel when the player or the cart dies. Shows
    // distance traveled and offers Restart (reload scene) and Main Menu.
    public class DeathScreen : MonoBehaviour
    {
        [Header("Wiring")]
        public PlayerHealth health;
        public CarriageHealth cartHealth;
        public DistanceMeter distanceMeter;
        public CanvasGroup canvasGroup;
        public Button restartButton;
        public Button menuButton;
        public Text titleText;
        public Text distanceText;

        [Header("Format")]
        public string distanceFormat = "Distance traveled {0:F1} m";
        public string playerDeathTitle = "YOU DIED";
        public string cartDeathTitle = "CART DESTROYED";

        [Header("Fade")]
        public float fadeIn = 0.6f;
        public float showDelay = 0.4f;

        bool _triggered;
        float _triggerTime;

        void Start()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            if (restartButton != null) restartButton.onClick.AddListener(Restart);
            if (menuButton != null) menuButton.onClick.AddListener(GoToMenu);
        }

        void Update()
        {
            if (!_triggered)
            {
                bool playerDead = health != null && !health.IsAlive;
                bool cartDead = cartHealth != null && !cartHealth.IsAlive;
                if (!playerDead && !cartDead) return;

                _triggered = true;
                _triggerTime = Time.unscaledTime;

                if (titleText != null)
                {
                    titleText.text = playerDead ? playerDeathTitle : cartDeathTitle;
                }
                if (distanceText != null && distanceMeter != null)
                {
                    distanceText.text = string.Format(distanceFormat, distanceMeter.GetMeters());
                }
                return;
            }

            if (canvasGroup == null) return;

            float elapsed = Time.unscaledTime - _triggerTime - showDelay;
            float a = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, fadeIn));
            canvasGroup.alpha = a;
            canvasGroup.interactable = a >= 1f;
            canvasGroup.blocksRaycasts = a > 0f;
        }

        void Restart()
        {
            var current = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(current);
        }

        void GoToMenu()
        {
            SceneLoader.LoadMenu();
        }
    }
}
