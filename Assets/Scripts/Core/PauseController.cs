using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Minimal pause handler bound to the Pause input action (Escape).
    /// The HUD phase will draw the pause overlay from PauseChanged.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        public PlayerInputReader input;

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            if (input == null)
            {
                input = GetComponent<PlayerInputReader>();
            }
        }

        private void OnEnable()
        {
            if (input != null)
            {
                input.PausePressed += TogglePause;
            }
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.PausePressed -= TogglePause;
            }

            if (IsPaused)
            {
                IsPaused = false;
                Time.timeScale = 1f;
            }
        }

        public void TogglePause()
        {
            IsPaused = !IsPaused;
            Time.timeScale = IsPaused ? 0f : 1f;
            GameEvents.RaisePauseChanged(IsPaused);
            Debug.Log(IsPaused ? "[Pause] Game paused." : "[Pause] Game resumed.");
        }
    }
}
