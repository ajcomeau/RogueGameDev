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
        private const int KEY_HELP = 191;
        
        private const int SEARCH_PCT = 20;  // Probability of search revealing hidden doors, etc..
        private const int MAX_LEVEL = 26;   // Maximum dungeon level
        private const int FAINT_PCT = 33;   // Probability of fainting at any given point when FAINT
        private const int MAX_TURN_LOSS = 5;  // Maximum turns to lose when fainting, etc..

        public enum DisplayMode {
            //DevMode = 0,
            Titles = 1,
            Primary = 2,
            Inventory = 3,
            Help = 4,
            GameOver = 5,
            Scoreboard = 6,
            Victory = 7,        
        }


        public MapLevel CurrentMap { get; set; }                    
        public int CurrentLevel { get; set; }
        public Player CurrentPlayer { get; }
        public int CurrentTurn { get; set; }
        public string ScreenDisplay { get; set; }  // Current contents of the screen.
        public DisplayMode GameMode { get; set; }
        public bool DevMode { get; set; }
        public bool FastPlay { get; set; }
        public Func<char?, bool>? ReturnFunction { get; set; }  // Function to be run after inventory selection.

        // Status message for top of screen.
        private string cStatus;  

        // Random number generator
        public static Random rand = new Random();

        public string StatusMessage
        {
            get { return cStatus; }
        }

        public string StatsDisplay()
        {
            string retValue = "";

            if (GameMode == DisplayMode.Primary)
            {
                // Assemble stats display for the bottom of the screen.
                retValue = $"Level: {CurrentLevel}    ";
                retValue += $"HP: {CurrentPlayer.MaxHP}/{CurrentPlayer.CurrentHP}    ";
                retValue += $"Strength: {CurrentPlayer.MaxStrength}/{CurrentPlayer.CurrentStrength}    ";
                retValue += $"Gold: {CurrentPlayer.Gold}    ";
                retValue += $"Armor: {(CurrentPlayer.Armor != null ? CurrentPlayer.Armor.ArmorClass : 0)}    ";
                retValue += $"Turn: {CurrentTurn}    ";
                retValue += $"Exp: {CurrentPlayer.ExperienceLevel()}/{CurrentPlayer.Experience}";


                if (CurrentPlayer.HungerState < Player.HungerLevel.Satisfied)
                    retValue += $"{CurrentPlayer.HungerState}     ";
            }
            else
                retValue = "";

            return retValue;
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
            this.CurrentPlayer.Location = CurrentMap.AddCharacterToMap(Player.CHARACTER);
            
            // Activate the player's current room.
            this.CurrentMap.DiscoverRoom(CurrentPlayer.Location.X, CurrentPlayer.Location.Y);

            // Set starting turn and show welcome message.
            this.CurrentTurn = 1;
            this.GameMode = DisplayMode.Primary;
            cStatus = $"Welcome to the Dungeon, {CurrentPlayer.PlayerName} ... (Press ? for list of commands.)";

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

            if (GameMode == DisplayMode.Inventory)
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
                    case KEY_HELP:  // Show help screen
                        GameMode = DisplayMode.Help;
                        ScreenDisplay = HelpScreen();
                        cStatus = "Here is a list of commands you can use.";
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
                    case KEY_S:     // Search
                        startTurn = true;
                        SearchForHidden();
                        break;
                    case KEY_E:     // Eat
                        startTurn = true;
                        Eat(null);
                        break;
                    case KEY_I:     // Show inventory
                        DisplayInventory();
                        cStatus = "Here is your current inventory. Press ESC to exit.";
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

            if (startTurn) CompleteTurn();

            // Display the appropriate map mode.
            if (GameMode == DisplayMode.Primary)
                ScreenDisplay = DevMode ? this.CurrentMap.MapCheck() : this.CurrentMap.MapText(CurrentPlayer.Location);

            switch (GameMode)
            {
                case DisplayMode.GameOver:
                    ScreenDisplay = RIPScreen();
                    break;
                default:
                    break;
            }

        }

        private void CompleteTurn()
        {
            do
            {
                // Perform whatever actions needed to complete turn
                // (i.e. monster moves)


                // Then, evaluate the player's current condition.
                EvaluatePlayer();
                // Increment current turn number
                CurrentTurn++;


                if (CurrentPlayer.Immobile > 0)
                {
                    CurrentPlayer.Immobile = CurrentPlayer.Immobile <= CurrentTurn ? 0 : CurrentPlayer.Immobile;
                    if (CurrentPlayer.Immobile == 0) cStatus = cStatus + " You can move again.";
                }
            } while (CurrentPlayer.Immobile > CurrentTurn);
        }

        private string CenterString(string Text, int Spaces)
        {
            // Center the string provided within the specified
            // number of spaces.

            string retValue = "";
            
            // If the string is longer than the number, just pass it back.
            if (Text.Length >= Spaces)
                retValue = Text;
            else
            // Otherwise, use PadLeft / PadRight
            {
                retValue = Text.PadLeft(Spaces / 2 + Text.Length / 2).PadRight(Spaces);
            }

            // If it's still short, keep adding a space.
            while (retValue.Length < Spaces)
                retValue = retValue.PadLeft(1);

            return retValue;
        }

        private string HelpScreen()
        {            
            return "\n\nArrows - movement\n\n" +
                "d - drop inventory\n\n" +
                "e - eat\n\n" +
                "s - search for hidden doorways\n\n" +
                "i - show inventory\n\n" +
                "> - go down a staircase\n\n" +
                "< - go up a staircase(requires Amulet from level 26\n\n" +
                "ESC - return to map.";
        }
        
        private string RIPScreen()
        {
            string endingCause = "";
            string screen;

            // If the player died from starvation, put that in the variable
            if (CurrentPlayer.HungerState == Player.HungerLevel.Dead)
                endingCause = "starvation";

            // Assemble the ASCII graphic and return it.
            screen = "\n\n\n\n\n\n\n\n" +
            "\n                   ╔═════════════════════════════╗" +
            "\n                   ║                             ║" +
            "\n                   ║                             ║" +
            "\n                   ║                             ║" +
            "\n                   ║        REST IN PEACE        ║" +
            "\n                   ║                             ║" +
            $"\n                   ║{CenterString(CurrentPlayer.PlayerName, 29)}║" +
            "\n                   ║          Killed by          ║" +
            $"\n                   ║{CenterString(endingCause,29)}║" +
            "\n                   ║                             ║" +
            $"\n                   ║{CenterString(CurrentPlayer.Gold.ToString() + " Au", 29)}║" +
            $"\n                   ║           {DateTime.Now.Year + " "}             ║" +
            "\n                   ║                             ║" +
            "\n                   ║                             ║" +
            "\n                 __\\/ (\\//(\\/ \\(//)\\)\\/(//)\\)//(\\__" +
            "\n";

            return screen;

        }

        private void EvaluatePlayer()
        {
            // If the player's scheduled to get hungry on the current turn, update the properties.
            if (CurrentPlayer.HungerTurn == CurrentTurn)
            {
                CurrentPlayer.HungerState = (CurrentPlayer.HungerState > 0)
                    ? --CurrentPlayer.HungerState : 0;

                // If the player is now hungry, weak or faint, add some turns.
                if (CurrentPlayer.HungerState < Player.HungerLevel.Satisfied
                    && CurrentPlayer.HungerState > Player.HungerLevel.Dead)
                {
                    CurrentPlayer.HungerTurn += Player.HUNGER_TURNS;
                    cStatus = $"You are starting to feel {CurrentPlayer.HungerState.ToString().ToLower()}";
                }
            }

            // If the player is FAINT, decide if they should faint on this move.
            if (CurrentPlayer.HungerState == Player.HungerLevel.Faint && CurrentPlayer.Immobile == 0)
            {
                if (rand.Next(1, 101) < FAINT_PCT)
                {
                    CurrentPlayer.Immobile = CurrentTurn + rand.Next(1, MAX_TURN_LOSS + 1);
                    cStatus = "You fainted from lack of food.";
                }
            }
            // If the player is now dead, signal the game over.
            else if (CurrentPlayer.HungerState == Player.HungerLevel.Dead)
                GameMode = DisplayMode.GameOver;

        }

        private void DisplayInventory()
        {
            // Switch the screen to the player's inventory.

            GameMode = DisplayMode.Inventory;
            ScreenDisplay = "\n\n";

            foreach(InventoryLine line in Inventory.InventoryDisplay(CurrentPlayer.PlayerInventory))
                ScreenDisplay += line.Description + "\n";
        }

        private void RestoreMap()
        {
            // Restore the map display.
            if (GameMode == DisplayMode.Inventory || GameMode == DisplayMode.Help)
            {
                GameMode = DisplayMode.Primary;
                ScreenDisplay = DevMode ? CurrentMap.MapCheck() : CurrentMap.MapText(CurrentPlayer.Location);
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
                CurrentPlayer.Location = CurrentMap.AddCharacterToMap(Player.CHARACTER);
                CurrentMap.DiscoverRoom(CurrentPlayer.Location.X, CurrentPlayer.Location.Y);
                                
                cStatus = "";
            }

            // Place Amulet on last level.
            if(CurrentLevel == MAX_LEVEL)
                CurrentMap.AddAmuletToMap();

        }

        private void ReplaceMap()
        {
            // Dev mode only - replace the map for testing.
            CurrentMap = new MapLevel();
            CurrentPlayer.Location = CurrentMap.AddCharacterToMap(Player.CHARACTER);
        }

        public void MoveCharacter(Player player, MapLevel.Direction direct)
        {
            char visibleCharacter;
            bool canMove, autoMove;

            // Move character if possible.
            // Clear the status.
            cStatus = "";

            do
            {
                // Get surrounding characters
                Dictionary<MapLevel.Direction, MapSpace> adjacent =
                    CurrentMap.SearchAdjacent(player.Location!.X, player.Location.Y);

                visibleCharacter = adjacent[direct].PriorityChar();

                // The player can move if the visible character is within a room or a hallway and there's no monster there.
                canMove = (MapLevel.SpacesAllowed.Contains(visibleCharacter) || adjacent[direct].ContainsItem()) &&
                    adjacent[direct].DisplayCharacter == null;

                autoMove = FastPlay && adjacent[direct].FastMove() &&
                    CurrentMap.SearchAdjacent(MapLevel.HALLWAY, adjacent[direct].X, adjacent[direct].Y).Count < 3;

                if (canMove)
                {
                    player.Location = CurrentMap.MoveDisplayItem(player.Location, adjacent[direct]);
                    // If the character has moved.

                    // If this is a doorway, determine if the room is lighted.
                    if (player.Location.MapCharacter == MapLevel.ROOM_DOOR)
                        CurrentMap.DiscoverRoom(player.Location.X, player.Location.Y);

                    // Discover the spaces surrounding the player.
                    CurrentMap.DiscoverSurrounding(player.Location.X, player.Location.Y);

                    // Respond to items on map.
                    if (player.Location.Occupied())
                    {
                        if (player.Location.ItemCharacter == MapLevel.GOLD)
                            PickUpGold();
                        else if (player.Location.MapInventory != null)
                            cStatus = AddInventory();
                    }

                    // Complete the turn actions.
                    CompleteTurn();
                }

                // Determine if player can move automatically on FastPlay.  Three or more adjacent
                // hallway spaces indicate a junction which needs to stop FastPlay.

            } while (autoMove);

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


        private bool Eat(char? ListItem)
        {
            bool retValue = false;
            List<Inventory> items;
            int foodValue = 0;

            if (GameMode != DisplayMode.Inventory)
            {
                // Verify the player has something they can eat.
                items = (from inv in CurrentPlayer.PlayerInventory
                            where inv.ItemCategory == Inventory.InvCategory.Food
                            select inv).ToList();

                if (items.Count > 0)
                {
                    // If there's something edible, show the inventory
                    // and let the player select it.  Set to return and exit.
                    DisplayInventory();
                    cStatus = "Please select something to eat.";
                    ReturnFunction = Eat;
                }
                else
                    // Otherwise, they'll be hungry for awhile.
                    cStatus = "You don't have anything to eat.";
            }
            else
            {
                // Get the selected item.
                items = (from InventoryLine in Inventory.InventoryDisplay(CurrentPlayer.PlayerInventory)
                            where InventoryLine.ID == ListItem
                            select InventoryLine.InvItem).ToList();

                if (items.Count > 0)
                {
                    // Call the appropriate delegate and remove the item
                    // from inventory.
                    // TODO: In this case, it makes more sense to complete this here than in a delegate function. Continue to evaluate as other inventory is implemented.
                    if (items[0].ItemCategory != Inventory.InvCategory.Food)
                    { 
                        cStatus = "You can't eat THAT!";
                        retValue = false;
                    }
                    else
                    { 
                        foodValue = rand.Next(Inventory.MIN_FOODVALUE, Inventory.MAX_FOODVALUE + 1);
                        CurrentPlayer.HungerTurn += foodValue;
                        CurrentPlayer.HungerState = Player.HungerLevel.Satisfied;
                        CurrentPlayer.PlayerInventory.Remove(items[0]);
                        RestoreMap();
                        cStatus = "Mmmm, that hit the spot.";
                        retValue = true;
                    }
                }
                else
                {
                    // Process non-existent option.
                    cStatus = "Please select something to eat.";
                    RestoreMap();
                    retValue = false;
                }

            }

            return retValue;
        }

        private bool DropInventory(char? ListItem)
        {
            bool retValue = false;
            List<Inventory> items;

            if (GameMode != DisplayMode.Inventory)
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

            if (GameMode != DisplayMode.Inventory)
                this.ReturnFunction = null;

            return retValue;
        }

        private string AddInventory()
        {
            // Inventory management.

            Inventory foundItem;

            string retValue = "";

            // If the player found the Amulet ...
            // TODO: Change this after the Amulet has been added as an inventory item. 
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
            else if (CurrentPlayer.Location!.MapInventory != null)
            {
                // For everything else, pick it up if it can fit in inventory.
                if(CurrentPlayer.PlayerInventory.Count < Player.INVENTORY_LIMIT)
                {
                    CurrentPlayer.Location!.ItemCharacter = null;                    
                    
                    if (CurrentPlayer.Location.MapInventory != null)
                    {
                        foundItem = CurrentPlayer.Location.MapInventory;

                        //TODO:  This will need to be changed based on item ident status.
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
