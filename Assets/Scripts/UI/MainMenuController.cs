using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ChickenShooter
{
    public class MainMenuController : MonoBehaviour
    {
        public Button newGameButton;
        public Button continueButton;
        public Button quitButton;
        public InputSystemUIInputModule uiModule;

        private void Start()
        {
            Time.timeScale = 1f;

            if (uiModule != null)
            {
                uiModule.AssignDefaultActions();
            }

            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(StartNewGame);
            }

            if (continueButton != null)
            {
                // The JSON save system is part of the next phase.
                continueButton.interactable = false;
                continueButton.onClick.AddListener(ContinueGame);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitGame);
            }
        }

        private void StartNewGame()
        {
            GameManager.Instance.StartNewGame();
        }

        private void ContinueGame()
        {
            Debug.Log("[MainMenu] Continue will be enabled by the save/load system.");
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
