using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;
using Chess.UI;

namespace Chess.Players
{
	public class HumanPlayer : Player
	{

		public enum InputState
		{
			None,
			PieceSelected,
			DraggingPiece
		}

        public bool CanTakeInput { get; set; }

		InputState currentState;

		BoardUI boardUI;
		Camera cam;
		Coord selectedPieceSquare;
		Board board;
		public HumanPlayer(Board board)
		{
            CanTakeInput = true;
            boardUI = GameObject.FindObjectOfType<BoardUI>();
			cam = Camera.main;
			this.board = board;
		}

		public override void NotifyTurnToMove()
		{

		}

        public override void OnGamePaused()
        {
            CanTakeInput = false;
        }

        public override void OnGameResumed()
        {
            CanTakeInput = true;
        }

        public override void Update()
        {
            if (Input.touchCount > 0 && CanTakeInput)
            {
                Touch touch = Input.GetTouch(0); // Get the first touch
                Ray ray = cam.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Vector3 touchPosWorld = hit.point;

                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            HandlePieceSelection(touchPosWorld);
                            break;
                        case TouchPhase.Moved:
                        case TouchPhase.Stationary:
                            if (currentState == InputState.DraggingPiece)
                            {
                                HandleDragMovement(touchPosWorld);
                            }
                            break;
                        case TouchPhase.Ended:
                            if (currentState == InputState.DraggingPiece)
                            {
                                HandlePiecePlacement(touchPosWorld);
                            }
                            break;
                    }
                }
                else
                {
                    CancelPieceSelection();
                }
            }
        }

        void HandlePieceSelection(Vector3 touchPos)
        {
            if (boardUI.TryGetCoordFromPosition(touchPos, out selectedPieceSquare))
            {
                int index = BoardHelper.IndexFromCoord(selectedPieceSquare);
                if (Piece.IsColour(board.Square[index], board.MoveColour))
                {
                    boardUI.HighlightLegalMoves(board, selectedPieceSquare);
                    boardUI.HighlightSquare(selectedPieceSquare);
                    currentState = InputState.DraggingPiece;
                }
            }
        }

        void HandleDragMovement(Vector3 touchPos)
        {
            // Update the UI to show dragging of the piece but don't place it yet
            boardUI.DragPiece(selectedPieceSquare, touchPos);
        }

        void HandlePiecePlacement(Vector3 touchPos)
        {
            Coord targetSquare;
            if (boardUI.TryGetCoordFromPosition(touchPos, out targetSquare))
            {
                TryMakeMove(selectedPieceSquare, targetSquare);
                currentState = InputState.None; // Reset the input state after placing the piece
            }
            else
            {
                CancelPieceSelection();
            }
        }

        void CancelPieceSelection()
        {
            if (currentState != InputState.None)
            {
                currentState = InputState.None;
                boardUI.ResetSquareColours();
                boardUI.HighlightLastMadeMoveSquares(board);
                boardUI.ResetPiecePosition(selectedPieceSquare);
            }
        }

        void TryMakeMove(Coord startSquare, Coord targetSquare)
		{
			int startIndex = BoardHelper.IndexFromCoord(startSquare);
			int targetIndex = BoardHelper.IndexFromCoord(targetSquare);
			bool moveIsLegal = false;
			Move chosenMove = new Move();

			MoveGenerator moveGenerator = new MoveGenerator();
			bool wantsKnightPromotion = UnityEngine.InputSystem.Keyboard.current[UnityEngine.InputSystem.Key.LeftAlt].isPressed;

			var legalMoves = moveGenerator.GenerateMoves(board);
			for (int i = 0; i < legalMoves.Length; i++)
			{
				var legalMove = legalMoves[i];

				if (legalMove.StartSquare == startIndex && legalMove.TargetSquare == targetIndex)
				{
					if (legalMove.IsPromotion)
					{
						if (legalMove.MoveFlag == Move.PromoteToQueenFlag && wantsKnightPromotion)
						{
							continue;
						}
						if (legalMove.MoveFlag != Move.PromoteToQueenFlag && !wantsKnightPromotion)
						{
							continue;
						}
					}
					moveIsLegal = true;
					chosenMove = legalMove;
					//	Debug.Log (legalMove.PromotionPieceType);
					break;
				}
			}

			if (moveIsLegal)
			{
				ChoseMove(chosenMove);
				currentState = InputState.None;
			}
			else
			{
				CancelPieceSelection();
			}
		}

	}
}