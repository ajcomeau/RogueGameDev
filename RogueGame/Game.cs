using RogueGame;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
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
        private const int KEY_S = 83;
        private const int KEY_D = 68;
        private const int KEY_N = 78;
        private const int KEY_E = 69;
        private const int KEY_I = 73;
        private const int KEY_ESC = 27;
        private const int SEARCH_PCT = 20;

        public MapLevel CurrentMap { get; set; }
        public int CurrentLevel { get; set; }
        public Player CurrentPlayer { get; }
        public int CurrentTurn { get; set; }
        public string ScreenDisplay { get; set; }
        public bool DevMode { get; set; }
        public bool InvDisplay { get; set; }

        private string cStatus;

        // Random number generator
        private static Random rand = new Random();

        public string StatusMessage
        {
            get { return cStatus; }
        }

        public string StatsDisplay
        {
            get { return $"Level: {CurrentLevel}   Gold: {CurrentPlayer.Gold}   " +
                    $"Turn: {CurrentTurn} "; }
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

            this.CurrentTurn = 1;
            cStatus = $"Welcome to the Dungeon, {CurrentPlayer.PlayerName} ...";

            this.ScreenDisplay = this.CurrentMap.MapText();
        }

        public void KeyHandler(int KeyVal, bool Shift, bool Control)
        {
            // Process whatever key is sent by the form.
            bool startTurn = false;

            // Basics
            switch (KeyVal)
            {
                case KEY_WEST:
                    startTurn = MoveCharacter(CurrentPlayer, MapLevel.Direction.West);
                    break;
                case KEY_NORTH:
                    startTurn = MoveCharacter(CurrentPlayer, MapLevel.Direction.North);
                    break;
                case KEY_EAST:
                    startTurn = MoveCharacter(CurrentPlayer, MapLevel.Direction.East);
                    break;
                case KEY_SOUTH:
                    startTurn = MoveCharacter(CurrentPlayer, MapLevel.Direction.South);
                    break;
            }

            // Shift combinations
            if (Shift)
            {
                switch (KeyVal)
                {
                case KEY_DOWNLEVEL:
                    startTurn = true;
                    if (CurrentPlayer.Location!.MapCharacter == MapLevel.STAIRWAY)
                        ChangeLevel(1);
                    else
                        cStatus = "There's no stairway here.";
                    break;
                    case KEY_UPLEVEL:
                        startTurn = true;
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
                switch (KeyVal)
                {
                    case KEY_S:
                        startTurn = true;
                        SearchForHidden();
                        break;
                    case KEY_E:
                        startTurn = true;
                        
                        break;
                    case KEY_I:
                            
                        break;
                    default:
                        break;
                }

            }

            if (Control)
            {
                switch (KeyVal)
                {
                    case KEY_D:
                        DevMode = !DevMode;
                            cStatus = DevMode ? "Developer Mode ON" : "Developer Mode OFF";
                        break;
                    case KEY_N:
                        if (DevMode)
                            ReplaceMap();
                        break;
                    default:
                        break;
                }
            }


            if (startTurn)
            {
                // Perform whatever actions needed to complete turn
                // (i.e. monster moves)


                // Increment current turn number
                CurrentTurn++;
            }


            ScreenDisplay = CurrentMap.MapText();

        }

        private void SearchForHidden()
        {
            List<MapSpace> spaces;

            if (rand.Next(1, 101) <= SEARCH_PCT)
            {
                cStatus = "Searching ...";
                spaces = CurrentMap.GetSurrounding(CurrentPlayer.Location!.X, CurrentPlayer.Location.Y);

                foreach (MapSpace space in spaces)
                {
                    if (space.SearchRequired)
                    {
                        space.SearchRequired = false;
                        space.AltMapCharacter = null;
                    }
                }

            }
            else
                cStatus = "";
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

        private void ReplaceMap()
        {
            // Dev mode only - replace the map for testing.
            CurrentMap = new MapLevel();
            CurrentPlayer.Location = CurrentMap.PlaceMapCharacter(Player.CHARACTER, true);
        }

        public bool MoveCharacter(Player player, MapLevel.Direction direct)
        {
            char visibleCharacter;
            bool retValue = false;

            // Move character if possible.  This method is in development.
            // Clear the status.
            cStatus = "";

            // List of characters a living character can move onto.
            List<char> charsAllowed = new List<char>(){MapLevel.ROOM_INT, MapLevel.STAIRWAY,
                MapLevel.ROOM_DOOR, MapLevel.HALLWAY};

            // Set surrounding characters
            Dictionary<MapLevel.Direction, MapSpace> adjacent =
                CurrentMap.SearchAdjacent(player.Location!.X, player.Location.Y);

            visibleCharacter = adjacent[direct].SearchRequired ? (char)adjacent[direct].AltMapCharacter! : adjacent[direct].MapCharacter;

            // If the map character in the chosen direction is habitable and if there's no monster there,
            // move the character there.
            if (charsAllowed.Contains(visibleCharacter) && 
                adjacent[direct].DisplayCharacter == null)
            { 
                    player.Location = CurrentMap.MoveDisplayItem(player.Location, adjacent[direct]);
                    retValue = true;
            }

            if(retValue)
            {
                // If the character has moved.

                // If this is a doorway, determine if the room is lighted.
                if (player.Location.MapCharacter == MapLevel.ROOM_DOOR)
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

            return retValue;

        }

        private void PickUpGold()
        {
            // Add the gold at the current location to the player's purse and remove
            // it from the map.
            int goldAmt = rand.Next(MapLevel.MIN_GOLD_AMT, MapLevel.MAX_GOLD_AMT + 1);
            CurrentPlayer.Gold += goldAmt;
            CurrentPlayer.Location!.ItemCharacter = null;
            cStatus = $"You picked up {goldAmt} pieces of gold.";

        }

        private string AddInventory()
        {
            // Inventory management.

            Inventory foundItem;

            string retValue = "";

            // If the player found the Amulet ...
            if(CurrentPlayer.Location!.ItemCharacter == MapLevel.AMULET)
            {
                CurrentPlayer.HasAmulet = true;
                CurrentPlayer.Location!.ItemCharacter = null;

                // Add it to the inventory.
                if(CurrentPlayer.Location.MapInventory != null)
                {
                    CurrentPlayer.PlayerInventory.Add(CurrentPlayer.Location.MapInventory);
                    CurrentPlayer.Location.MapInventory = null;
                }
                
                retValue = "You found the Amulet of Yendor!  It has been added to your inventory.";
            }
            else if (CurrentPlayer.Location!.ItemCharacter != null)
            {
                // For everything else, pick it up if it can fit in inventory.
                if(CurrentPlayer.PlayerInventory.Count < Player.INVENTORY_LIMIT)
                {
                    CurrentPlayer.Location!.ItemCharacter = null;                    
                    
                    if (CurrentPlayer.Location.MapInventory != null)
                    {
                        foundItem = CurrentPlayer.Location.MapInventory;

                        retValue = $"You picked up {foundItem.CodeName}.";                        
                        
                        // Move the Inventory reference to the player's inventory.
                        CurrentPlayer.PlayerInventory.Add(foundItem);
                        CurrentPlayer.Location.MapInventory = null;
                    } 
                }
                else
                {
                    // Notify the player if inventory is full.
                    retValue = "You cannot pick it up. Your inventory is full.";
                }
            }

            return retValue;
        }

        private string InventoryDisplay()
        {
            string retValue = "";
            List<InventoryLine> lines = new List<InventoryLine>();

            // Get groupable identified inventory.
            var groupedInventory =
                (from invEntry in CurrentPlayer.PlayerInventory
                 where invEntry.IsGroupable && invEntry.IsIdentified
                 group invEntry by invEntry.RealName into itemGroup
                 select itemGroup).ToList();

            // Add groupable non-identified inventory.
            groupedInventory.Concat(
                from invEntry in CurrentPlayer.PlayerInventory
                where invEntry.IsGroupable && !invEntry.IsIdentified
                group invEntry by invEntry.CodeName into itemGroup
                select itemGroup).ToList();

            // Get non-groupable identified
            var individualItems =
                (from invEntry in CurrentPlayer.PlayerInventory
                 where !invEntry.IsGroupable
                 select invEntry).ToList();

            foreach( var itemGroup in groupedInventory)
            {
                lines.Add(new InventoryLine { Count = itemGroup.Count(), ItemType = itemGroup.First().ItemType, DisplayName = itemGroup.Key });
            }

            lines = lines.OrderBy(x => x.ItemType).ToList();

            foreach( InventoryLine line in lines )
            {
                retValue += $"{line.Count} {line.DisplayName}\n";
            }


            return retValue;
        }

        internal class InventoryLine
        {
            public int Count;
            public Inventory.InventoryType ItemType;
            public string DisplayName;
        }

    }
}
