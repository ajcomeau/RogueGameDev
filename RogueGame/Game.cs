﻿using RogueGame;
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
        private const int KEY_UPLEVEL = 188;
        private const int KEY_DOWNLEVEL = 190;
        private const int MAX_LEVEL = 26;

        public MapLevel CurrentMap { get; set; }
        public int CurrentLevel { get; set; }
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

            // Generate the new map and shroud it.
            this.CurrentMap = new MapLevel();
            this.CurrentMap.ShroudMap();
            
            // Put new player on map.
            this.CurrentPlayer = new Player(PlayerName);
            this.CurrentPlayer.Location = CurrentMap.PlaceMapCharacterLINQ(Player.CHARACTER, true);
            
            // Activate the player's current room.
            this.CurrentMap.DiscoverRoom(CurrentPlayer.Location.X, CurrentPlayer.Location.Y);

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
                switch (KeyVal)
                {
                    case KEY_DOWNLEVEL:
                        if (CurrentPlayer.Location!.MapCharacter == MapLevel.STAIRWAY)
                            ChangeLevel(1);
                        else
                            cStatus = "There's no stairway here.";
                        break;
                    case KEY_UPLEVEL:
                        if (CurrentPlayer.Location!.MapCharacter == MapLevel.STAIRWAY)
                            ChangeLevel(-1);
                        else
                            cStatus = "There's no stairway here.";
                        break;
                    default:
                        break;
                }
            }
            else
            {


            }
        }

        private void ChangeLevel(int Change)
        {
            bool allowPass = false;

            if (Change < 0)
            {
                allowPass = CurrentPlayer.HasAmulet && CurrentLevel > 1;
                cStatus = allowPass ? "" : "You cannot go that way.";
            }
            else if (Change > 0) 
            {
                allowPass = CurrentLevel < MAX_LEVEL;
                cStatus = allowPass ? "" : "You have reached the bottom level. You must go the other way.";            
            }

            if (allowPass)
            {
                CurrentMap = new MapLevel();
                CurrentMap.ShroudMap();
                CurrentLevel += Change;
                CurrentPlayer.Location = CurrentMap.PlaceMapCharacter(Player.CHARACTER, true);
                CurrentMap.DiscoverRoom(CurrentPlayer.Location.X, CurrentPlayer.Location.Y);
                
                if (CurrentLevel == MAX_LEVEL && !CurrentPlayer.HasAmulet)
                    CurrentMap.PlaceMapCharacter(MapLevel.AMULET, false);
                
                cStatus = "";
            }

        }

        public void MoveCharacter(Player player, MapLevel.Direction direct)
        {

            // Move character if possible.  This method is in development.
            // Clear the status.
            cStatus = "";

            // List of characters a living character can move onto.
            List<char> charsAllowed = new List<char>(){MapLevel.ROOM_INT, MapLevel.STAIRWAY,
                MapLevel.ROOM_DOOR, MapLevel.HALLWAY};

            // Set surrounding characters
            Dictionary<MapLevel.Direction, MapSpace> adjacent =
                CurrentMap.SearchAdjacent(player.Location!.X, player.Location.Y);

            // If the map character in the chosen direction is habitable and if there's no monster there,
            // move the character there.
            if (charsAllowed.Contains(adjacent[direct].MapCharacter) && 
                adjacent[direct].DisplayCharacter == null)
                    player.Location = CurrentMap.MoveDisplayItem(player.Location, adjacent[direct]);

            // If this is a doorway, determine if the room is lighted.
            if(player.Location.MapCharacter == MapLevel.ROOM_DOOR)
                CurrentMap.DiscoverRoom(player.Location.X, player.Location.Y);

            // Discover the spaces surrounding the player.
            CurrentMap.DiscoverSurrounding(player.Location.X, player.Location.Y);

            // Respond to items on map.
            if (player.Location.ItemCharacter != null)
            {
                if (player.Location.ItemCharacter == MapLevel.GOLD)
                    PickUpGold();
                else if (player.Location.ItemCharacter != null)
                    cStatus = AddInventory();
            }
        }

        private void PickUpGold()
        {
            // Add the gold at the current location to the player's purse and remove
            // it from the map.
            int goldAmt = rand.Next(MapLevel.MIN_GOLD_AMT, MapLevel.MAX_GOLD_AMT);
            CurrentPlayer.Gold += goldAmt;
            CurrentPlayer.Location!.ItemCharacter = null;
            cStatus = $"You picked up {goldAmt} pieces of gold.";

        }

        private string AddInventory()
        {
            // Inventory management. Currently just handling the Amulet.

            string retValue = "";

            if(CurrentPlayer.Location!.ItemCharacter == MapLevel.AMULET)
            {
                CurrentPlayer.HasAmulet = true;
                CurrentPlayer.Location!.ItemCharacter = null;
                retValue = "You found the Amulet of Yendor!  It has been added to your inventory.";
            }

            return retValue;
        }
    }
}
