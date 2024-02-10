using UnityEngine;
using Chess.Core;
using Chess.UI;
using Chess.Players;
using UnityEngine.InputSystem;
using Utilities.Audio;
using UnityEngine.Events;
using System;
using TMPro;
using System.Collections.Generic;

namespace Chess.Game
{
	public class GameManager : MonoBehaviour
    {
        [Serializable] public class ResultEvent : UnityEvent<GameResult.Result> { }

        public event Action onPositionLoaded;
		public event Action<Move> onMoveMade;
        public UnityEvent OnMoveCompleted;
        public ResultEvent OnGameEnded;

		public enum PlayerType { Human, AI }

		[Header("References")]
		public ClockManager clockManager;
		public AISettings aiSettings;
		public GameSettings gameSettings;
        public TMP_Text player1Text;
        public TMP_Text player2Text;
		public TMP_Text moveText;

        private string currentFen;
        private ulong zobristDebug;
        private bool loadCustomPosition;
        private string customPosition = "1rbq1r1k/2pp2pp/p1n3p1/2b1p3/R3P3/1BP2N2/1P3PPP/1NBQ1RK1 w - - 0 1";

        // Internal stuff
		GameResult.Result gameResult;
		Player whitePlayer;
		Player blackPlayer;
		Player playerToMove;
		BoardUI boardUI;

        public Board board { get; private set; }
		public bool isPlayer1Turn { get { return playerToMove == whitePlayer; } }
		Board searchBoard;

		void Start()
		{
			Application.targetFrameRate = 120;

			boardUI = FindObjectOfType<BoardUI>();
			board = new Board();
			searchBoard = new Board();
			aiSettings.diagnostics = new Searcher.SearchDiagnostics();
            player1Text.text = gameSettings.Opponent == PlayerType.Human ? "Player 1" : "Player";
            player2Text.text = gameSettings.Opponent == PlayerType.Human ? "Player 2" : "Bot";
			moveText.text = player1Text.text + " Move";

            NewGame(PlayerType.Human, gameSettings.Opponent);

		}

		public void RestartGame()
		{
            NewGame(PlayerType.Human, gameSettings.Opponent);
        }

		public void GamePaused()
		{
			whitePlayer.OnGamePaused();
			blackPlayer.OnGamePaused();
        }

		public void GameResumed()
		{
            whitePlayer.OnGameResumed();
            blackPlayer.OnGameResumed();
        }

		void Update()
		{ 
			HandleInput();
			UpdateGame();
			UpdateDebugInfo();
		}

		void UpdateGame()
		{
			if (gameResult == GameResult.Result.Playing)
			{
				playerToMove.Update();
			}

		}

		void UpdateDebugInfo()
		{
			zobristDebug = board.currentGameState.zobristKey;
			ulong generatedKey = Zobrist.CalculateZobristKey(board);
			if (generatedKey != zobristDebug)
			{
				Debug.Log("Key Error: incremental: " + zobristDebug + "  gen: " + generatedKey);
			}

		}

		void HandleInput()
		{
			Keyboard keyboard = Keyboard.current;

			if (keyboard[Key.U].wasPressedThisFrame)
			{
				if (board.AllGameMoves.Count > 0)
				{
					Move moveToUndo = board.AllGameMoves[^1];
					board.UnmakeMove(moveToUndo);
					searchBoard.UnmakeMove(moveToUndo);
					boardUI.UpdatePosition(board);
					boardUI.ResetSquareColours();
					boardUI.HighlightLastMadeMoveSquares(board);

					PlayMoveSound(moveToUndo);

					gameResult = GameResult.GetGameState(board);
					NotifyGameResult(gameResult);
				}
			}

			if (keyboard[Key.N].wasPressedThisFrame)
			{
				Debug.Log("Make Null Move");
				board.MakeNullMove();
			}


		}

		void OnMoveChosen(Move move)
		{
			PlayMoveSound(move);

			bool animateMove = playerToMove is AIPlayer;
			board.MakeMove(move);
			searchBoard.MakeMove(move);

			currentFen = FenUtility.CurrentFen(board);
			onMoveMade?.Invoke(move);
			OnMoveCompleted?.Invoke();

            boardUI.UpdatePosition(board, move, animateMove);
            clockManager.ToggleClock();
            
            NotifyPlayerToMove();
		}

		public void NewGame(bool humanPlaysWhite)
		{
			boardUI.SetPerspective(humanPlaysWhite);
			NewGame((humanPlaysWhite) ? PlayerType.Human : PlayerType.AI, (humanPlaysWhite) ? PlayerType.AI : PlayerType.Human);
		}

		public void NewComputerVersusComputerGame()
		{
			boardUI.SetPerspective(true);
			NewGame(PlayerType.AI, PlayerType.AI);
		}

		void NewGame(PlayerType whitePlayerType, PlayerType blackPlayerType)
		{
			if (loadCustomPosition)
			{
				currentFen = customPosition;
				board.LoadPosition(customPosition);
				searchBoard.LoadPosition(customPosition);
			}
			else
			{
				currentFen = FenUtility.StartPositionFEN;
				board.LoadStartPosition();
				searchBoard.LoadStartPosition();
			}
            onPositionLoaded?.Invoke();
			boardUI.UpdatePosition(board);
			boardUI.ResetSquareColours();
            boardUI.ResetCapturedPieces();

            CreatePlayer(ref whitePlayer, whitePlayerType);
			CreatePlayer(ref blackPlayer, blackPlayerType);

            clockManager.StartClocks(board.IsWhiteToMove, gameSettings.Duration, gameSettings.DurationIncrement);
            clockManager.ClockTimeout -= OnTimeout;
            clockManager.ClockTimeout += OnTimeout;


            gameResult = GameResult.Result.Playing;

			NotifyPlayerToMove();

		}


		public void QuitGame()
		{
			Application.Quit();
		}

		void NotifyPlayerToMove()
		{
			gameResult = GameResult.GetGameState(board);

			if (gameResult == GameResult.Result.Playing)
			{
				playerToMove = (board.IsWhiteToMove) ? whitePlayer : blackPlayer;
                moveText.text = (playerToMove == whitePlayer ? player1Text.text : player2Text.text) + " Move";

                playerToMove.NotifyTurnToMove();

			}
			else
			{
				GameOver();
			}
		}

		void GameOver()
		{
			Debug.Log("Game Over " + gameResult);
			clockManager.StopClocks();
            NotifyGameResult(gameResult);
        }

		void NotifyGameResult(GameResult.Result result)
		{
			if (result != GameResult.Result.Playing)
			{
                OnGameEnded?.Invoke(result);
            }
		}

		void PlayMoveSound(Move moveToBePlayed)
		{
            bool isCapture = false;
			if (board.Square[moveToBePlayed.TargetSquare] != Piece.None)
			{
				isCapture = true;
                boardUI.OnPieceCaptured(board.Square[moveToBePlayed.TargetSquare], playerToMove == whitePlayer);
            }

            AudioController.Instance.PlayAudio(isCapture ? AudioName.PIECE_CAPTURE: AudioName.PIECE_MOVE);
        }

		void OnTimeout(bool whiteTimedOut)
		{
			gameResult = whiteTimedOut ? GameResult.Result.WhiteTimeout : GameResult.Result.BlackTimeout;
			GameOver();
		}

		void CreatePlayer(ref Player player, PlayerType playerType)
		{
			if (player != null)
			{
				player.onMoveChosen -= OnMoveChosen;
			}

			if (playerType == PlayerType.Human)
			{
				player = new HumanPlayer(board);
			}
			else
			{
				player = new AIPlayer(searchBoard, aiSettings, gameSettings.Mode);
			}
			player.onMoveChosen += OnMoveChosen;
		}
	}
}