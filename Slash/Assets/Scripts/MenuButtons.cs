using UnityEngine;
using UnityEngine.UI;

namespace Slash
{
    // Hooks the main menu buttons to the scene loader.
    public class MenuButtons : MonoBehaviour
    {
        [Header("Wiring")]
        public Button endlessButton;
        public Button creditsButton;

        void Start()
        {
            if (endlessButton != null) endlessButton.onClick.AddListener(OnEndlessClicked);
            if (creditsButton != null) creditsButton.onClick.AddListener(OnCreditsClicked);
        }

        void OnEndlessClicked()
        {
            AudioCues.PlayMenuClick();
            SceneLoader.LoadEndless();
        }

        void OnCreditsClicked()
        {
            AudioCues.PlayMenuClick();
            SceneLoader.LoadCredits();
        }
    }
}
