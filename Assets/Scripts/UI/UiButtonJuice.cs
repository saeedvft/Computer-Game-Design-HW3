using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChickenShooter
{
    /// <summary>
    /// Small hover/press scale feedback for menu buttons.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UiButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        public float hoverScale = 1.05f;
        public float pressScale = 0.97f;

        private Vector3 _baseScale = Vector3.one;
        private bool _hover;
        private bool _pressed;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _baseScale = transform.localScale;
        }

        private void OnDisable()
        {
            transform.localScale = _baseScale;
            _hover = false;
            _pressed = false;
        }

        private void Update()
        {
            if (_button != null && !_button.interactable)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, _baseScale, 12f * Time.unscaledDeltaTime);
                return;
            }

            float target = _pressed ? pressScale : _hover ? hoverScale : 1f;
            Vector3 scale = _baseScale * target;
            transform.localScale = Vector3.Lerp(transform.localScale, scale, 14f * Time.unscaledDeltaTime);
        }

        public void OnPointerEnter(PointerEventData eventData) => _hover = true;
        public void OnPointerExit(PointerEventData eventData) => _hover = false;
        public void OnPointerDown(PointerEventData eventData) => _pressed = true;
        public void OnPointerUp(PointerEventData eventData) => _pressed = false;
        public void OnSelect(BaseEventData eventData) => _hover = true;
        public void OnDeselect(BaseEventData eventData) => _hover = false;
    }
}
