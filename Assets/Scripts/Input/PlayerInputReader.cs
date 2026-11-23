using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChickenShooter
{
    /// <summary>
    /// The single entry point for player input. Uses only the New Input System
    /// and exposes gameplay-friendly values to the ship, weapon and bomb.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        public InputActionAsset actions;

        private InputActionMap _playerMap;
        private InputAction _moveAction;
        private InputAction _fireAction;
        private InputAction _bombAction;
        private InputAction _pauseAction;

        public Vector2 Move { get; private set; }
        public bool FireHeld => _fireAction != null && _fireAction.IsPressed();
        public bool BombWasPressed => _bombAction != null && _bombAction.WasPressedThisFrame();

        public event Action PausePressed;

        private void Awake()
        {
            if (actions == null)
            {
                actions = Resources.Load<InputActionAsset>("GameInput");
            }

            if (actions == null)
            {
                Debug.LogWarning("[PlayerInputReader] No InputActionAsset assigned.");
                return;
            }

            _playerMap = actions.FindActionMap("Player", false);
            if (_playerMap == null)
            {
                Debug.LogWarning("[PlayerInputReader] The Player action map was not found.");
                return;
            }

            _moveAction = _playerMap.FindAction("Move", false);
            _fireAction = _playerMap.FindAction("Fire", false);
            _bombAction = _playerMap.FindAction("Bomb", false);
            _pauseAction = _playerMap.FindAction("Pause", false);
        }

        private void OnEnable()
        {
            if (_playerMap == null)
            {
                return;
            }

            _playerMap.Enable();
            if (_pauseAction != null)
            {
                _pauseAction.performed += OnPausePerformed;
            }
        }

        private void OnDisable()
        {
            if (_pauseAction != null)
            {
                _pauseAction.performed -= OnPausePerformed;
            }

            if (_playerMap != null)
            {
                _playerMap.Disable();
            }
        }

        private void Update()
        {
            Move = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            PausePressed?.Invoke();
        }
    }
}
