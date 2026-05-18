using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Slash
{
    // Returns to the main menu from the credits scene. Triggers on the wired
    // back button, Escape, or pressing Enter / Space so the player has a few
    // friendly ways out without watching the full scroll.
    public class CreditsBack : MonoBehaviour
    {
        [Header("Wiring")]
        public Button backButton;

        void Start()
        {
            if (backButton != null) backButton.onClick.AddListener(GoBack);
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.escapeKey.wasPressedThisFrame
                || kb.enterKey.wasPressedThisFrame
                || kb.spaceKey.wasPressedThisFrame)
            {
                GoBack();
            }
        }

        void GoBack()
        {
            AudioCues.PlayMenuClick();
            SceneLoader.LoadMenu();
        }
    }
}
