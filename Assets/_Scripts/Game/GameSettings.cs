using UnityEngine;
using static Chess.Game.GameManager;

namespace Chess.Game
{
    
	[CreateAssetMenu(menuName = "Game Settings/Settings")]
    public class GameSettings : ScriptableObject
    {

        public int Duration;
        public int DurationIncrement;
        public PlayerType Opponent;
        public GameMode Mode;

    }

}