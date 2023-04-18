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
        // Movement keys
        private const int KEY_WEST = 37;
        private const int KEY_NORTH = 38;
        private const int KEY_EAST = 39;
        private const int KEY_SOUTH = 40;
        // Stairway keys
        private const int KEY_UPLEVEL = 188;
        private const int KEY_DOWNLEVEL = 190;
        // Command keys
        private const int KEY_S = 83;
        private const int KEY_D = 68;
        private const int KEY_N = 78;
        private const int KEY_E = 69;
        private const int KEY_I = 73;
        private const int KEY_ESC = 27;
        
        private const int SEARCH_PCT = 20;  // Probability of search revealing hidden doors, etc..
        private const int MAX_LEVEL = 26;   // Maximum dungeon level

        public MapLevel CurrentMap { get; set; }                    
        public int CurrentLevel { get; set; }
        public Player CurrentPlayer { get; }
        public int CurrentTurn { get; set; }
        public string ScreenDisplay { get; set; }  // Current contents of the screen.
        public bool DevMode { get; set; }   // Dev mode shows entire map and allows map to be changed out.
        public bool InvDisplay { get; set; }  // Activated when inventory is being displayed.
        public Func<char?, bool>? ReturnFunction { get; set; }  // Function to be run after inventory selection.

        // Status message for top of screen.
        private string cStatus;  

        // Random number generator
        private static Random rand = new Random();

        public string StatusMessage
        {
            get { return cStatus; }
        }

        // Stats readout for bottom of screen.
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

            // Set starting turn and show welcome message.
            this.CurrentTurn = 1;
            cStatus = $"Welcome to the Dungeon, {CurrentPlayer.PlayerName} ...";

            // Set the current screen display.
            this.ScreenDisplay = DevMode ? this.CurrentMap.MapCheck() : this.CurrentMap.MapText(CurrentPlayer.Location);
        }

        public void KeyHandler(int KeyVal, bool Shift, bool Control)
        {
            // Process whatever key is sent by the form.
            // Putting a break point in this function to test causes it to lose keystrokes
            // following CTRL and SHIFT so they're not being sent here on their own anymore.

            bool startTurn = false, keyHandled = false;
            char lowerCase = char.ToLower((char)KeyVal);

            if (InvDisplay)
            {
                // For letters, call the current return function.
                if (lowerCase >= 'a' && lowerCase <= 'z')
                {
                    if (ReturnFunction != null)
                        ReturnFunction(lowerCase);
                }
                else if (KeyVal == KEY_ESC)
                {
                    // For ESC, clear the return function and restore the game map.
                    ReturnFunction = null;
                    RestoreMap();
                    cStatus = "";
                }
                keyHandled = true;
            }

            // Shift combinations
            if (Shift & !keyHandled)
            {
                switch (KeyVal)
                {
                    case KEY_DOWNLEVEL:     // Going downstairs.
                        startTurn = true;
                        if (CurrentPlayer.Location!.MapCharacter == MapLevel.STAIRWAY)
                            ChangeLevel(1);
                        else
                            cStatus = "There's no stairway here.";
                        break;
                    case KEY_UPLEVEL:       // Going upstairs.
                        startTurn = true;
                        if (CurrentPlayer.Location!.MapCharacter == MapLevel.STAIRWAY)
                            ChangeLevel(-1);
                        else
                            cStatus = "There's no stairway here.";
                        break;
                    default:
                        break;
                }
                keyHandled = true;
            }

            if (Control & !keyHandled)
            {
                switch (KeyVal)
                {
                    case KEY_D:         // Dev mode ON / OFF
                        DevMode = !DevMode;
                        cStatus = DevMode ? "Developer Mode ON" : "Developer Mode OFF";
                        break;
                    case KEY_N:         // New map
                        if (DevMode)
                            ReplaceMap();
                        break;
                    default:
                        break;
                }
                keyHandled = true;
            }

            // Basics 
            if (!keyHandled)
            {
                switch (KeyVal)
                {
                    // Movement keys
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
                    case KEY_S:     // Search
                        startTurn = true;
                        SearchForHidden();
                        break;
                    case KEY_E:     // Eat
                        startTurn = true;

                        break;
                    case KEY_I:     // Show inventory
                        DisplayInventory();
                        cStatus = "Here are the current contents of your inventory. Press ESC to exit.";
                        break;
                    case KEY_ESC:  // Restore map
                        RestoreMap();
                        cStatus = "";
                        break;
                    case KEY_D:     // Drop item
                        DropInventory(null);
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

            // If the inventory display hasn't been activated, display the appropriate map mode.
            if (!InvDisplay)
                ScreenDisplay = DevMode ? this.CurrentMap.MapCheck() : this.CurrentMap.MapText(CurrentPlayer.Location);

        }

        private void DisplayInventory()
        {
            // Switch the screen to the player's inventory.

            InvDisplay = true;
            ScreenDisplay = "\n\n";

            foreach(InventoryLine line in Inventory.InventoryDisplay(CurrentPlayer.PlayerInventory))
                ScreenDisplay += line.Description + "\n";
        }

        private void RestoreMap()
        {
            // Restore the map display.
            if (InvDisplay)
            {
                InvDisplay = false;
                ScreenDisplay = DevMode ? this.CurrentMap.MapCheck() : this.CurrentMap.MapText(CurrentPlayer.Location);
            }
        }

        private void SearchForHidden()
        {
            // Search for hiden items and reveal them if found.
            // TODO: This could be made dependent on player stats.
            List<MapSpace> spaces;

            // Search if we roll within probability constant.
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

        private bool DropInventory(char? ListItem)
        {
            bool retValue = false;
            List<Inventory> items;

            if (!InvDisplay)
            {
                DisplayInventory();
                cStatus = "Please select an item to drop.";
                ReturnFunction = DropInventory;
            }
            else
            {
                items = (from InventoryLine in Inventory.InventoryDisplay(CurrentPlayer.PlayerInventory)
                    where InventoryLine.ID == ListItem
                    select InventoryLine.InvItem).ToList();
                
                if (items.Count > 0)
                {
                    if(CurrentPlayer.Location!.MapInventory == null)
                    {
                        CurrentPlayer.Location.MapInventory = items[0];
                        CurrentPlayer.Location.ItemCharacter = 
                            CurrentPlayer.Location.MapInventory.DisplayCharacter;
                        CurrentPlayer.PlayerInventory.Remove(items[0]);
                        RestoreMap();
                        retValue = true;
                        cStatus = $"The item has been removed from inventory.";
                    }
                    else
                    {
                        cStatus = "There is already an item there.";
                        retValue = false;
                    }
                }
                else
                {
                    cStatus = "Please select an inventory item to drop.";
                    RestoreMap();
                    retValue = false;
                }

            }

            if (!InvDisplay)
                this.ReturnFunction = null;

            return retValue;
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
                    retValue = "You can't pick that up; your inventory is full.";
                }
            }

            return retValue;
        }

    }
}
