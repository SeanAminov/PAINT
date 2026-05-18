using UnityEngine.SceneManagement;

namespace Slash
{
    // Named scene constants plus small helpers for the menu buttons.
    public static class SceneLoader
    {
        public const string MenuScene = "MainMenu";
        public const string EndlessScene = "Endless";
        public const string CreditsScene = "CreditScene";

        public static void LoadMenu() { SceneManager.LoadScene(MenuScene); }
        public static void LoadEndless() { SceneManager.LoadScene(EndlessScene); }
        public static void LoadCredits() { SceneManager.LoadScene(CreditsScene); }
    }
}
