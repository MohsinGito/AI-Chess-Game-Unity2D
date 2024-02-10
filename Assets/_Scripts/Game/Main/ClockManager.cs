using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Chess.Game
{
    public class ClockManager : MonoBehaviour
    {

        public event System.Action<bool> ClockTimeout;

        [Header("References")]
        [SerializeField] private TMP_Text timerUIWhite;
        [SerializeField] private TMP_Text timerUIBlack;
        [SerializeField] private Image timeFillWhite;
        [SerializeField] private Image timeFillBlack;

        [Header("Clock Settings")]
        public int lowTimeThreshold = 10;
        [Range(0, 1)]
        public float inactiveAlpha = 0.75f;
        [Range(0, 1)]
        public float decimalFontSizeMultiplier = 0.75f;
        public Color lowTimeCol;

        private bool whiteToPlay;
        private bool isRunningWhite;
        private bool isRunningBlack;
        private float initialTime;
        private float secondsRemainingWhite;
        private float secondsRemainingBlack;
        private int increment;

        void Start()
        {
            SetPaused(true, true);
            SetPaused(false, true);
        }

        public void StartClocks(bool whiteToPlay, int minutesBase, int incrementSeconds)
        {
            this.whiteToPlay = whiteToPlay;
            this.increment = incrementSeconds;

            initialTime = minutesBase * 60;
            secondsRemainingWhite = secondsRemainingBlack = initialTime;
            timeFillWhite.fillAmount = timeFillBlack.fillAmount = 1f;

            UpdateClockUI(timerUIWhite, secondsRemainingWhite);
            UpdateClockUI(timerUIBlack, secondsRemainingBlack);
            isRunningWhite = isRunningBlack = false;
            SetPaused(whiteToPlay, false);
        }

        public void ToggleClock()
        {
            if (whiteToPlay)
            {
                secondsRemainingWhite += increment;
                isRunningWhite = false;
            }
            else
            {
                secondsRemainingBlack += increment;
                isRunningBlack = false;
            }

            whiteToPlay = !whiteToPlay;
            SetPaused(whiteToPlay, false);
        }

        public void StopClocks()
        {
            SetPaused(true, true);
            SetPaused(false, true);
        }

        private void Update()
        {
            if (isRunningWhite)
            {
                UpdateClock(ref secondsRemainingWhite, timerUIWhite, timeFillWhite);
            }
            if (isRunningBlack)
            {
                UpdateClock(ref secondsRemainingBlack, timerUIBlack, timeFillBlack);
            }
        }

        private void UpdateClock(ref float secondsRemaining, TMP_Text timerUI, Image timeFill)
        {
            secondsRemaining -= Time.deltaTime;
            if (secondsRemaining <= 0)
            {
                secondsRemaining = 0;
                ClockTimeout?.Invoke(whiteToPlay);
                SetPaused(true, whiteToPlay);
            }
            UpdateClockUI(timerUI, secondsRemaining);
            UpdateTimeFill(timeFill, secondsRemaining);
        }

        private void UpdateClockUI(TMP_Text timerUI, float seconds)
        {
            int numMinutes = (int)(seconds / 60);
            int numSeconds = (int)(seconds % 60);
            timerUI.text = $"{numMinutes:00}:{numSeconds:00}";

            if (seconds <= lowTimeThreshold)
            {
                int dec = (int)((seconds - numSeconds) * 10);
                float size = timerUI.fontSize * decimalFontSizeMultiplier;
                timerUI.text += $"<size={size}>.{dec}</size>";
                timerUI.color = lowTimeCol;
            }
            else
            {
                timerUI.color = Color.white;
            }
        }

        private void UpdateTimeFill(Image timeFill, float secondsRemaining)
        {
            timeFill.fillAmount = 1 - (secondsRemaining / initialTime);
        }

        private void SetPaused(bool whiteClock, bool isPaused)
        {
            if (whiteClock)
            {
                isRunningWhite = !isPaused;
                timerUIWhite.color = new Color(1, 1, 1, isPaused ? inactiveAlpha : 1);
                timeFillWhite.gameObject.SetActive(!isPaused);
            }
            else
            {
                isRunningBlack = !isPaused;
                timerUIBlack.color = new Color(1, 1, 1, isPaused ? inactiveAlpha : 1);
                timeFillBlack.gameObject.SetActive(!isPaused);
            }
        }

        private void AddTime(float seconds, int incrementSeconds)
        {
            if (whiteToPlay)
            {
                secondsRemainingWhite += incrementSeconds;
            }
            else
            {
                secondsRemainingBlack += incrementSeconds;
            }
        }
    }
}