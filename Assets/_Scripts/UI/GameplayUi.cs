using TMPro;
using Chess.Game;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUi : MonoBehaviour
{

    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameSettings gameSettings;

    [Header("Ui Refs")]
    [SerializeField] private Image timeFill;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text resultText;

    [Header("PopUp Panels")]
    [SerializeField] private CanvasGroup resignPanel;
    [SerializeField] private CanvasGroup vsPlayerWinPanel;
    [SerializeField] private CanvasGroup vsBotWinPanel;
    [SerializeField] private CanvasGroup vsBotDefeatPanel;
    [SerializeField] private CanvasGroup acceptDrawPanel;
    [SerializeField] private CanvasGroup matchDrawPanel;

    public void OnPuaseButtonClick()
    {
        gameManager.GamePaused();
        DisplayPanel(resignPanel);
    }

    public void OnContinueButtonClick()
    {
        gameManager.GameResumed();
        HidePanel(resignPanel);
    }

    public void OnResignButtonClicked()
    {
        gameManager.GamePaused();
        if (gameSettings.Opponent == GameManager.PlayerType.AI)
        {
            DisplayPanel(vsBotDefeatPanel);
            resultText.text = "You Loose";
        }
        else
        {
            DisplayPanel(vsPlayerWinPanel);
            resultText.text = (gameManager.isPlayer1Turn ? "Player 2" : " Player 1") + " Win";
        }
    }

    public void ReturnToMenu()
    {
        AdsManager.Instance.DisplayInterstitialAd(MoveToMenuScene, MoveToMenuScene);
    }

    public void RestartGame()
    {
        HideAllPanels();
        resultText.text = string.Empty;
        gameManager.GameResumed();
        gameManager.RestartGame();
    }

    public void OfferDraw()
    {
        if (gameSettings.Opponent == GameManager.PlayerType.AI)
            return;

        HideAllPanels();
        DOVirtual.DelayedCall(0.25f, () => DisplayPanel(acceptDrawPanel));
    }

    public void OnDrawAccepted()
    {
        HidePanel(acceptDrawPanel);
        DisplayPanel(matchDrawPanel);
    }

    public void OnDrawRejected()
    {
        HidePanel(acceptDrawPanel);
    }

    public void OnGameEnded(GameResult.Result result)
    {
        gameManager.GamePaused();
        DOVirtual.DelayedCall(1f, () => DisplayResult(result));
    }

    private void DisplayResult(GameResult.Result result)
    {
        if (result is GameResult.Result.WhiteIsMated or GameResult.Result.BlackIsMated)
        {
            if (gameSettings.Opponent == GameManager.PlayerType.AI)
            {
                resultText.text = (result == GameResult.Result.WhiteIsMated ? "Bot" : "You") + " Win By Checkmate";
                DisplayPanel(result == GameResult.Result.WhiteIsMated ? vsBotDefeatPanel : vsBotWinPanel);
            }
            else
            {
                resultText.text = (result == GameResult.Result.WhiteIsMated ? "Player 2" : "Player 1") + " Win By Checkmate";
                DisplayPanel(vsPlayerWinPanel);
            }
        }
        else if (result is GameResult.Result.WhiteTimeout or GameResult.Result.BlackTimeout)
        {
            if (gameSettings.Opponent == GameManager.PlayerType.AI)
            {
                resultText.text = (result == GameResult.Result.WhiteTimeout ? "Bot" : "You") + " Win On Time";
                DisplayPanel(result == GameResult.Result.WhiteIsMated ? vsBotDefeatPanel : vsBotWinPanel);
            }
            else
            {
                resultText.text = (result == GameResult.Result.WhiteTimeout ? "Player 2" : "Player 1") + " Wins On Time";
                DisplayPanel(vsPlayerWinPanel);
            }
        }
        else if (result == GameResult.Result.FiftyMoveRule)
        {
            resultText.text = "Draw By 50 Move Rule";
            DisplayPanel(matchDrawPanel);
        }
        else if (result == GameResult.Result.Repetition)
        {
            resultText.text = "Draw By 3-Fold Repetition";
            DisplayPanel(matchDrawPanel);
        }
        else if (result == GameResult.Result.Stalemate)
        {
            resultText.text = "Draw By Stalemate";
            DisplayPanel(matchDrawPanel);
        }
        else if (result == GameResult.Result.InsufficientMaterial)
        {
            resultText.text = "Draw Due To Insufficient Material";
            DisplayPanel(matchDrawPanel);
        }
        else
        {
            resultText.text = string.Empty;
            DisplayPanel(matchDrawPanel);
        }
    }

    private void MoveToMenuScene()
    {
        OverlayUiManager.Instance.LoadScene("Menu");
    }

    private void DisplayPanel(CanvasGroup _panel)
    {
        _panel.alpha = 0;
        _panel.gameObject.SetActive(true);
        _panel.DOFade(1, 0.25f);
    }

    private void HidePanel(CanvasGroup _panel)
    {
        _panel.DOFade(0, 0.25f).OnComplete(() =>
        {
            _panel.gameObject.SetActive(false);
        });
    }

    private void HideAllPanels()
    {
        if (resignPanel.gameObject.activeSelf)
            HidePanel(resignPanel);
        if (vsPlayerWinPanel.gameObject.activeSelf)
            HidePanel(vsPlayerWinPanel);
        if (vsBotWinPanel.gameObject.activeSelf)
            HidePanel(vsBotWinPanel);
        if (vsBotDefeatPanel.gameObject.activeSelf)
            HidePanel(vsBotDefeatPanel);
        if (acceptDrawPanel.gameObject.activeSelf)
            HidePanel(acceptDrawPanel);
        if (matchDrawPanel.gameObject.activeSelf)
            HidePanel(matchDrawPanel);
    }

}