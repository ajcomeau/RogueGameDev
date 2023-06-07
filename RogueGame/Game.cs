﻿using RogueGame;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RogueGame
{    
    /// <summary>
    /// Main class for managing game state and progress.
    /// </summary>
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
        private const int KEY_F = 70;
        private const int KEY_ESC = 27;
        private const int KEY_HELP = 191;
        
        /// <summary>
        /// Probability of search revealing hidden doors, etc..
        /// </summary>
        private const int SEARCH_PCT = 20; 
        /// <summary>
        /// Maximum dungeon level
        /// </summary>
        public const int MAX_LEVEL = 26;   
        /// <summary>
        /// Probability of fainting at any given point when FAIN
        /// </summary>
        private const int FAINT_PCT = 33;   
        /// <summary>
        /// Maximum turns to lose when fainting, etc..
        /// </summary>
        private const int MAX_TURN_LOSS = 5;
        private const int MAX_PURSUIT = 7;
        /// <summary>
        /// Lists modes to be used for displaying different screens.
        /// </summary>
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

        /// <summary>
        /// Current map object being shown.
        /// </summary>
        public MapLevel CurrentMap { get; set; }                    
        /// <summary>
        /// Current game level.
        /// </summary>
        public int CurrentLevel { get; set; }
        /// <summary>
        /// Current player object
        /// </summary>
        public Player CurrentPlayer { get; }
        /// <summary>
        /// Current turn number
        /// </summary>
        public int CurrentTurn { get; set; }
        /// <summary>
        /// String showing current contents of the screen.
        /// </summary>
        public string ScreenDisplay { get; set; }
        /// <summary>
        /// Current display mode indicating which screen is showing
        /// </summary>
        public DisplayMode GameMode { get; set; }
        /// <summary>
        /// Developer mode ON / OFF
        /// </summary>
        public bool DevMode { get; set; }
        /// <summary>
        /// Fast Play mode ON / OFF
        /// </summary>
        public bool FastPlay { get; set; }
        /// <summary>
        /// Delgate used to return to function that enables an inventory item to be used.
        /// </summary>
        public Func<char?, bool>? ReturnFunction { get; set; } 
        /// <summary>
        /// Status message for top of screen.
        /// </summary>
        private string cStatus;  

        /// <summary>
        /// Random number generator
        /// </summary>
        public static Random rand = new Random();

        /// <summary>
        /// Read-only property to return current status message.
        /// </summary>
        public string StatusMessage
        {
            get { return cStatus; }
        }

        /// <summary>
        /// Get current player stats display for bottom of screen.
        /// </summary>
        public string StatsDisplay()
        {
            string retValue = "";

            if (GameMode == DisplayMode.Primary)
            {
                // Assemble stats display for the bottom of the screen.
                retValue = $"Level: {CurrentLevel}  ";
                retValue += $"HP: {CurrentPlayer.MaxHP}/{CurrentPlayer.CurrentHP}  ";
                retValue += $"Strength: {CurrentPlayer.MaxStrength}/{CurrentPlayer.CurrentStrength}  ";
                retValue += $"Gold: {CurrentPlayer.Gold}  ";
                retValue += $"Armor: {(CurrentPlayer.Armor != null ? CurrentPlayer.Armor.ArmorClass : 0)}  ";
                retValue += $"Turn: {CurrentTurn}  ";
                retValue += $"Exp: {CurrentPlayer.ExperienceLevel()}/{CurrentPlayer.Experience}";

                
                if (CurrentPlayer.HungerState < Player.HungerLevel.Satisfied)
                    retValue += $"  {CurrentPlayer.HungerState}     ";
            }
            else
                retValue = "";

            return retValue;
        }

        /// <summary>
        /// Primary constructor for starting new game.
        /// </summary>
        /// <param name="PlayerName"></param>
        public Game(string PlayerName) {

            // Setup a new game with a map and a player.
            // Put the player on the map and set the opening status.
            
            this.CurrentLevel = 1;            
            // Create new player.
            this.CurrentPlayer = new Player(PlayerName);
            // Generate the new map, add player and shroud the map.
            this.CurrentMap = new MapLevel(CurrentLevel, CurrentPlayer);
            this.CurrentPlayer.Location = CurrentMap.GetOpenSpace(false);
            this.CurrentMap.ShroudMap();

            // Activate the player's current room.
            this.CurrentMap.DiscoverRoom(CurrentPlayer.Location.X, CurrentPlayer.Location.Y);

            // Set starting turn and show welcome message.
            this.CurrentTurn = 1;
            this.GameMode = DisplayMode.Primary;
            cStatus = $"Welcome to the Dungeon, {CurrentPlayer.PlayerName} ... (Press ? for list of commands.)";

            // Set the current screen display.
            this.ScreenDisplay = DevMode ? this.CurrentMap.MapCheck() : this.CurrentMap.MapText();
          
        }

        /// <summary>
        /// Method for responding to key presses.
        /// </summary>
        /// <param name="KeyVal">ASCII value of key pressed.</param>
        /// <param name="Shift">If SHIFT is held down.</param>
        /// <param name="Control">If CTRL is held down.</param>
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
                    case KEY_F: // Fast Play
                        FastPlay = !FastPlay;
                        cStatus = FastPlay ? "Fast Play mode ON." : "Fast Play mode OFF";
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
                        MovePlayer(CurrentPlayer, MapLevel.Direction.West);
                        break;
                    case KEY_NORTH:
                        MovePlayer(CurrentPlayer, MapLevel.Direction.North);
                        break;
                    case KEY_EAST:
                        MovePlayer(CurrentPlayer, MapLevel.Direction.East);
                        break;
                    case KEY_SOUTH:
                        MovePlayer(CurrentPlayer, MapLevel.Direction.South);
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
                ScreenDisplay = DevMode ? this.CurrentMap.MapCheck() : this.CurrentMap.MapText();

            switch (GameMode)
            {
                case DisplayMode.GameOver:
                    ScreenDisplay = RIPScreen();
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// Carries out necessary actions to finish the turn.
        /// </summary>
        private void CompleteTurn()
        {
            do
            {
                // Perform whatever actions needed to complete turn
                // (i.e. monster moves)
                foreach (Monster monster in CurrentMap.ActiveMonsters)
                    MoveMonster(monster);

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

        /// <summary>
        /// Centers a text string for display.
        /// </summary>
        /// <param name="Text">Text to be centered.</param>
        /// <param name="Spaces">Total number of spaces in displayed string.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns help screen text.
        /// </summary>
        /// <returns></returns>
        private string HelpScreen()
        {
            return "\n\nArrows - movement\n\n" +
                "d - drop inventory\n\n" +
                "e - eat\n\n" +
                "s - search for hidden doorways\n\n" +
                "i - show inventory\n\n" +
                "F - Fast Play mode ON / OFF\n\n" +
                "> - go down a staircase\n\n" +
                "< - go up a staircase(requires Amulet from level 26)\n\n" +
                "ESC - return to map.\n\n" +
                "CTRL-D - Developer mode.  See entire map.\n" +
                "CTRL-N - Change out map for new one in dev mode.";
        }
        
        /// <summary>
        /// Creates and returns R.I.P. screen.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Evaluate and adjust all player stats at end of turn.
        /// </summary>
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

        /// <summary>
        /// Bring up inventory screen for viewing.
        /// </summary>
        private void DisplayInventory()
        {
            // Switch the screen to the player's inventory.

            GameMode = DisplayMode.Inventory;
            ScreenDisplay = "\n\n";

            foreach(InventoryLine line in Inventory.InventoryDisplay(CurrentPlayer.PlayerInventory))
                ScreenDisplay += line.Description + "\n";
        }

        /// <summary>
        /// Restore current map after viewing another screen.
        /// </summary>
        private void RestoreMap()
        {
            // Restore the map display.
            if (GameMode == DisplayMode.Inventory || GameMode == DisplayMode.Help)
            {
                GameMode = DisplayMode.Primary;
                ScreenDisplay = DevMode ? CurrentMap.MapCheck() : CurrentMap.MapText();
            }
        }

        /// <summary>
        /// Search surrounding area for hidden items and display them.
        /// </summary>
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
            else cStatus = "";
        }

        /// <summary>
        /// Change the current map level number.
        /// </summary>
        /// <param name="Change"></param>
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
                CurrentMap = new MapLevel(CurrentLevel, CurrentPlayer);
                CurrentMap.ShroudMap();
                CurrentLevel += Change;
                CurrentPlayer.Location = CurrentMap.GetOpenSpace(false);
                CurrentMap.DiscoverRoom(CurrentPlayer.Location.X, CurrentPlayer.Location.Y);
                                
                cStatus = "";
            }
        }

        /// <summary>
        /// Dev mode: Change out current map for a new one.
        /// </summary>
        private void ReplaceMap()
        {
            // Dev mode only - replace the map for testing.
            CurrentMap = new MapLevel(CurrentLevel, CurrentPlayer);
            CurrentPlayer.Location = CurrentMap.GetOpenSpace(false);
        }

        /// <summary>
        /// Move character in specified direction.
        /// </summary>
        /// <param name="player">Player object to be moved</param>
        /// <param name="direct">Direction enumeration reference</param>
        public void MovePlayer(Player player, MapLevel.Direction direct)
        {
            char visibleCharacter;
            bool canMove, stopMoving = false, turnComplete = false;
            Inventory? invFound = null;
            Dictionary<MapLevel.Direction, MapSpace> adjacent =
                CurrentMap.SearchAdjacent(player.Location!.X, player.Location.Y);

            // Move character if possible.
            // Clear the status.
            cStatus = "";

            do
            {
                // Inspect target character
                visibleCharacter = adjacent[direct].PriorityChar();
                invFound = CurrentMap.DetectInventory(adjacent[direct]);

                // The player can move if the visible character is within a room or a hallway and there's no monster there.
                canMove = MapLevel.SpacesAllowed.Contains(visibleCharacter) || invFound != null;

                if (canMove)
                {
                    // Move the character.
                    player.Location = adjacent[direct];

                    // If this is a doorway, determine if the room is lighted.
                    if (player.Location.MapCharacter == MapLevel.ROOM_DOOR)
                        CurrentMap.DiscoverRoom(player.Location.X, player.Location.Y);

                    // Discover the spaces surrounding the player and note if something is found.
                    stopMoving = CurrentMap.DiscoverSurrounding(player.Location.X, player.Location.Y);

                    // Respond to items on map.
                    if (invFound != null) cStatus = AddInventory();

                    // Player turn completed.
                    turnComplete = true;
                }
                else if (CurrentMap.DetectMonster(adjacent[direct]) != null)
                {
                    Monster opponent = (from Monster monster in CurrentMap.ActiveMonsters
                                                    where monster.Location == adjacent[direct]
                                                    select monster).First();

                    Attack(CurrentPlayer, opponent);

                    // Player turn completed.
                    turnComplete = true;
                }

                // Complete turn if indicated.
                if (turnComplete) { CompleteTurn(); }

                // Determine if player can move automatically on FastPlay.  Three or more adjacent
                // hallway spaces indicate a junction which needs to stop FastPlay.
                adjacent = CurrentMap.SearchAdjacent(player.Location!.X, player.Location.Y);

            } while (!stopMoving && CanAutoMove(player.Location, adjacent[direct]));
        }

        private void Attack(Player Attacker, Monster Defender)
        {

            // Basic attack method to get monsters out of the way.  In progress.

            // Start with a 50% chance of landing a punch.
            bool hitSuccess = rand.Next(1, 101) > 50;
            int damage = 0;

            // Up to 50% of the monster's HP.
            if (hitSuccess)
            {
                cStatus = $"You hit the {Defender.MonsterName.ToLower()}.";
                damage = rand.Next(1, (int)(Defender.MaxHP / 2) + 1);
            }
            else cStatus = $"You missed the {Defender.MonsterName.ToLower()}.";

            Defender.HPDamage += damage;

            // If the monster has been defeated, remove it from the map.
            if(Defender.CurrentHP < 1)
            {
                CurrentMap.ActiveMonsters.Remove(Defender);
                cStatus = $"You defeated the {Defender.MonsterName.ToLower()}.";
            }
        }

        public void MoveMonster(Monster monster)
        {
            char visibleCharacter;
            int tentativeDistance;
            bool timeToMove, canMove;
            MapLevel.Direction direct, direct90, direct270;
            MapSpace destinationSpace = monster.Location!;

            // Move monster if possible.

            timeToMove = (monster.CurrentState == Monster.Activity.Wandering ||
                rand.Next(1, 101) >= monster.Inertia);

            if (timeToMove)
            {
                // Get adjacent spacees.
                Dictionary<MapLevel.Direction, MapSpace> adjacent =
                    CurrentMap.SearchAdjacent(monster.Location!.X, monster.Location.Y);

                // Get the current distance of the player from the monster.
                int playerDistance = Math.Abs(CurrentPlayer.Location!.X - monster.Location.X)
                    + Math.Abs(CurrentPlayer.Location.Y - monster.Location.Y);

                // If the monster has not chosen a direction, find out if the player is near.
                if (playerDistance <= MAX_PURSUIT && monster.Aggressive)
                {
                    // If the player is within pursuit distance, search the adjacent spaces
                    // for one that's available and closest to the player.
                    foreach (KeyValuePair<MapLevel.Direction, MapSpace> adjSpace in adjacent)
                    {
                        if (MapLevel.SpacesAllowed.Contains(adjSpace.Value.PriorityChar()) || 
                            CurrentMap.DetectInventory(adjSpace.Value) != null)
                        {
                            tentativeDistance = Math.Abs(adjSpace.Value.X - CurrentPlayer.Location.X)
                                + Math.Abs(adjSpace.Value.Y - CurrentPlayer.Location.Y);

                            // If the next space is closer, set it as the new destination.
                            if (tentativeDistance < Math.Abs(CurrentPlayer.Location.X - destinationSpace.X)
                                + Math.Abs(CurrentPlayer.Location.Y - destinationSpace.Y))
                            {
                                destinationSpace = adjSpace.Value;
                                monster.Direction = adjSpace.Key;
                            }
                        }
                    }
                }

                // If the monster is still feeling aimless, just pick one except 'None'.
                if (monster.Direction == null)
                {
                    do
                    {
                        monster.Direction = (MapLevel.Direction)rand.Next(-2, 3);
                    } while (monster.Direction == MapLevel.Direction.None);
                }

                // Get relative directions to monster's choice.
                direct = (MapLevel.Direction)monster.Direction!;
                direct90 = CurrentMap.GetDirection90(direct);
                direct270 = CurrentMap.GetDirection270(direct);

                // Inspect target character
                visibleCharacter = adjacent[direct].PriorityChar();

                // The monster can move if the visible character is within a room or a hallway
                // and there's nobody else there.

                canMove = canMove = MapLevel.SpacesAllowed.Contains(visibleCharacter) || 
                    CurrentMap.DetectInventory(adjacent[direct]) != null;

                if (canMove)
                    monster.Location = adjacent[direct];
                else
                {
                    if (CurrentMap.DetectMonster(adjacent[direct]) != null && monster.Aggressive)
                        // The monster just tried to run into another monster.  For now, just change direction.
                        // TODO:  This might need to result in an attack.
                        monster.Direction = rand.Next(1, 101) > 50 ? direct270 : direct90;
                    else if (adjacent[direct] == CurrentPlayer.Location)
                        // The monster just tried to run into the player.
                        monster.Direction = rand.Next(1, 101) > 50 ? direct270 : direct90;
                    else { 
                        // Change direction and decide on a current state.
                        monster.Direction = rand.Next(1, 101) > 50 ? direct270 : direct90;
                        if (rand.Next(1, 101) < monster.Inertia) { monster.CurrentState = Monster.Activity.Resting; }
                }
                }
            }


        }

        /// <summary>
        /// In Fast Play, evaluate the next space in a given direciton for moving.
        /// </summary>
        /// <param name="Origin">Starting or current space</param>
        /// <param name="Target">Target space</param>
        /// <returns></returns>
        private bool CanAutoMove(MapSpace Origin, MapSpace Target)
        {
            // Determine if the player can keep moving in the
            // current direction. Target space must be eligible
            // and the same map character as the current space.
            // If the player is in a hallway, they must stop at any junctions.
            return FastPlay
                & CurrentMap.DetectMonster(Target) == null // No monster
                & CurrentMap.DetectInventory(Target) == null // No mnventory  
                & Target.MapCharacter == Origin.MapCharacter 
                & MapLevel.SpacesAllowed.Contains(Target.PriorityChar())
                & CurrentMap.SearchAdjacent(MapLevel.HALLWAY, Origin.X, Origin.Y).Count < 3;
        
        }


        /// <summary>
        /// Eat specified food.
        /// </summary>
        /// <param name="ListItem">Selected list item</param>
        /// <returns>True / False indicating if item was eaten</returns>
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

        /// <summary>
        /// Drop specified inventory on map.
        /// </summary>
        /// <param name="ListItem">Selected item to be dropped</param>
        /// <returns>True / False indicating success</returns>
        private bool DropInventory(char? ListItem)
        {
            bool retValue = false;
            List<InventoryLine> items;

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
                    select InventoryLine).ToList();
                
                if (items.Count > 0)
                {
                    if(CurrentMap.DetectInventory(CurrentPlayer.Location!) == null)
                    {
                        if (items[0].InvItem.ItemCategory == Inventory.InvCategory.Ammunition
                            && items[0].InvItem.IsGroupable)
                        {
                            // We're dropping the entire batch so update the amount.
                            items[0].InvItem.Amount = items[0].Count;
                            // For ammunition, remove all items from the slot.
                            CurrentPlayer.PlayerInventory =
                                CurrentPlayer.PlayerInventory.Where(x => x.RealName != items[0].InvItem.RealName).ToList();
                                                        
                            cStatus = $"You dropped {Inventory.ListingDescription(items[0].Count, items[0].InvItem)}.";
                        }
                        else
                        {
                            items[0].InvItem.Amount = 1;
                            CurrentPlayer.PlayerInventory.Remove(items[0].InvItem);
                            cStatus = $"You dropped {Inventory.ListingDescription(1, items[0].InvItem)}.";
                        }

                        items[0].InvItem.Location = CurrentPlayer.Location;
                        CurrentMap.MapInventory.Add(items[0].InvItem);
                        RestoreMap();
                        retValue = true;                        
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

        /// <summary>
        /// Add found items to player's inventory.
        /// </summary>
        /// <returns>Display string with description of item.</returns>
        private string AddInventory()
        {
            // Inventory management.
            int itemAmount = 1;
            bool addToInventory = false;          
            List<Inventory> tempInventory = CurrentPlayer.PlayerInventory;
            Inventory? foundItem = CurrentMap.DetectInventory(CurrentPlayer.Location!);
            string retValue = "";

            if (foundItem != null)
            { 
                if(foundItem.ItemCategory == Inventory.InvCategory.Gold)
                {
                    // Add the gold at the current location to the player's purse and remove
                    // it from the map.
                    int goldAmt = rand.Next(MapLevel.MIN_GOLD_AMT, MapLevel.MAX_GOLD_AMT + 1);
                    CurrentPlayer.Gold += goldAmt;
                    CurrentMap.MapInventory.Remove(foundItem);
                    cStatus = $"You picked up {goldAmt} pieces of gold.";
                }
                else
                {
                    // Determine if there's room in inventory for the item.
                    // If it's groupable and the player already has it in a slot, add it.
                    // Otherwise, if there's an extra slot available, add it.
                    addToInventory = (foundItem.IsGroupable && CurrentPlayer.SearchInventory(foundItem.RealName));
                    if (!addToInventory) addToInventory =
                            Inventory.InventoryDisplay(CurrentPlayer.PlayerInventory).Count + 1 <= Player.INVENTORY_LIMIT;

                    // If the additional inventory fits within the limit, keep the item.
                    // Otherwise, remove it.                
                    if (addToInventory)
                    {
                        // When the item is actually added, it needs to be a single item.
                        itemAmount = foundItem.Amount;
                        foundItem.Amount = 1;
                        // Move the item to the player's inventory.
                        for (int i = 1; i <= itemAmount; i++)
                            CurrentPlayer.PlayerInventory.Add(Inventory.GetInventoryItem(foundItem.RealName)!);

                        retValue = $"You picked up {Inventory.ListingDescription(itemAmount, foundItem)}.";
                        CurrentMap.MapInventory.Remove(foundItem);

                        if(foundItem.ItemCategory == Inventory.InvCategory.Amulet) 
                        {
                            CurrentPlayer.HasAmulet = true;
                            retValue = "You found the Amulet of Yendor!  It has been added to your inventory.";
                        }
                    }
                    else
                    {
                        CurrentPlayer.PlayerInventory.Remove(foundItem);
                        retValue = "The item won't fit in your inventory.";
                    }
                }
            }

            return retValue;
        }

    }
}
