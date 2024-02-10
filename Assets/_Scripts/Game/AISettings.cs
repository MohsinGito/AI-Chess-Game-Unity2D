using UnityEngine;

namespace Chess.Game
{
	using UnityEngine;
    using Chess.Core;
    using System.Collections.Generic;

    [CreateAssetMenu(menuName = "AI/Settings")]
    public class AISettings : ScriptableObject
	{

		public event System.Action requestCancelSearch;

		[Header("Search")]
		public SearchSettings.SearchMode mode;
		public int searchTimeMillis = 1000;
		public int fixedSearchDepth;
		public bool runOnMainThread;

		[Header("Modes Data")]
		public bool useModeData;
		public List<ModeData> modesData;
		public int maxBookPly = 16;
		public int bookMoveDelayMs = 200;
		public int bookMoveDelayRandomExtraMs = 0;

		[Header("Diagnostics")]
		public Searcher.SearchDiagnostics diagnostics;

		public void RequestCancelSearch()
		{
			requestCancelSearch?.Invoke();
		}
	}
}

[System.Serializable]
public struct ModeData
{
	public GameMode Mode;
	public TextAsset Data;
}