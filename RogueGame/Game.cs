using RogueGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueGame
{    
    internal class Game
    {
        private const int KEY_WEST = 37;
        private const int KEY_NORTH = 38;
        private const int KEY_EAST = 39;
        private const int KEY_SOUTH = 40;

        public MapLevel CurrentMap { get; set; }
        public int CurrentLevel { get; }
        public Player CurrentPlayer { get; }
        public int CurrentTurn { get; }
        
        private string cStatus;

        public string StatusMessage
        {
            get { return cStatus; }
        }


        public Game(string PlayerName) {
            // Setup a new game
            this.CurrentLevel = 0;
            this.CurrentMap = new MapLevel();
            this.CurrentPlayer = new Player(PlayerName);
            this.CurrentTurn = 0;
            cStatus = $"Welcome to the Dungeon, {CurrentPlayer.PlayerName} ...";            
        }

        public void KeyHandler(int KeyVal)
        {
            switch (KeyVal)
            {
                case KEY_WEST:
                    cStatus = "You moved west.";
                    break;
                case KEY_NORTH:
                    cStatus = "You moved north.";
                    break;
                case KEY_EAST:
                    cStatus = "You moved east.";
                    break;
                case KEY_SOUTH:
                    cStatus = "You moved south.";
                    break;
            }
        }
    }
}
