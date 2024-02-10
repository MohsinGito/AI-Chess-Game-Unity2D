using TMPro;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using Chess.Game;

public class MenuUi : MonoBehaviour
{

    [SerializeField] private CanvasGroup startingScreen;
    [SerializeField] private CanvasGroup newGameScreen;
    [SerializeField] private CanvasGroup difficultySelectionScreen;
    [SerializeField] private TMP_Text perMoveDurationText;
    [SerializeField] private GameSettings gameSettings;

    private List<int> perMoveDurations;
    private int currentPerMoveDuration;

    private void Awake()
    {
        perMoveDurations = new List<int> { 1, 2, 3, 5, 10, 15, 30 };
        currentPerMoveDuration = 4;

        DisplayPerMoveDuration();
    }

    #region Button Events

    public void OnStartButtonClick()
    {
        FadeScreens(startingScreen, newGameScreen);
    }

    public void OnSettingsButtonClick()
    {
        OverlayUiManager.Instance.SettingsUI.Display();
    }

    public void OnPerMoveDurationButtonClick()
    {
        currentPerMoveDuration = currentPerMoveDuration + 1 >= perMoveDurations.Count ? 0 : currentPerMoveDuration + 1;
        DisplayPerMoveDuration();
    }

    public void OnVsFriendButtonClick()
    {
        gameSettings.Duration = perMoveDurations[currentPerMoveDuration];
        gameSettings.Mode = GameMode.NONE;
        gameSettings.Opponent = GameManager.PlayerType.Human;
        OverlayUiManager.Instance.LoadScene("Gameplay");
    }

    public void OnVsBotButtonClick()
    {
        FadeScreens(newGameScreen, difficultySelectionScreen);
    }

    public void OnDiffcultyModeButtonClick(int _difficulty)
    {
        gameSettings.Duration = perMoveDurations[currentPerMoveDuration];
        gameSettings.Mode = (GameMode)_difficulty;
        gameSettings.Opponent = GameManager.PlayerType.AI;
        OverlayUiManager.Instance.LoadScene("Gameplay");
    }

    public void OnBackToStartingScreenButtonClick()
    {
        FadeScreens(newGameScreen, startingScreen);
    }

    public void OnBackToNewGameButtonClick()
    {
        FadeScreens(difficultySelectionScreen, newGameScreen);
    }

    #endregion

    private void FadeScreens(CanvasGroup _prevScreen, CanvasGroup _nextScreen)
    {
        _nextScreen.alpha = 0;
        _nextScreen.gameObject.SetActive(true);
        _nextScreen.DOFade(1, 0.25f);
        _prevScreen.DOFade(0, 0.25f).OnComplete(() => _prevScreen.gameObject.SetActive(false));
    }

    private void DisplayPerMoveDuration()
    {
        perMoveDurationText.text = perMoveDurations[currentPerMoveDuration] + (perMoveDurations[currentPerMoveDuration] == 1 ? "min" : "mins");
    }

}

public enum GameMode
{
    EASY = 0,
    MEDIUM = 1,
    HARD = 2,
    NONE = 3
}