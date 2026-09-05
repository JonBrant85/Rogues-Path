using System;
using DG.Tweening;
using DuloGames.UI;
using DuloGames.UI.Tweens;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Rogues_Path.UI {
    public class UIHealthDisplay : MonoBehaviour {
        public CanvasGroup CanvasGroup;
        public Text UnitNameText;
        public UIProgressBar ProgressBar;
        public Text ProgressBarText;
        public TweenEasing Easing = TweenEasing.InOutQuint;
        public Test_UIProgressBar.TextVariant TextVariant = Test_UIProgressBar.TextVariant.ValueMax;
        public float TextValue = 100f;
        public string TextValueFormat = "0.#";
        [NonSerialized] private readonly TweenRunner<FloatTween> floatTweenRunner = new();

        private void Awake() {
            floatTweenRunner.Init(this);
        }

        private void OnDisable() {
            floatTweenRunner.StopTween();
        }

        public void SetHealth(float currentHealth, float maximumHealth, bool animate = true) {
            TextValue = Mathf.Max(0f, maximumHealth);
            TextVariant = Test_UIProgressBar.TextVariant.ValueMax;
            currentHealth = Mathf.Clamp(currentHealth, 0f, TextValue);
            float amount = TextValue > 0f ? currentHealth / TextValue : 0f;

            if (animate && isActiveAndEnabled) {
                TweenFillAmount(amount);
            }
            else {
                floatTweenRunner.StopTween();
                SetFillAmount(amount);
            }
        }

        public void TweenFillAmount(float amount) {
            StartTween(Mathf.Clamp01(amount), 0.25f);
        }

        public void Show() {
            CanvasGroup.DOFade(1, 0.5f)
                .OnComplete(
                    () => {
                        CanvasGroup.blocksRaycasts = true;
                        CanvasGroup.interactable = true;
                    });
        }

        public void Hide() {
            CanvasGroup.DOFade(0, 0.5f)
                .OnComplete(
                    () => {
                        CanvasGroup.blocksRaycasts = false;
                        CanvasGroup.interactable = false;
                    });
        }

        private void SetFillAmount(float amount) {
            if (ProgressBar == null)
                return;

            amount = Mathf.Clamp01(amount);
            ProgressBar.fillAmount = amount;

            ProgressBarText.text = TextVariant switch {
                Test_UIProgressBar.TextVariant.Percent => Mathf.RoundToInt(amount * 100f) + "%",
                Test_UIProgressBar.TextVariant.Value => (TextValue * amount).ToString(TextValueFormat),
                Test_UIProgressBar.TextVariant.ValueMax => (TextValue * amount).ToString(TextValueFormat) + "/" + TextValue.ToString(TextValueFormat),
                _ => ProgressBarText.text
            };

        }

        private void StartTween(float targetFloat, float duration) {
            var floatTween = new FloatTween {
                duration = duration,
                startFloat = ProgressBar.fillAmount,
                targetFloat = targetFloat
            };

            floatTween.AddOnChangedCallback(SetFillAmount);
            //floatTween.AddOnFinishCallback(OnTweenFinished);
            floatTween.ignoreTimeScale = true;
            floatTween.easing = Easing;
            floatTweenRunner.StartTween(floatTween);
        }
    }
}
