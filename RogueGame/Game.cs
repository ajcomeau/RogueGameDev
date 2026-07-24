using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static RogueGame.Inventory;

namespace RogueGame
{    
    /// <summary>
    /// Main class for managing game state and progress.
    /// </summary>
    internal class Game
    {
        #region Constants
        // Movement keys
        private const int KEY_WEST = 37;
        private const int KEY_NORTH = 38;
        private const int KEY_EAST = 39;
        private const int KEY_SOUTH = 40;
        // Stairway keys
        private const int KEY_UPLEVEL = 188;
        private const int KEY_DOWNLEVEL = 190;
        // Command keys
        private const int KEY_Q = 81;
        private const int KEY_R = 82;
        private const int KEY_S = 83;
        private const int KEY_D = 68;
        private const int KEY_H = 72;
        private const int KEY_N = 78;
        private const int KEY_E = 69;
        private const int KEY_I = 73;
        private const int KEY_F = 70;
        private const int KEY_T = 84;
        private const int KEY_W = 87;
        private const int KEY_ESC = 27;
        private const int KEY_HELP = 191;
        // Etc.
        private const int HEAL_RATE = 12;  // Number of turns between each health regen.
        private const int HP_LEVEL_INCREASE = 10; // Maximum HP to add with each exp. level.
        
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
        /// <summary>
        /// Max number of spaces for monster to detect and pursue player.
        /// </summary>
        private const int MAX_PURSUIT = 7;
        /// <summary>
        /// Probability that wearables will be cursed.
        /// </summary>
        private const int ITEM_CURSE_PROB = 15;
        /// <summary>
        /// Degree of confusion caused by various items.
        /// </summary>
        private const int DEGREE_CONFUSION = 50;        
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

        #endregion

        #region Properties
        /// <summary>
        /// Game level Inventory instance for accessing functions.
        /// </summary>
        private Inventory GameInventory { get; }
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
        /// Current display mode indicating which screen is showing
        /// </summary>
        public DisplayMode GameMode { get; set; }
        /// <summary>
        /// Developer mode ON / OFF
        /// </summary>
        public bool DevMode { get; set; }
        /// <summary>
        /// Hulk Mode - Monsters killed with a single punch
        /// </summary>
        public bool HulkMode { get; set; }
        /// <summary>
        /// Fast Play mode ON / OFF
        /// </summary>
        public bool FastPlay { get; set; }
        /// <summary>
        /// How did the player die?
        /// </summary>
        public string? CauseOfDeath { get; set; }
        /// <summary>
        /// Delgate used to return to function that enables an inventory item to be used.
        /// </summary>
        public Func<char?, bool>? ReturnFunction { get; set; }         
        /// <summary>
        /// Status message for top of screen.
        /// </summary>
        public BindingList<string> StatusList = new BindingList<string>();
        /// <summary>
        /// Record field to store KeyPress and CTRL / SHIFT options.
        /// </summary>
        /// <param name="KeyPressed"></param>
        /// <param name="Ctrl"></param>
        /// <param name="Shift"></param>
        private record struct recKeyChord(int KeyPressed, bool Ctrl, bool Shift);
        /// <summary>
        /// Searchable dictionary field to hold possible key presses in game.
        /// </summary>
        private Dictionary<recKeyChord, (Action method, string desc)> KeyActions;
        /// <summary>
        /// Searchable dictionary field to hold delegates for inventory items.
        /// </summary>
        private Dictionary<(Inventory.InvCategory InvCat, string InvName), Func<bool>> InventoryActions; 
        /// <summary>
        /// Random number generator
        /// </summary>
        public static Random rand = new Random();
        /// <summary>
        /// Class boolean to indicate if there's a turn in progress.
        /// </summary>
        private bool TurnInProgress = false;
        /// <summary>
        /// Class boolean to indicate if a key command has been processed.
        /// </summary>
        private bool keyHandled = false;

        #endregion

        #region DisplayFunctions

        /// <summary>
        /// Returns help screen text.
        /// </summary>
        /// <returns></returns>
        private void HelpScreen()
        {
            string screenText = "Command list - Press ESC to return.\n\n";
            bool firstColumn = true;

            foreach (var (keyChord, (method, desc)) in KeyActions)
            {
                if (firstColumn)
                    screenText += desc + new string(' ', 40 - desc.Length);
                else
                    screenText += desc + "\n";

                firstColumn = !firstColumn;
            }

            this.CurrentMap.UpdateDisplayFromText(screenText);
        }
        /// <summary>
        /// Creates and returns R.I.P. screen.
        /// </summary>
        /// <returns></returns>
        private void RIPScreen()
        {
            string screen;

            if (CauseOfDeath == null) CauseOfDeath = "mysterious forces.";

            // Assemble the ASCII graphic and return it.
            screen = "\n\n\n\n\n\n" +
            "\n                        ╔═════════════════════════════╗" +
            "\n                        ║                             ║" +
            "\n                        ║                             ║" +
            "\n                        ║                             ║" +
            "\n                        ║        REST IN PEACE        ║" +
            "\n                        ║                             ║" +
            $"\n                        ║{CenterString(CurrentPlayer.PlayerName, 29)}║" +
            "\n                        ║          Killed by          ║" +
            $"\n                        ║{CenterString(CauseOfDeath, 29)}║" +
            "\n                        ║                             ║" +
            $"\n                        ║{CenterString(CurrentPlayer.Gold.ToString() + " Au", 29)}║" +
            $"\n                        ║           {DateTime.Now.Year + " "}             ║" +
            "\n                        ║                             ║" +
            "\n                        ║                             ║" +
            "\n                      __\\/ (\\//(\\/ \\(//)\\)\\/(//)\\)//(\\__" +
            "\n";

            this.CurrentMap.UpdateDisplayFromText(screen);

        }
        /// <summary>
        /// Add status line to status box.
        /// </summary>
        /// <param name="Status">Message to add to status display.</param>
        /// <param name="Confirm">True to display a message box requiring user confirmation.</param>
        private void UpdateStatus(string Status, bool Confirm)
        {
            if (Confirm)
            {
                StatusList.Insert(0, Status);
                MessageBox.Show(Status);
            }
            else StatusList.Insert(0, Status);
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
        /// Get current player stats display for bottom of screen.
        /// </summary>
        /// <returns></returns>
        public string StatsDisplay()
        {
            string retValue = "";

            if (GameMode == DisplayMode.Primary)
            {
                // Assemble stats display for the bottom of the screen.
                retValue = $"Level: {CurrentLevel}  ";
                retValue += $"HP: {CurrentPlayer.CurrentHP}/{CurrentPlayer.MaxHP}  ";
                retValue += $"Strength: {CurrentPlayer.CurrentStrength}/{CurrentPlayer.MaxStrength}  ";
                retValue += $"Gold: {CurrentPlayer.Gold}  ";
                retValue += $"Armor: {(CurrentPlayer.Armor != null ? CurrentPlayer.Armor.ArmorClass + CurrentPlayer.Armor.Increment : 0)}  ";
                retValue += $"Turn: {CurrentTurn}  ";
                retValue += $"Exp: {CurrentPlayer.ExpLevel}/{CurrentPlayer.Experience}";
                
                if (CurrentPlayer.HungerState < Player.HungerLevel.Satisfied)
                    retValue += $"  {CurrentPlayer.HungerState}     ";
            }
            else
                retValue = "";

            return retValue;
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
                if (DevMode)
                    CurrentMap.MapCheck();
                else
                    CurrentMap.MapText();
            }
        }
        /// <summary>
        /// Dev mode: Change out current map for a new one.
        /// </summary>
        private void ReplaceMap()
        {
            // Dev mode only - replace the map for testing.
            CurrentMap = new MapLevel(CurrentLevel, CurrentPlayer, GameInventory);
            CurrentPlayer.Location = CurrentMap.GetOpenSpace(false);
        }
        /// <summary>
        /// Bring up inventory screen for viewing.
        /// </summary>
        public void DisplayInventory()
        {
            string screenText = "Inventory List\n\n";
            // Switch the screen to the player's inventory.
            GameMode = DisplayMode.Inventory;

            foreach (InventoryLine line in GameInventory.InventoryDisplay(CurrentPlayer.PlayerInventory))
                if (line.InvItem == CurrentPlayer.Armor)
                    screenText += line.Description + " (being worn)\n";  // current armor
                else if (CurrentPlayer.Wielding != null && line.InvItem == CurrentPlayer.Wielding)
                    screenText += line.Description + " (wielding)\n";  // weapon
                else if (CurrentPlayer.RightHand != null && line.InvItem == CurrentPlayer.RightHand)
                    screenText += line.Description + " (on right hand)\n";  // ring
                else if (CurrentPlayer.LeftHand != null && line.InvItem == CurrentPlayer.LeftHand)
                    screenText += line.Description + " (on left hand)\n";  // ring
                else
                    screenText += line.Description + "\n";

            this.CurrentMap.UpdateDisplayFromText(screenText);
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Primary constructor for starting new game.
        /// </summary>
        /// <param name="PlayerName">Name of current player.</param>
        public Game(string PlayerName)
        {
            this.GameInventory = new Inventory(true);
            // Setup a new game with a map and a player.
            // Put the player on the map and set the opening status.

            this.CurrentLevel = 1;
            // Create new player.
            this.CurrentPlayer = new Player(PlayerName, GameInventory.GetAssignedInventory());
            // Initialize possible commands
            InitializeCommands();
            // Generate the new map, add player and shroud the map.
            this.CurrentMap = new MapLevel(CurrentLevel, CurrentPlayer, GameInventory);
            //this.CurrentPlayer.Location = CurrentMap.GetOpenSpace(false);
            this.CurrentMap.ShroudMap();

            // Activate the player's current room.
            this.CurrentMap.DiscoverRoom(CurrentPlayer.Location.X, CurrentPlayer.Location.Y);

            // Set starting turn and show welcome message.
            this.CurrentTurn = 1;
            this.GameMode = DisplayMode.Primary;
            UpdateStatus($"Welcome to the Dungeon, {CurrentPlayer.PlayerName} ... (Press ? for list of commands.)", false);

            // Set the current screen display.
            if (DevMode)
                this.CurrentMap.MapCheck();
            else
                this.CurrentMap.MapText();
        }

        /// <summary>
        /// Initialize all key commands with Action delegates and descriptions.
        /// </summary>
        private void InitializeCommands()
        {
            // Create searchable dictionary of key commands and delegates to methods.
            KeyActions = new Dictionary<recKeyChord, (Action, string)>
            {
                {new recKeyChord(KEY_DOWNLEVEL, false, true), (DownStairsProc, "> - Go downstairs")},
                {new recKeyChord(KEY_UPLEVEL, false, true), (UpstairsProc, "< - Go upstairs (requires amulet)")},
                {new recKeyChord(KEY_F, false, true), (FastPlayProc, "F - Fast Play ON / OFF")},
                {new recKeyChord(KEY_HELP, false, true), (HelpProc, "? - Show help screen")},
                {new recKeyChord(KEY_T, false, true), (RemoveArmorProc, "R - Remove armor")},
                {new recKeyChord(KEY_W, false, true), (WearArmorProc, "W - Wear armor")},
                {new recKeyChord(KEY_SOUTH, false, false), (SouthProc, "Down arrow - Move south")},
                {new recKeyChord(KEY_WEST, false, false), (WestProc, "Left arrow - Move west")},
                {new recKeyChord(KEY_NORTH, false, false), (NorthProc, "Up arrow - Move north")},
                {new recKeyChord(KEY_EAST, false, false), (EastProc, "Right arrow - Move east")},
                {new recKeyChord(KEY_Q, false, false), (QuaffProc, "q - Quaff potion")},
                {new recKeyChord(KEY_R, false, false), (ReadProc, "r - Read scroll")},
                {new recKeyChord(KEY_S, false, false), (SearchProc, "s - Search for item")},
                {new recKeyChord(KEY_E, false, false), (EatProc, "e - Eat food")},
                {new recKeyChord(KEY_I, false, false), (DisplayInventory, "i - Show inventory")},
                {new recKeyChord(KEY_D, false, false), (DropProc, "d - Drop item")},
                {new recKeyChord(KEY_W, false, false), (WieldProc, "w - Wield a weapon")},
                {new recKeyChord(KEY_D, true, false), (DevModeProc, "CTRL-D - Dev Mode ON / OFF")},
                {new recKeyChord(KEY_N, true, false), (NewMapProc, "CTRL-N - Draw new map (Dev mode)")},
                {new recKeyChord(KEY_H, true, false), (HulkModeProc, "CTRL-H - Hulk mode (cheat)")},
            };

            //Searchable dictionary field to hold delegates for inventory items.
            InventoryActions = new Dictionary<(InvCategory InvCat, string InvName), Func<bool>>
            {
                {(InvCategory.Scroll, "Identify"), ScrollOfIdentifyBegin},
                {(InvCategory.Scroll, "Magic Mapping"), ScrollOfMagicMapping},
                {(InvCategory.Scroll, "Enchant Armor"), ScrollOfEnchantArmor},
                {(InvCategory.Scroll, "Enchant Weapon"), ScrollOfEnchantWeapon},
                {(InvCategory.Scroll, "Food Detection"), ScrollOfFoodDetection},
                {(InvCategory.Scroll, "Light"), ScrollOfLight},
                {(InvCategory.Scroll, "Confuse Monster"), ScrollOfConfuseMonsterBegin},
                {(InvCategory.Scroll, "Remove Curse"), ScrollOfRemoveCurse},
                {(InvCategory.Scroll, "Sleep"), ScrollOfSleep},
                {(InvCategory.Scroll, "Teleportation"), ScrollOfTeleportation},
                {(InvCategory.Scroll, "Aggravate Monsters"), ScrollOfAggravateMonsters},
                {(InvCategory.Scroll, "Create Monster"), ScrollOfCreateMonster},
                {(InvCategory.Scroll, "Gold Detection"), ScrollOfGoldDetection},
                {(InvCategory.Scroll, "Hold Monsters"), ScrollofHoldMonsters},
                {(InvCategory.Scroll, "Protect Armor"), ScrollOfProtectArmor},
                {(InvCategory.Scroll, "Clear Monsters"), ScrollOfClearMonsters},
                {(InvCategory.Scroll, "Blank Paper"), ScrollOfPaper}
            };
        }
        #endregion

        #region Procedures
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

                    if (CurrentPlayer.Immobile == 0) UpdateStatus("You can move again.", false);
                }
            } while (CurrentPlayer.Immobile > CurrentTurn);

            // End turn
            TurnInProgress = false;
        }
        /// <summary>
        /// Evaluate and adjust all player stats at end of turn.
        /// </summary>
        private void EvaluatePlayer()
        {
            if (GameMode != DisplayMode.GameOver)
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
                        UpdateStatus($"You are starting to feel {CurrentPlayer.HungerState.ToString().ToLower()}", false);
                    }
                }

                // If the player is FAINT, decide if they should faint on this move.
                if (CurrentPlayer.HungerState == Player.HungerLevel.Faint && CurrentPlayer.Immobile == 0)
                {
                    if (rand.Next(1, 101) < FAINT_PCT)
                    {
                        CurrentPlayer.Immobile = CurrentTurn + rand.Next(1, MAX_TURN_LOSS + 1);
                        UpdateStatus("You fainted from lack of food.", true);
                    }
                }
                // If the player is now dead, signal the game over.
                else if (CurrentPlayer.HungerState == Player.HungerLevel.Dead)
                {
                    GameMode = DisplayMode.GameOver;
                    CauseOfDeath = "starvation";
                }

                // Regenerate hit points.
                if (CurrentTurn % HEAL_RATE == 0 && CurrentPlayer.HPDamage > 0)
                    CurrentPlayer.HPDamage -= rand.Next(1, (int)(CurrentPlayer.ExpLevel / 3 + 1));

                if (CurrentPlayer.HPDamage < 0) CurrentPlayer.HPDamage = 0;

                // Check for experience level increase.
                if (CurrentPlayer.Experience >= CurrentPlayer.NextExpLevelUp)
                {
                    CurrentPlayer.NextExpLevelUp *= 2;
                    CurrentPlayer.ExpLevel += 1;
                    CurrentPlayer.MaxHP += rand.Next(1, HP_LEVEL_INCREASE + 1);

                    if (CurrentPlayer.HPDamage > 0)
                        CurrentPlayer.HPDamage -= rand.Next(1, CurrentPlayer.HPDamage);

                    UpdateStatus($"Welcome to Level {CurrentPlayer.ExpLevel}.", false);
                }

                // End any inventory effects if they haven't been used.
                if (CurrentPlayer.InventoryEffect != null)
                    if (CurrentPlayer.InventoryEffect?.EndingTurn <= CurrentTurn)
                        CurrentPlayer.InventoryEffect?.TargetFunction.Invoke();


                // Clear confusion, blindness
                if (CurrentPlayer.Confused > 0 && CurrentPlayer.Confused >= CurrentTurn)
                {
                    UpdateStatus("You feel less confused now.", false);
                    CurrentPlayer.Confused = 0;
                }

                if (CurrentPlayer.Blind > 0 && CurrentPlayer.Blind >= CurrentTurn)
                {
                    UpdateStatus("You can see again.", false);
                    CurrentPlayer.Blind = 0;
                }
            }
        }
        /// <summary>
        /// Change the current map level number.
        /// </summary>
        /// <param name="Change">Number of levels to move.</param>
        private void ChangeLevel(int Change)
        {
            bool allowPass = false;
            string failMessage = "";

            // Player can go down a level until the final level.
            // They can only go up if they have the Amulet.
            if (Change < 0)
            {
                allowPass = CurrentPlayer.HasAmulet && CurrentLevel > 1;
                failMessage = "You can't go that way.";
            }
            else if (Change > 0)
            {
                allowPass = CurrentLevel < MAX_LEVEL;
                failMessage = "You have reached the final level. You must find the Amulet and return to the surface.";
            }

            // Change the level or show the fail message.
            if (allowPass)
            {
                CurrentLevel += Change;
                CurrentMap = new MapLevel(CurrentLevel, CurrentPlayer, GameInventory);
                CurrentMap.ShroudMap();
                CurrentPlayer.Location = CurrentMap.GetOpenSpace(false);
                CurrentMap.DiscoverRoom(CurrentPlayer.Location.X, CurrentPlayer.Location.Y);
            }
            else
                UpdateStatus(failMessage, false);

        }
        /// <summary>
        /// Capitalize first letter of text passed in.
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string CapitalFirstLetter(string Text)
        {
            // Capitalize as needed.
            if (Text.Length == 0)
                return "";
            else if (Text.Length == 1)
                return Text.ToUpper();
            else
                return Text[0].ToString().ToUpper() + Text[1..];
        }
        /// <summary>
        /// Add 'a' or 'an' as appropriate
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string AddEnglishArticle(string Text)
        {
            // Add appropriate article - "a" or "an".
            if ("AEIOU".Contains(Text.Substring(0, 1)))
                return $"an {Text}";
            else
                return $"a {Text}";
        }

        /// <summary>
        /// When the player identifies a certain inventory type, mark it
        /// so that future items will be identified.
        /// </summary>
        /// <param name="PriorityID">PriorityID of selected item</param>
        public void SetInventoryAsIdentified(int PriorityID)
        {
            Inventory? template = GameInventory.InventoryItems.FirstOrDefault(x => x.PriorityId == PriorityID);

            // Set the inventory template as identified
            if (template != null) template.IsIdentified = true;

            // Set all instances in the player's inventory as identified.
            foreach (Inventory item in CurrentPlayer.PlayerInventory)
            {
                if (item.PriorityId == PriorityID)
                    item.IsIdentified = true;
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
                UpdateStatus("Searching ...", false);
                spaces = CurrentMap.GetSurrounding(CurrentPlayer.Location!.X, CurrentPlayer.Location.Y, 1);

                foreach (MapSpace space in spaces)
                {
                    if (space.SearchRequired)
                    {
                        space.SearchRequired = false;
                        space.AltMapCharacter = null;
                    }
                }
            }
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
            Inventory? invFound = null; Monster? monster = null;
            Dictionary<MapLevel.Direction, MapSpace> adjacent =
                CurrentMap.SearchAdjacent(player.Location!.X, player.Location.Y);

            // If player is confused, there's a chance of reversed movement.
            if (player.Confused > 0 && rand.Next(100) > DEGREE_CONFUSION)
                direct = CurrentMap.GetDirection180(direct);

            // Move character if possible.
            do
            {
                // Inspect target character
                if (adjacent.ContainsKey(direct))
                {
                    visibleCharacter = CurrentMap.PriorityChar(adjacent[direct], false).DisplayChar;
                    invFound = CurrentMap.DetectInventory(adjacent[direct]);
                    monster = CurrentMap.DetectMonster(adjacent[direct]);
                }
                else
                {
                    visibleCharacter = MapLevel.EMPTY.DisplayChar;
                    invFound = null;
                    monster = null;
                }

                // The player can move if the visible character is within a room or a hallway and there's no monster there.
                canMove = MapLevel.InhabitableSpacesGlyphList.Contains(visibleCharacter) ||
                    (invFound != null && monster == null);

                if (canMove)
                {
                    // Move the character.
                    player.Location = adjacent[direct];

                    // If this is a doorway, determine if the room is lighted.
                    if (player.Location.MapCharacter.DisplayChar == MapLevel.ROOM_DOOR.DisplayChar)
                        CurrentMap.DiscoverRoom(player.Location.X, player.Location.Y);

                    // Show the surrounding spaces if the player can see.
                    if (CurrentPlayer.Blind == 0)
                        CurrentMap.ShowSurrounding(player.Location.X, player.Location.Y);

                    // Discover the spaces surrounding the player and note if something is found.
                    stopMoving = CurrentMap.DetectObstruction(player.Location.X, player.Location.Y);

                    // Respond to items on map.
                    if (invFound != null) UpdateStatus(AddInventory(), false);

                    // Player turn completed.
                    turnComplete = true;
                }
                else if (monster != null)
                {                    
                    Attack(CurrentPlayer, monster);

                    // Player turn completed.
                    turnComplete = true;
                }

                // Complete turn if indicated.
                if (turnComplete) { CompleteTurn(); }

                // Determine if player can move automatically on FastPlay.  Three or more adjacent
                // hallway spaces indicate a junction which needs to stop FastPlay.
                adjacent = CurrentMap.SearchAdjacent(player.Location!.X, player.Location.Y);

            } while (!stopMoving && invFound == null && CanAutoMove(player.Location, adjacent[direct]));
        }

        /// <summary>
        /// Attack routine with calculations - player attacking
        /// </summary>
        /// <param name="Attacker">Player object as attacker</param>
        /// <param name="Defender">Monster object as defender</param>
        private void Attack(Player Attacker, Monster Defender)
        {
            int hitChance;
            bool hitSuccess;
            int damage = 0, minDamage = 1, maxDamage = 4;
            Inventory? weapon = CurrentPlayer.Wielding;

            // Set the monster as the current opponent.
            CurrentPlayer.Opponent = Defender;

            // Chance of landing a punch - 30% + (5% * XP level) - (5% * monster armor class).
            // Hulk mode can be used for "testing" - certain punch with immediate kill.
            hitChance = 50 + (5 * CurrentPlayer.ExpLevel) - (5 * Defender.ArmorClass);

            // If the player is confused, decrease the chance to 25%.
            if (Attacker.Confused > 0)
                hitChance = (int)(hitChance * 0.25);

            hitSuccess = HulkMode ? true : rand.Next(1, 101) <= hitChance;

            // Either way, if the monster wasn't angry before, it sure is now.
            Defender.CurrentState = Monster.Activity.Angered;

            // Get weapon damage rating, default to bare hands (1-4)
            if (weapon != null)
            {
                minDamage = weapon.MinDamage;
                maxDamage = weapon.MaxDamage;
            }

            // Random HP damage within weapon potential.
            if (hitSuccess)
            {
                UpdateStatus($"You hit the {Defender.MonsterName.ToLower()}.", false);
                damage = HulkMode ? Defender.MaxHP : rand.Next(minDamage, maxDamage + 1);

                if (CurrentPlayer.InventoryEffect != null)
                     CurrentPlayer.InventoryEffect?.TargetFunction.Invoke();
            }
            else UpdateStatus($"You missed the {Defender.MonsterName.ToLower()}.", false);

            Defender.HPDamage += damage;

            // If the monster has been defeated, remove it from the map and spawn another one.
            if (Defender.CurrentHP < 1)
            {
                CurrentPlayer.Opponent = null;
                CurrentMap.ActiveMonsters.Remove(Defender);
                UpdateStatus($"You defeated the {Defender.MonsterName.ToLower()}.", false);
                CurrentPlayer.Experience += Defender.ExpReward + (int)(Defender.MaxHP / 6);
                CurrentMap.AddMonsters(1);
            }
        }
        /// <summary>
        /// Attack routine with calculation - monster attacking
        /// </summary>
        /// <param name="Attacker">Monster object as attacker</param>
        /// <param name="Defender">Player object as defender</param>
        private void Attack(Monster Attacker, Player Defender)
        {
            int hitChance, armorRating, damage = 0;
            bool hitSuccess;
            Inventory? armor = CurrentPlayer.Armor;

            // Chance of landing a punch - 30% + (5% * monster min hit points)  - (5% * player armor class)
            // (protection rings will be factored in later)

            if (armor != null)
                armorRating = armor.ArmorClass + armor.Increment;
            else
                armorRating = 1;

            hitChance = 50 + (Attacker.MinStartingHP * 5) - (armorRating * 5);
            
            // If the monster is confused, decrease the chance to 25%.
            if (Attacker.Confused > 0)
                hitChance = (int)(hitChance * 0.25);

            hitSuccess = rand.Next(1, 101) <= hitChance;

            // Random HP between monster's min and max attack damage.
            if (hitSuccess)
            {
                UpdateStatus($"The {Attacker.MonsterName.ToLower()} hit you.", false);
                damage = rand.Next(Attacker.MinAttackDmg, Attacker.MaxAttackDmg + 1);
            }
            else UpdateStatus($"The {Attacker.MonsterName.ToLower()} missed you.", false);

            Defender.HPDamage += damage;

            // If the player has been defeated, end the game.
            if (Defender.CurrentHP < 1)
            {
                GameMode = DisplayMode.GameOver;
                UpdateStatus($"The {Attacker.MonsterName.ToLower()} killed you.", false);
                CauseOfDeath = (AddEnglishArticle(Attacker.MonsterName.ToLower()));
            }
        }
        /// <summary>
        /// Move the monster in response to player's move.
        /// </summary>
        /// <param name="monster">Monster object</param>
        public void MoveMonster(Monster monster)
        {
            char visibleCharacter;
            int tentativeDistance, playerDistance;
            bool timeToMove, canMove, wrongMove;
            MapLevel.Direction direct, direct90, direct270;
            MapLevel.Direction? playerDirection = null;
            MapSpace destinationSpace = monster.Location!;
            Inventory? foundInventory;

            // If the monster is confused, paralyzed, blind, decide if it's time to snap out of it.
            if (monster.Confused > 0 && monster.Confused <= CurrentTurn)
                monster.Confused = 0;

            if (monster.Immobile > 0 && monster.Immobile <= CurrentTurn)
                monster.Immobile = 0;

            if (monster.Blind > 0 && monster.Blind <= CurrentTurn)
                monster.Blind = 0;

            // Move monster if possible.
            timeToMove = (monster.CurrentState == Monster.Activity.Wandering && monster.Immobile == 0 &&
                rand.Next(1, 101) >= monster.Inertia) || monster.CurrentState == Monster.Activity.Angered;

            if (timeToMove)
            {
                // Get adjacent spacees.
                Dictionary<MapLevel.Direction, MapSpace> adjacent =
                    CurrentMap.SearchAdjacent(monster.Location!.X, monster.Location.Y);

                // If the player is in an adjacent space, get the direction.
                if (adjacent.ContainsValue(CurrentPlayer.Location!))
                {
                    playerDistance = 1;
                    playerDirection = adjacent.Where(p => p.Value == CurrentPlayer.Location!).FirstOrDefault().Key;
                }
                else
                {
                    // Get the current distance of the player from the monster.
                    playerDistance = Math.Abs(CurrentPlayer.Location!.X - monster.Location.X)
                        + Math.Abs(CurrentPlayer.Location.Y - monster.Location.Y);
                }

                // Move toward the player if they are close enough or if the monster is angry.
                if (playerDirection != null && (monster.CurrentState == Monster.Activity.Angered || monster.Aggressive))
                    monster.Direction = playerDirection;
                else if (playerDistance <= MAX_PURSUIT && monster.Aggressive)
                {
                    // If the player is within pursuit distance, search the adjacent spaces
                    // for one that's available and closest to the player.
                    foreach (KeyValuePair<MapLevel.Direction, MapSpace> adjSpace in adjacent)
                    {
                        if (MapLevel.InhabitableSpacesGlyphList.Contains(CurrentMap.PriorityChar(adjSpace.Value, false).DisplayChar) ||
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
                else if (playerDistance > MAX_PURSUIT)
                    // If the player is far off, just go to wandering.
                    monster.CurrentState = Monster.Activity.Wandering;

                // If the monster is still feeling aimless, just pick one except 'None'.
                if (monster.Direction == null)
                {
                    do
                    {
                        monster.Direction = (MapLevel.Direction)rand.Next(-2, 3);
                    } while (monster.Direction == MapLevel.Direction.None);
                }

                // Get relative directions to monster's choice. Chance to reverse movement if the monster is confused.
                wrongMove = (monster.Confused > 0 && rand.Next(100) > DEGREE_CONFUSION); 
                direct = (MapLevel.Direction)monster.Direction!;
                if(wrongMove) { direct = CurrentMap.GetDirection180(direct); }
                direct90 = CurrentMap.GetDirection90(direct);
                direct270 = CurrentMap.GetDirection270(direct);

                if (adjacent.ContainsKey(direct))
                {
                    // Inspect target character
                    visibleCharacter = CurrentMap.PriorityChar(adjacent[direct], false).DisplayChar;
                    foundInventory = CurrentMap.DetectInventory(adjacent[direct]);
                }
                else
                {
                    visibleCharacter = MapLevel.EMPTY.DisplayChar;
                    foundInventory = null;
                }

                // The monster can move if the visible character is within a room or a hallway
                // and there's nobody else there.
                canMove = MapLevel.InhabitableSpacesGlyphList.Contains(visibleCharacter) || foundInventory != null;

                if (canMove)
                    monster.Location = adjacent[direct];
                else
                {
                    if (CurrentMap.DetectMonster(adjacent[direct]) != null && monster.Aggressive)
                        // The monster just tried to run into another monster.  For now, just change direction.
                        // TODO:  This might need to result in an attack.
                        monster.Direction = rand.Next(1, 101) > 50 ? direct270 : direct90;
                    else if (adjacent[direct] == CurrentPlayer.Location)
                        // Attack the player
                        Attack(monster, CurrentPlayer);
                    else
                    {
                        // Change direction and decide on a current state.
                        monster.Direction = rand.Next(1, 101) > 50 ? direct270 : direct90;
                        if (rand.Next(1, 101) < monster.Inertia) monster.CurrentState = Monster.Activity.Resting;
                    }
                }
            }
            else
                // If the monster couldn't move, it might be resting. Decide if it should come out of nap time.
                if (rand.Next(1, 101) > monster.Inertia) monster.CurrentState = Monster.Activity.Wandering;
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
                & Target.MapCharacter.DisplayChar == Origin.MapCharacter.DisplayChar
                & MapLevel.InhabitableSpacesGlyphList.Contains(CurrentMap.PriorityChar(Target, false).DisplayChar)
                & CurrentMap.SearchAdjacent(MapLevel.HALLWAY.DisplayChar, Origin.X, Origin.Y).Count < 3;
        }

        #endregion

        #region Command Keys
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

            keyHandled = false;
            char lowerCase = char.ToLower((char)KeyVal);

            if (KeyVal == KEY_ESC)
            {
                ReturnFunction = null;
                RestoreMap();
            }

            switch (GameMode)
            {
                case DisplayMode.Inventory:
                    // For letters, call the current return function.
                    if (lowerCase >= 'a' && lowerCase <= 'z')
                    {
                        if (ReturnFunction != null) 
                            ReturnFunction(lowerCase);
                    }
                    keyHandled = true;
                    break;
                case DisplayMode.GameOver:
                    RIPScreen();
                    break;
                default:
                    break;
            }

            if (!keyHandled)
            {
                // Shift, Ctrl and Basic combinations
                if (GameMode == DisplayMode.Primary)
                {
                    if (KeyActions.TryGetValue(new recKeyChord(KeyVal, Control, Shift), out var taskInfo))
                        taskInfo.method.Invoke();

                    keyHandled = true;
                }
            }

            // Complete turn if one was started.
            if (TurnInProgress) CompleteTurn();
            
            // Display the appropriate map mode.
            if (GameMode == DisplayMode.Primary)
                if (DevMode) 
                    { this.CurrentMap.MapCheck(); } 
                else 
                    { this.CurrentMap.MapText(); }            

        }

        #region KeyProcs

        private void WieldProc()
        {
            // Wield a weapon
            Wield(null);
        }

        private void DropProc()
        {
            // Drop an inventory item
            DropInventory(null);
        }

        private void EatProc()
        {
            // Eat something
            TurnInProgress = true;
            Eat(null);
        }

        private void SearchProc()
        {
            // Search for hidden items
            TurnInProgress = true;
            SearchForHidden();
        }

        private void ReadProc()
        {
            // Read scroll
            TurnInProgress = true;
            ReadScroll(null);
        }

        private void QuaffProc()
        {
            // Quaff potion
            TurnInProgress = true;
            QuaffPotion(null);
        }

        private void WestProc()
        {
            // Move player west
            MovePlayer(CurrentPlayer, MapLevel.Direction.West);
        }

        private void NorthProc()
        {
            // Move player north
            MovePlayer(CurrentPlayer, MapLevel.Direction.North);
        }

        private void EastProc()
        {
            // Move player east
            MovePlayer(CurrentPlayer, MapLevel.Direction.East);
        }

        private void SouthProc()
        {
            // Move player south
            MovePlayer(CurrentPlayer, MapLevel.Direction.South);
        }
        /// <summary>
        /// Allows the dev to cycle through many maps for testing
        /// </summary>
        private void NewMapProc()
        {
            // Show new map if in Dev mode.
            if (DevMode)
                ReplaceMap();
        }
        /// <summary>
        /// Hulk mode is a cheat code that lets the player quickly kill monsters.
        /// </summary>
        private void HulkModeProc()
        {
            // Enable / disable hulk mode.
            // Monsters killed with a single punch.
            HulkMode = !HulkMode;
            UpdateStatus(HulkMode ? "Hulk Mode ON" : "Hulk Mode OFF", false);

        }
        /// <summary>
        /// Turn Dev Mode ON / OFF
        /// </summary>
        private void DevModeProc()
        {
            // Toggle Dev mode
            DevMode = !DevMode;
            UpdateStatus(DevMode ? "Developer Mode ON" : "Developer Mode OFF", false);
        }

        private void WearArmorProc()
        {
            // Wear armor
            TurnInProgress = true;
            WearArmor(null);
        }
        private void RemoveArmorProc()
        {
            // Take off armor
            TurnInProgress = true;
            RemoveArmor();
        }
        private void HelpProc()
        {
            // Display help screen.
            GameMode = DisplayMode.Help;
            HelpScreen();
        }
        /// <summary>
        /// Go up staircase if possible.
        /// </summary>
        private void UpstairsProc()
        {           
            TurnInProgress = true;
            if (CurrentPlayer.Location!.MapCharacter.DisplayChar == MapLevel.STAIRWAY.DisplayChar)
                ChangeLevel(-1);
            else
                UpdateStatus("There's no stairway here.", false);
        }
        /// <summary>
        /// Go down staircase if possible. 
        /// </summary>
        private void DownStairsProc()
        {            
            TurnInProgress = true;
            if (CurrentPlayer.Location!.MapCharacter.DisplayChar == MapLevel.STAIRWAY.DisplayChar)
                ChangeLevel(1);
            else
                UpdateStatus("There's no stairway here.", false);
        }
        /// <summary>
        /// Turn Fast Play ON / OFF, enables player to continuously move until an obstruction.
        /// </summary>
        private void FastPlayProc()
        {
            // Fast Play Toggle
            FastPlay = !FastPlay;
            UpdateStatus(FastPlay ? "Fast Play mode ON." : "Fast Play mode OFF", false);
        }
        #endregion
        /// <summary>
        /// Wield a specific weapon.
        /// </summary>
        /// <param name="ListItem">Menu character of chosen item</param>
        /// <returns></returns>
        private bool Wield(char? ListItem)
        {
            bool retValue = false;
            List<Inventory> items;

            if (GameMode != DisplayMode.Inventory)
            {
                // Verify the player has something they can wield.
                items = (from inv in CurrentPlayer.PlayerInventory
                         where inv.ItemCategory == InvCategory.Weapon ||
                         inv.ItemCategory == InvCategory.Ammunition
                         select inv).ToList();

                if (items.Count > 0)
                {
                    // If there's a weapon, show the inventory
                    // and let the player select it.  Set to return and exit.
                    DisplayInventory();
                    UpdateStatus("Please select an item to wield.", false);
                    ReturnFunction = Wield;
                }
                else
                    // Otherwise, their hand-to-hand skills better be good.
                    UpdateStatus("You don't have anything that can be used as a weapon.", false);
            }
            else
            {
                // Get the selected item.
                items = (from InventoryLine in GameInventory.InventoryDisplay(CurrentPlayer.PlayerInventory)
                         where InventoryLine.ID == ListItem
                         select InventoryLine.InvItem).ToList();

                if (items.Count > 0)
                {
                    // Call the appropriate delegate and remove the item
                    // from inventory.
                    if (items[0].ItemCategory != InvCategory.Weapon &&
                        items[0].ItemCategory != InvCategory.Ammunition)
                    {
                        UpdateStatus($"You should reconsider. {CapitalFirstLetter(items[0].RealName)} is not an effective weapon.", false);
                        retValue = false;
                    }
                    else
                    {
                        CurrentPlayer.Wielding = items[0];
                        RestoreMap();

                        if (items[0].IsGroupable)
                            UpdateStatus($"You are now wielding some {items[0].PluralName}.", false);
                        else
                            UpdateStatus($"You are now wielding {AddEnglishArticle(items[0].PluralName)}.", false);

                        retValue = true;
                    }
                }
                else
                {
                    // Process non-existent option.
                    UpdateStatus("Please select something to wield.", false);
                    RestoreMap();
                    retValue = false;
                }

                ReturnFunction = null;

            }

            return retValue;
        }

        /// <summary>
        /// Remove current armor from player.
        /// </summary>
        private void RemoveArmor()
        {
            string status = "";
            Inventory? armor = CurrentPlayer.Armor;

            if (armor != null && !armor.IsCursed)
            {
                CurrentPlayer.Armor = null;
                status = $"You removed {armor.RealName}";
            }
            else if (armor != null && armor.IsCursed)
                status = "You try to remove the armor but it's cursed.";
            else
                status = "You aren't wearing any armor.";

            UpdateStatus(status, false);
        }

        /// <summary>
        /// Wear specified armor.
        /// </summary>
        /// <param name="ListItem">Menu character of chosen item</param>
        /// <returns>True / False indicating if item was sucessfuly worn</returns>
        private bool WearArmor(char? ListItem)
        {
            bool retValue = false;
            List<Inventory> items;

            if (GameMode != DisplayMode.Inventory)
            {
                if (CurrentPlayer.Armor != null)
                    UpdateStatus("You are already wearing armor. You must take it off first.", false);
                else
                {
                    // Verify the player has armor in inventory.
                    items = (from inv in CurrentPlayer.PlayerInventory
                             where inv.ItemCategory == InvCategory.Armor
                             select inv).ToList();

                    if (items.Count > 0)
                    {
                        // If there's armor, show the inventory
                        // and let the player select it.  Set to return and exit.
                        DisplayInventory();
                        UpdateStatus("Please select an armor to wear.", false);
                        ReturnFunction = WearArmor;
                    }
                    else
                        // Otherwise, they're stuck with whatever they have.
                        UpdateStatus("You don't have any armor in inventory.", false);
                }
            }
            else
            {
                // Get the selected item.
                items = (from InventoryLine in GameInventory.InventoryDisplay(CurrentPlayer.PlayerInventory)
                         where InventoryLine.ID == ListItem
                         select InventoryLine.InvItem).ToList();

                if (items.Count > 0)
                {
                    if (items[0].ItemCategory != InvCategory.Armor)
                    {
                        UpdateStatus("You can't wear that.", false);
                        retValue = false;
                    }
                    else
                    {
                        // If the player selects a valid item, add it as their armor and decide if it's cursed.
                        CurrentPlayer.Armor = items[0];
                        CurrentPlayer.Armor.IsCursed = rand.Next(1, 101) <= ITEM_CURSE_PROB ? true : false;
                        UpdateStatus($"You are now wearing {items[0].RealName}.", false);
                        retValue = true;
                    }
                }
                else
                {
                    // Process non-existent option.
                    UpdateStatus("Please select some armor to wear.", false);
                    retValue = false;
                }

                ReturnFunction = null;
            }

            if (ReturnFunction == null) RestoreMap();

            return retValue;
        }

        /// <summary>
        /// Eat specified food.
        /// </summary>
        /// <param name="ListItem">Menu character of chosen item</param>
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
                         where inv.ItemCategory == InvCategory.Food
                         select inv).ToList();

                if (items.Count > 0)
                {
                    // If there's something edible, show the inventory
                    // and let the player select it.  Set to return and exit.
                    DisplayInventory();
                    UpdateStatus("Please select something to eat.", false);
                    ReturnFunction = Eat;
                }
                else
                    // Otherwise, they'll be hungry for awhile.
                    UpdateStatus("You don't have anything to eat.", false);
            }
            else
            {
                // Get the selected item.
                items = (from InventoryLine in GameInventory.InventoryDisplay(CurrentPlayer.PlayerInventory)
                         where InventoryLine.ID == ListItem
                         select InventoryLine.InvItem).ToList();

                if (items.Count > 0)
                {
                    // Call the appropriate delegate and remove the item
                    // from inventory.
                    // TODO: In this case, it makes more sense to complete this here than in a delegate function. Continue to evaluate as other inventory is implemented.
                    if (items[0].ItemCategory != InvCategory.Food)
                    {
                        UpdateStatus("You can't eat THAT!", false);
                        retValue = false;
                    }
                    else
                    {
                        foodValue = rand.Next(MIN_FOODVALUE, MAX_FOODVALUE + 1);
                        CurrentPlayer.HungerTurn += foodValue;
                        CurrentPlayer.HungerState = Player.HungerLevel.Satisfied;
                        CurrentPlayer.PlayerInventory.Remove(items[0]);
                        UpdateStatus("Mmmm, that hit the spot.", false);
                        // Reward a strength point if needed.
                        if (CurrentPlayer.StrengthMod > 0) CurrentPlayer.StrengthMod--;
                        retValue = true;
                    }
                }
                else
                {
                    // Process non-existent option.
                    UpdateStatus("Please select something to eat.", false);
                    retValue = false;
                }

                ReturnFunction = null;
            }

            if (ReturnFunction == null) RestoreMap();
            return retValue;
        }


        /// <summary>
        /// Drop specified inventory on map.
        /// </summary>
        /// <param name="ListItem">Menu character of chosen item</param>
        /// <returns>True / False indicating success</returns>
        private bool DropInventory(char? ListItem)
        {
            bool retValue = false;
            List<InventoryLine> items;

            if (GameMode != DisplayMode.Inventory)
            {
                DisplayInventory();
                UpdateStatus("Please select an item to drop.", false);
                ReturnFunction = DropInventory;
            }
            else
            {
                items = (from InventoryLine in GameInventory.InventoryDisplay(CurrentPlayer.PlayerInventory)
                         where InventoryLine.ID == ListItem
                         select InventoryLine).ToList();

                if (items.Count > 0)
                {
                    if (CurrentMap.DetectInventory(CurrentPlayer.Location!) == null)
                    {
                        if (items[0].InvItem.ItemCategory == InvCategory.Ammunition
                            && items[0].InvItem.IsGroupable)
                        {
                            // We're dropping the entire batch so update the amount.
                            items[0].InvItem.Amount = items[0].Count;
                            // For ammunition, remove all items from the slot.
                            CurrentPlayer.PlayerInventory =
                                CurrentPlayer.PlayerInventory.Where(x => x.RealName != items[0].InvItem.RealName).ToList();

                            UpdateStatus($"You dropped {GameInventory.ListingDescription(items[0].Count, items[0].InvItem)}.", false);
                        }
                        else
                        {
                            items[0].InvItem.Amount = 1;
                            CurrentPlayer.PlayerInventory.Remove(items[0].InvItem);
                            UpdateStatus($"You dropped {GameInventory.ListingDescription(1, items[0].InvItem)}.", false);
                        }

                        items[0].InvItem.Location = CurrentPlayer.Location;
                        CurrentMap.MapInventory.Add(items[0].InvItem);
                        retValue = true;
                    }
                    else
                    {
                        UpdateStatus("There is already an item there.", false);
                        retValue = false;
                    }
                }
                else
                {
                    UpdateStatus("Please select an inventory item to drop.", false);
                    retValue = false;
                }

                ReturnFunction = null;
            }

            if (ReturnFunction == null) RestoreMap();

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
                if (foundItem.ItemCategory == InvCategory.Gold)
                {
                    // Add the gold at the current location to the player's purse and remove
                    // it from the map.
                    int goldAmt = rand.Next(MapLevel.MIN_GOLD_AMT, MapLevel.MAX_GOLD_AMT + 1);
                    CurrentPlayer.Gold += goldAmt;
                    CurrentMap.MapInventory.Remove(foundItem);
                    retValue = $"You picked up {goldAmt} pieces of gold.";
                }
                else
                {
                    // Determine if there's room in inventory for the item.
                    // If it's groupable and the player already has it in a slot, add it.
                    // Otherwise, if there's an extra slot available, add it.
                    addToInventory = (foundItem.IsGroupable && CurrentPlayer.SearchInventory(foundItem.RealName) != null);
                    if (!addToInventory) addToInventory =
                            GameInventory.InventoryDisplay(CurrentPlayer.PlayerInventory).Count + 1 <= Player.INVENTORY_LIMIT;

                    // If the additional inventory fits within the limit, keep the item.
                    // Otherwise, remove it.                
                    if (addToInventory)
                    {
                        // When the item is actually added, it needs to be a single item.
                        itemAmount = foundItem.Amount;
                        foundItem.Amount = 1;
                        // Move the item to the player's inventory.
                        for (int i = 1; i <= itemAmount; i++)
                            CurrentPlayer.PlayerInventory.Add(GameInventory.GetInventoryItem(foundItem.RealName)!);

                        retValue = $"You picked up {GameInventory.ListingDescription(itemAmount, foundItem)}.";
                        CurrentMap.MapInventory.Remove(foundItem);

                        if (foundItem.ItemCategory == InvCategory.Amulet)
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
        /// <summary>
        /// Read a selected scroll item.
        /// </summary>
        /// <param name="ListItem">Menu character of chosen item</param>
        /// <returns></returns>
        private bool ReadScroll(char? ListItem)
        {
            bool retValue = false, readScroll = false;
            List<Inventory> items;

            if (GameMode != DisplayMode.Inventory)
            {
                // Verify the player has something they can read.
                items = (from inv in CurrentPlayer.PlayerInventory
                         where inv.ItemCategory == InvCategory.Scroll
                         select inv).ToList();

                if (items.Count > 0)
                {
                    // If there are any scrolls, show the inventory
                    // and let the player select it.  Set to return and exit.
                    DisplayInventory();
                    UpdateStatus("Please select an item to read.", false);
                    ReturnFunction = ReadScroll;
                }
                else
                    // Otherwise, notify the player.
                    UpdateStatus("You don't have any scrolls.", false);
            }
            else
            {
                // Get the selected item.
                items = (from InventoryLine in GameInventory.InventoryDisplay(CurrentPlayer.PlayerInventory)
                         where InventoryLine.ID == ListItem
                         select InventoryLine.InvItem).ToList();

                if (items.Count > 0)
                {
                    // Call the appropriate delegate and remove the item
                    // from inventory.
                    if (items[0].ItemCategory != InvCategory.Scroll)
                    {
                        UpdateStatus("There's nothing on it to read.", false);
                        retValue = false;
                    }
                    else
                    {
                        // Set the inventory item as identified if necessary.
                        if (!items[0].IsIdentified) SetInventoryAsIdentified(items[0].PriorityId);

                        // Find and invoke the delegate
                        if (InventoryActions.TryGetValue((Inventory.InvCategory.Scroll, items[0].RealName), out var taskInfo))
                        {
                            // Remove the item from the player's inventory and invoke delegate.
                            CurrentPlayer.PlayerInventory.Remove(items[0]);
                            ReturnFunction = null;
                            readScroll = taskInfo.Invoke();
                        }

                        retValue = readScroll;
                    }

                }
                else
                {
                    // Process non-existent option.
                    UpdateStatus("Please select something to read.", false);
                    
                    ReturnFunction = null;
                    retValue = false;
                }

                if(ReturnFunction == null) RestoreMap();
            }

            return retValue;
        }
        /// <summary>
        /// Quaff the selected potion.
        /// </summary>
        /// <param name="ListItem">Menu character of chosen item</param>
        /// <returns></returns>
        private bool QuaffPotion(char? ListItem)
        {
            bool retValue = false, quaffPotion = false;
            List<Inventory> items;

            if (GameMode != DisplayMode.Inventory)
            {
                // Verify the player has something they can drink.
                items = (from inv in CurrentPlayer.PlayerInventory
                         where inv.ItemCategory == InvCategory.Potion
                         select inv).ToList();

                if (items.Count > 0)
                {
                    // If there are any potions, show the inventory
                    // and let the player select it.  Set to return and exit.
                    DisplayInventory();
                    UpdateStatus("Please select a potion to drink.", false);
                    ReturnFunction = QuaffPotion;
                }
                else
                    // Otherwise, notify the player.
                    UpdateStatus("You don't have anything to drink.", false);
            }
            else
            {
                // Get the selected item.
                items = (from InventoryLine in GameInventory.InventoryDisplay(CurrentPlayer.PlayerInventory)
                         where InventoryLine.ID == ListItem
                         select InventoryLine.InvItem).ToList();

                if (items.Count > 0)
                {
                    // Call the appropriate delegate and remove the item
                    // from inventory.
                    if (items[0].ItemCategory != InvCategory.Potion)
                    {
                        UpdateStatus("You can't drink that.", false);
                        retValue = false;
                    }
                    else
                    {
                        // Set the inventory item as identified if necessary.
                        if (!items[0].IsIdentified) SetInventoryAsIdentified(items[0].PriorityId);

                        // Find and invoke the delegate
                        if (InventoryActions.TryGetValue((Inventory.InvCategory.Potion, items[0].RealName), out var taskInfo))
                        {
                            // Remove the item from the player's inventory and invoke delegate.
                            CurrentPlayer.PlayerInventory.Remove(items[0]);
                            quaffPotion = taskInfo.Invoke();
                        }

                        retValue = quaffPotion;
                    }
                }
                else
                {
                    // Process non-existent option.
                    UpdateStatus("Please select something to drink.", false);
                    ReturnFunction = null;
                    retValue = false;
                }

                if (ReturnFunction == null) RestoreMap();
            }

            return retValue;
        }

        #endregion

        #region Scroll Methods

        public bool ScrollOfIdentifyBegin()
        {
            bool retValue = false;
            UpdateStatus("This is a Scroll of Identify. Please select an item to identify.", false);
            DisplayInventory();
            ReturnFunction = ScrollOfIdentifyEnd;
            retValue = true;

            return retValue;
        }

        private bool ScrollOfIdentifyEnd(char? ListItem)
        {
            bool retValue = false;
            List<InventoryLine> lines;
            // Get the selected item.
            lines = (from InventoryLine in GameInventory.InventoryDisplay(CurrentPlayer.PlayerInventory)
                     where InventoryLine.ID == ListItem
                     select InventoryLine).ToList();

            if (lines.Count > 0)
            {
                // Update inventory template to Identified and then update player's inventory.
                SetInventoryAsIdentified(lines[0].InvItem.PriorityId);
                UpdateStatus(GameInventory.ListingDescription(lines[0].Count, lines[0].InvItem), false);
            }
            else
            {
                // Process non-existent option.
                UpdateStatus("That item doesn't exist.", false);
            }

            ReturnFunction = null;            

            retValue = true;

            RestoreMap();

            return retValue;

        }

        private bool ScrollOfMagicMapping()
        {
            // Reveal entire map
            UpdateStatus("This scroll has a map on it!", false);
            CurrentMap.DiscoverMap();
            
            return true;
        }

        private bool ScrollOfEnchantArmor()
        {
            // Raise the player's current armor by one level and remove any curse.
            if(CurrentPlayer.Armor != null) {
                UpdateStatus($"Your armor's rating has been upgraded to {CurrentPlayer.Armor.ArmorClass + ++CurrentPlayer.Armor.Increment}.", false);
                CurrentPlayer.Armor.IsCursed = false;
            }
            else
                UpdateStatus($"This is a scroll of enchant armor. Alas, you aren't wearing any.", false);

            return true;
        }

        private bool ScrollOfEnchantWeapon()
        {
            // Increase the damage for the player's current weapon.
            if (CurrentPlayer.Wielding != null)
            {
                CurrentPlayer.Wielding.DmgIncrement++;
                CurrentPlayer.Wielding.IsCursed = false;
                UpdateStatus($"Your {CurrentPlayer.Wielding.RealName} gives off a bright flash of light.", false);
            }
            else
                UpdateStatus($"This is a scroll of enchant weapon. Too bad you aren't wielding one.", false);
            
            return true;
        }

        private bool ScrollOfFoodDetection()
        {
            bool retValue = false;
            // Reveal all the food on the map.
            retValue = CurrentMap.DiscoverInventoryByCat(InvCategory.Food);
            
            if (retValue) 
                UpdateStatus("Your nose tingles as you smell food nearby.", false);
            else
                UpdateStatus("You hear a growling noise very close to you.", false);

            return retValue;
        }

        private bool ScrollOfGoldDetection()
        {
            bool retValue = false;
            // Reveal all the gold on the map.
            retValue = CurrentMap.DiscoverInventoryByCat(InvCategory.Gold);

            if (retValue)
                UpdateStatus("You hear the jingle of coins somewhere on this level.", false);
            else
                UpdateStatus("'Check out the Dungeon,' they said. 'There's PLENTY of gold down there!' they said.", false);

            return retValue;
        }

        private bool ScrollOfLight()
        {
            if (CurrentPlayer.Location != null)
            {
                // Reveal the current room.
                CurrentMap.LightUpRoom(CurrentPlayer.Location.X, CurrentPlayer.Location.Y);
                UpdateStatus("The entire room is lit by an unearthly glow.", false);
            }

            return true;
        }

        private bool ScrollOfConfuseMonsterBegin()
        {
            //TODO: Review these values for possible new constants depending on other inventory effect ranges.
            // Activate the player's ability to confuse the next monster hit.
            int turns = rand.Next(100, 150);
            CurrentPlayer.InventoryEffect = (CurrentTurn + turns, ScrollOfConfuseMonsterEnd);            
            UpdateStatus("Your hands begin to glow red.", false);

            return true;
        }

        private bool ScrollOfConfuseMonsterEnd()
        {
            // Confuse the next monster the player hits for a random number of turns.
            if(CurrentPlayer.Opponent != null)
            { 
                CurrentPlayer.Opponent.Confused = CurrentTurn + rand.Next(2, 7);
                UpdateStatus($"The {CurrentPlayer.Opponent.MonsterName.ToLower()} appears confused.", false);
            }

            CurrentPlayer.InventoryEffect = null;
            UpdateStatus("Your hands stop glowing red.", false);

            return true;
        }

        private bool ScrollOfRemoveCurse()
        {
            // Remove any curses on weapons and armor in use.

            if(CurrentPlayer.Armor != null)
                CurrentPlayer.Armor.IsCursed = false;

            if (CurrentPlayer.Wielding != null)
                CurrentPlayer.Wielding.IsCursed = false;

            UpdateStatus("You suddenly feel someone watching over you.", false);

            return true;
        }

        private bool ScrollOfSleep()
        {
            // Put the player to sleep for a few turns.
            CurrentPlayer.Immobile = CurrentTurn + rand.Next(2, 5);
            UpdateStatus("You fall asleep.", false);

            return true;
        }

        private bool ScrollOfTeleportation()
        {
            // Move the player to a random spot on the map and confuse them
            // for a few moves.
            CurrentPlayer.Location = CurrentMap.GetOpenSpace(true);
            UpdateStatus("This is a scroll of teleportation!", false);

            CurrentPlayer.Confused = CurrentTurn + rand.Next(3, 10);
            UpdateStatus("You feel rather disoriented ...", false);

            return true;
        }

        private bool ScrollOfAggravateMonsters()
        {
            // Make every monster on the map aggressive.

            foreach (Monster monster in (from Monster in CurrentMap.ActiveMonsters 
                                         select Monster))
                monster.Aggressive = true;
            
            UpdateStatus("The scroll emits a high pitched whistling noise.", false);
            UpdateStatus("From every direction, you hear howls of outrage.", false);

            return true;
        }

        private bool ScrollOfCreateMonster()
        {
            List<MapSpace> spaces = CurrentMap.GetSurrounding(CurrentPlayer.Location!.X, CurrentPlayer.Location.Y, 2);
            // Create a new monster near the player.
            CurrentMap.AddMonsters(1, spaces);
            UpdateStatus("The room suddenly got a bit more crowded.", false);

            return true;
        }

        private bool ScrollofHoldMonsters()
        {
            List<MapSpace> surrounding = CurrentMap.GetSurrounding(CurrentPlayer.Location!.X, CurrentPlayer.Location.Y, 2);

            // Make every monster within two paces immobile for up to 25 turns.
            List<Monster> monsters = 
                CurrentMap.ActiveMonsters.Where(monster => surrounding
                .Any(space => space.X == monster.Location!.X && space.Y == monster.Location.Y))
                .ToList();

            foreach (Monster monster in monsters)
                monster.Immobile = CurrentTurn + rand.Next(25);

            UpdateStatus("The monsters around you suddenly freeze in their tracks. A fast and quiet exit would be wise at this point.", false);

            return true;

        }

        private bool ScrollOfProtectArmor()
        {
            // Set the player's current armor as protected.
            if (CurrentPlayer.Armor != null)
            {
                CurrentPlayer.Armor.IsProtected = true;
                CurrentPlayer.Armor.IsCursed = false;
                UpdateStatus($"Great news! Your {CurrentPlayer.Armor.RealName} is protected against damage, theft and arcane curses up to 1,000,000 gold.", false);
                UpdateStatus($"At Dungeon Insurance, your safety is our first concern!", false);
            }
            else
                UpdateStatus($"Hello, {CurrentPlayer.PlayerName}. We've been trying to reach you about your extended armor insurance.", false);

            return true;

        }

        private bool ScrollOfClearMonsters()
        {
            Inventory invItem;

            // Transfer monsters gold and inventory back to map.
            foreach (Monster monster in CurrentMap.ActiveMonsters)
            {
                if (monster.Gold > 0)
                {
                    invItem = GameInventory.GetInventoryItem(InvCategory.Gold, CurrentMap.GetOpenSpace(true)!);
                    CurrentMap.AddInventory(invItem, invItem.Location, false);
                }

                foreach(Inventory item in monster.MonsterInventory)
                {
                    invItem = GameInventory.GetInventoryItem(item.RealName)!;
                    CurrentMap.AddInventory(item, CurrentMap.GetOpenSpace(true)!, true);
                }
                    
            }

            // Clear the monsters off the current level map.
            CurrentMap.ActiveMonsters.Clear();
            UpdateStatus($"Somewhere near, you hear a disembodied voice whisper 'No more monsters ...'", false);
            UpdateStatus($"You feel a chill in your bones as these rooms suddenly go quiet.", false);

            return true;

        }

        private bool ScrollOfPaper()
        {
            // Blank scroll
            UpdateStatus($"The scroll's parchment has a rich and elegant feel to it but is otherwise blank.", false);

            return true;

        }

        #endregion

    }
}
