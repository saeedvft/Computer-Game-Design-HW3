using System.Collections;
using TMPro;
using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Big centred callout used for round intros, level completion and game
    /// over. Text scales in, holds, then fades out using unscaled time.
    /// </summary>
    public class LevelBanner : MonoBehaviour
    {
        public TMP_Text label;
        public TMP_Text sublabel;
        public CanvasGroup group;

        private Coroutine _routine;

        private void Awake()
        {
            if (group == null) group = GetComponent<CanvasGroup>();
            HideImmediate();
        }

        public void Show(string message, float duration)
        {
            Show(message, string.Empty, duration);
        }

        public void Show(string message, string subtitle, float duration)
        {
            if (label == null || group == null)
            {
                return;
            }

            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(ShowRoutine(message, subtitle, Mathf.Max(0.3f, duration)));
        }

        private IEnumerator ShowRoutine(string message, string subtitle, float duration)
        {
            label.text = message;
            if (sublabel != null)
            {
                sublabel.text = subtitle;
            }

            float elapsed = 0f;
            const float inDuration = 0.22f;

            while (elapsed < inDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / inDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                group.alpha = t;
                label.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.82f, 1f, eased);
                yield return null;
            }

            group.alpha = 1f;
            label.rectTransform.localScale = Vector3.one;

            yield return new WaitForSecondsRealtime(duration);

            elapsed = 0f;
            const float outDuration = 0.3f;

            while (elapsed < outDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / outDuration);
                group.alpha = 1f - t;
                yield return null;
            }

            HideImmediate();
            _routine = null;
        }

        private void HideImmediate()
        {
            if (group != null)
            {
                group.alpha = 0f;
            }

            if (label != null)
            {
                label.text = string.Empty;
                label.rectTransform.localScale = Vector3.one;
            }

            if (sublabel != null)
            {
                sublabel.text = string.Empty;
            }
        }
    }
}
