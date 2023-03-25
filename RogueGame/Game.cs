using RogueGame;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
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

        // Random number generator
        private static Random rand = new Random();

        public string StatusMessage
        {
            get { return cStatus; }
        }

        public string StatsDisplay
        {
            get { return $"Level: {CurrentLevel}   Gold: {CurrentPlayer.Gold} "; }
        }


        public Game(string PlayerName) {

            // Setup a new game with a map and a player.
            // Put the player on the map and set the opening status.

            this.CurrentLevel = 1;
            this.CurrentMap = new MapLevel();
            this.CurrentPlayer = new Player(PlayerName);
            this.CurrentPlayer.Location = CurrentMap.PlaceMapCharacterLINQ(Player.CHARACTER, true);
            this.CurrentTurn = 0;
            cStatus = $"Welcome to the Dungeon, {CurrentPlayer.PlayerName} ...";         
        }

        public void KeyHandler(int KeyVal, bool Shift)
        {
            // Process whatever key is sent by the form.

            // Basics
            switch (KeyVal)
            {
                case KEY_WEST:
                    MoveCharacter(CurrentPlayer, MapLevel.Direction.West);
                    break;
                case KEY_NORTH:
                    MoveCharacter(CurrentPlayer, MapLevel.Direction.North);
                    break;
                case KEY_EAST:
                    MoveCharacter(CurrentPlayer, MapLevel.Direction.East);
                    break;
                case KEY_SOUTH:
                    MoveCharacter(CurrentPlayer, MapLevel.Direction.South);
                    break;
            }

            // Shift combinations
            if (Shift)
            {


            }
            else
            {


            }

        }

        public void MoveCharacter(Player player, MapLevel.Direction direct)
        {
            // Move character if possible.  This method is in development.

            // List of characters a living character can move onto.
            List<char> charsAllowed = new List<char>(){MapLevel.ROOM_INT, MapLevel.STAIRWAY,
                MapLevel.ROOM_DOOR, MapLevel.HALLWAY};

            // Set surrounding characters
            Dictionary<MapLevel.Direction, MapSpace> surrounding =
                CurrentMap.SearchAdjacent(player.Location.X, player.Location.Y);

            // If the map character in the chosen direction is habitable and if there's no monster there,
            // move the character there.
            if (charsAllowed.Contains(surrounding[direct].MapCharacter) && 
                surrounding[direct].DisplayCharacter == null)
                    player.Location = CurrentMap.MoveDisplayItem(player.Location, surrounding[direct]);

            cStatus = "";

            if (player.Location.ItemCharacter == MapLevel.GOLD)
                PickUpGold(); 
            
        }

        private void PickUpGold()
        {
            int goldAmt = rand.Next(MapLevel.MAX_GOLD_AMT);
            CurrentPlayer.Gold += goldAmt;
            CurrentPlayer.Location.ItemCharacter = null;
            cStatus = $"You picked up {goldAmt} pieces of gold.";

        }
    }
}
