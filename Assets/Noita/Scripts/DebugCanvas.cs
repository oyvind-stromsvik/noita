using TMPro;
using TwiiK.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Noita {

    public class DebugCanvas : Singleton<DebugCanvas> {
        
        [SerializeField] private TextMeshProUGUI fallingParticlesCountText;
        // TODO: Should be a vector2 input field.
        [SerializeField] private Toggle gravityToggle;
        [SerializeField] private Slider stickinessSlider;
        [SerializeField] private TextMeshProUGUI stickinessSliderText;
        [SerializeField] private Slider bouncinessSlider;
        [SerializeField] private TextMeshProUGUI bouncinessSliderText;
        
        private Vector2 _initialGravity;
        
        private void Start() {
            _initialGravity = GameManager.Instance.gravity;
        }
        
        public void UpdateFallingParticlesCount(int count) {
            fallingParticlesCountText.text = count.ToString();
        }

        /// <summary>
        /// Callback for the gravity toggle.
        /// </summary>
        public void OnGravityToggleChangedCallback() {
            GameManager.Instance.gravity = gravityToggle.isOn ? _initialGravity : Vector2.zero;
        }
        
        /// <summary>
        /// Callback for the stickiness slider.
        /// </summary>
        public void OnStickinessSliderChangedCallback() {
            GameManager.Instance.stickiness = stickinessSlider.value;
            stickinessSliderText.text = GameManager.Instance.stickiness.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        }
        
        /// <summary>
        /// Callback for the bouncyness slider.
        /// </summary>
        public void OnBouncinessSliderChangedCallback() {
            GameManager.Instance.bounciness = bouncinessSlider.value;
            bouncinessSliderText.text = GameManager.Instance.bounciness.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        }
    }

}
