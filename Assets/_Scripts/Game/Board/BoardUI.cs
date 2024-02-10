using Chess.Core;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Chess.UI
{
	public class BoardUI : MonoBehaviour
	{

		public PieceTheme pieceTheme;
		public BoardTheme boardTheme;
		public BoardTile boardTile;
        public List<SpriteRenderer> whitePlayerCapturedPieces;
        public List<SpriteRenderer> blackPlayerCapturedPieces;
        public bool showLegalMoves;
		public bool whiteIsBottom = true;

		[Header("Line Rendereing")]
        public GameObject linePrefab;
		public float lineLength;
		public float lineOffsetLinear;
        public float lineOffsetDaigonal;

        BoardTile[,] boardTiles;
		SpriteRenderer[,] squarePieceRenderers;
		Move lastMadeMove;
		MoveGenerator moveGenerator;
        int whitePlayerCapturedPieceInxex;
        int blackPlayerCapturedPieceInxex;
        List<GameObject> lineInstances = new List<GameObject>();

        const float pieceDepth = -0.1f;
		const float pieceDragDepth = -0.2f;

		void Awake()
		{
			moveGenerator = new MoveGenerator();
			CreateBoardUI();
		}

        public void ResetCapturedPieces()
        {
            whitePlayerCapturedPieceInxex = 0;
            blackPlayerCapturedPieceInxex = 0;

            foreach (SpriteRenderer _piece in whitePlayerCapturedPieces)
                _piece.sprite = null;

            foreach (SpriteRenderer _piece in blackPlayerCapturedPieces)
                _piece.sprite = null;

        }

		public void OnPieceCaptured(int piece, bool whitePlayerMove)
		{
			Sprite pieceSp = pieceTheme.GetPieceSprite(piece);
            if (whitePlayerMove)
			{
				whitePlayerCapturedPieces[whitePlayerCapturedPieceInxex].sprite = pieceSp;
                whitePlayerCapturedPieceInxex += 1;
            }
			else
			{
                blackPlayerCapturedPieces[blackPlayerCapturedPieceInxex].sprite = pieceSp;
                blackPlayerCapturedPieceInxex += 1;
            }
		}

        public void UpdatePosition(Board board, Move move, bool animate = false)
		{
			lastMadeMove = move;
			if (animate)
			{
				StartCoroutine(AnimateMove(move, board));
			}
			else
			{
				UpdatePosition(board);
				ResetSquareColours();
				HighlightMoveSquares(move);
			}
		}

		public void UpdatePosition(Board board)
		{
			for (int rank = 0; rank < 8; rank++)
			{
				for (int file = 0; file < 8; file++)
				{
					Coord coord = new Coord(file, rank);
					int piece = board.Square[BoardHelper.IndexFromCoord(coord.fileIndex, coord.rankIndex)];
					squarePieceRenderers[file, rank].sprite = pieceTheme.GetPieceSprite(piece);
					squarePieceRenderers[file, rank].transform.position = PositionFromCoord(file, rank, pieceDepth);
				}
			}
		}


		public void HighlightLegalMoves(Board board, Coord fromSquare)
		{
			if (showLegalMoves)
			{
				Move[] moves = moveGenerator.GenerateMoves(board).ToArray();

				for (int i = 0; i < moves.Length; i++)
				{
					Move move = moves[i];

					if (move.StartSquare == BoardHelper.IndexFromCoord(fromSquare))
                    {
                        Coord coord = BoardHelper.CoordFromIndex(move.TargetSquare);
						boardTiles[coord.fileIndex, coord.rankIndex].Trajectory = true;

						DrawLine(boardTiles[fromSquare.fileIndex, fromSquare.rankIndex].transform.position, boardTiles[coord.fileIndex, coord.rankIndex].transform.position);
                    }
				}
			}
		}

		public void DragPiece(Coord pieceCoord, Vector2 mousePos)
		{
			squarePieceRenderers[pieceCoord.fileIndex, pieceCoord.rankIndex].transform.position = new Vector3(mousePos.x, mousePos.y, pieceDragDepth);
		}

		public void ResetPiecePosition(Coord pieceCoord)
		{
			Vector3 pos = PositionFromCoord(pieceCoord.fileIndex, pieceCoord.rankIndex, pieceDepth);
			squarePieceRenderers[pieceCoord.fileIndex, pieceCoord.rankIndex].transform.position = pos;
		}

		public void HighlightMoveSquares(Move move)
		{
            boardTiles[BoardHelper.CoordFromIndex(move.StartSquare).fileIndex, BoardHelper.CoordFromIndex(move.StartSquare).rankIndex].GetComponent<BoardTile>().MoveTrace = true;
            boardTiles[BoardHelper.CoordFromIndex(move.TargetSquare).fileIndex, BoardHelper.CoordFromIndex(move.TargetSquare).rankIndex].GetComponent<BoardTile>().MoveTrace = true;
        }

		public void HighlightLastMadeMoveSquares(Board board)
		{
			if (board.AllGameMoves.Count > 0)
			{
				HighlightMoveSquares(board.AllGameMoves[^1]);
			}
		}


		public void HighlightSquare(Coord coord)
		{
			boardTiles[coord.fileIndex, coord.rankIndex].GetComponent<BoardTile>().Highlight = true;
        }

		public bool TryGetCoordFromPosition(Vector2 worldPos, out Coord selectedCoord)
		{
			int file = (int)(worldPos.x + 4);
			int rank = (int)(worldPos.y + 4);
			if (!whiteIsBottom)
			{
				file = 7 - file;
				rank = 7 - rank;
			}
			selectedCoord = new Coord(file, rank);
			return file >= 0 && file < 8 && rank >= 0 && rank < 8;
		}


		IEnumerator AnimateMove(Move move, Board board)
		{
			float t = 0;
			const float moveAnimDuration = 0.15f;
			Coord startCoord = BoardHelper.CoordFromIndex(move.StartSquare);
			Coord targetCoord = BoardHelper.CoordFromIndex(move.TargetSquare);
			Transform pieceT = squarePieceRenderers[startCoord.fileIndex, startCoord.rankIndex].transform;
			Vector3 startPos = PositionFromCoord(startCoord);
			Vector3 targetPos = PositionFromCoord(targetCoord);
			Coord coord = BoardHelper.CoordFromIndex(move.StartSquare);
            boardTiles[coord.fileIndex, coord.rankIndex].Highlight = true;

            while (t <= 1)
			{
				yield return null;
				t += Time.deltaTime * 1 / moveAnimDuration;
				pieceT.position = Vector3.Lerp(startPos, targetPos, t);
			}
			UpdatePosition(board);
			ResetSquareColours();
			HighlightMoveSquares(move);
			pieceT.position = startPos;
		}


        void CreateBoardUI()
        {
            boardTiles = new BoardTile[8, 8];
            squarePieceRenderers = new SpriteRenderer[8, 8];

            for (int rank = 0; rank < 8; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
					BoardTile newTile = Instantiate(boardTile, transform);
                    newTile.transform.name = BoardHelper.SquareNameFromCoordinate(file, rank);
                    newTile.transform.position = PositionFromCoord(file, rank, 0);
                    squarePieceRenderers[file, rank] = newTile.Piece;
                    boardTiles[file, rank] = newTile;
                }
            }

            ResetSquareColours();
        }

        void ResetSquarePositions()
		{
			for (int rank = 0; rank < 8; rank++)
			{
				for (int file = 0; file < 8; file++)
				{
					boardTiles[file, rank].transform.position = PositionFromCoord(file, rank, 0);
					squarePieceRenderers[file, rank].transform.position = PositionFromCoord(file, rank, pieceDepth);
				}
			}

			if (!lastMadeMove.IsNull)
			{
				HighlightMoveSquares(lastMadeMove);
			}
		}

        public void ResetAllLines()
        {
            foreach (GameObject line in lineInstances)
            {
                Destroy(line);
            }
            lineInstances.Clear();
        }

        void DrawLine(Vector3 from, Vector3 to)
        {
            Vector3 direction = (to - from).normalized;
            Vector3 offset = CalculateOffset(direction);
            Vector3 adjustedFrom = from + offset;

            float distance = Vector3.Distance(adjustedFrom, to);
            int repeatCount = Mathf.FloorToInt(distance / lineLength);

            for (int i = 0; i < repeatCount; i++)
            {
                Vector3 position = adjustedFrom + direction * (lineLength * i);
                GameObject lineInstance = Instantiate(linePrefab.gameObject, position, Quaternion.identity, transform);
                lineInstance.transform.up = direction;
                lineInstances.Add(lineInstance);
            }
        }

        Vector3 CalculateOffset(Vector3 direction)
        {
            direction = direction.normalized;

			Vector3 offsetPos = Vector3.zero;
            float x = Mathf.Abs(direction.x);
            float y = Mathf.Abs(direction.y);

			if (x > 0 && y > 0)
				offsetPos = (new Vector3(direction.x, direction.y, 0).normalized) * lineOffsetDaigonal;

			else if (x > 0)
				offsetPos = (new Vector3(direction.x, 0, 0).normalized) * lineOffsetLinear;

			else if (y > 0)
                offsetPos = (new Vector3(0, direction.y, 0).normalized) *lineOffsetLinear;

            return offsetPos;
        }


        public void SetPerspective(bool whitePOV)
		{
			whiteIsBottom = whitePOV;
			ResetSquarePositions();

		}

		public void ResetSquareColours()
        {
			ResetAllLines();

            for (int rank = 0; rank < 8; rank++)
			{
				for (int file = 0; file < 8; file++)
				{
					Coord coord = new Coord(file, rank);
                    boardTiles[coord.fileIndex, coord.rankIndex].ResetTile();

                }
			}
		}

		public Vector3 PositionFromCoord(int file, int rank, float depth = 0)
		{
			return PositionFromCoord(file, rank, depth, whiteIsBottom);
		}

		public static Vector3 PositionFromCoord(int file, int rank, float depth = 0, bool whiteIsBottom = true)
		{
            float gridSpacing = 1f;
            float offsetX = -3.505f * gridSpacing;
            float offsetY = -3.5f * gridSpacing;
            if (whiteIsBottom)
            {
                return new Vector3(offsetX + file * gridSpacing, offsetY + rank * gridSpacing, depth);
            }
            return new Vector3(offsetX + (7 - file) * gridSpacing, offsetY + (7 - rank) * gridSpacing, depth);
        }

		public Vector3 PositionFromCoord(Coord coord, float depth = 0)
		{
			return PositionFromCoord(coord.fileIndex, coord.rankIndex, depth);
		}

	}
}