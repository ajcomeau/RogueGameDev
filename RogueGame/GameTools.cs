using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace RogueGame
{
    public static class GameTools
    {
        #region MapConstants
        /// <summary>
        /// Horizontal wall piece
        /// </summary>
        public static readonly MapGlyph HORIZONTAL = new MapGlyph('═', Color.SaddleBrown, Color.Black);      // Unicode symbols can be copy-pasted from https://www.w3.org/TR/xml-entity-names/025.html.
        /// <summary>
        /// Vertical wall piece.
        /// </summary>
        public static readonly MapGlyph VERTICAL = new MapGlyph('║', Color.SaddleBrown, Color.Black);
        /// <summary>
        /// Northwest room corner
        /// </summary>
        public static readonly MapGlyph CORNER_NW = new MapGlyph('╔', Color.SaddleBrown, Color.Black);
        /// <summary>
        /// Southeast room corner
        /// </summary>
        public static readonly MapGlyph CORNER_SE = new MapGlyph('╝', Color.SaddleBrown, Color.Black);
        /// <summary>
        /// Northeast room corner
        /// </summary>
        public static readonly MapGlyph CORNER_NE = new MapGlyph('╗', Color.SaddleBrown, Color.Black);
        /// <summary>
        /// Southwest room corner
        /// </summary>
        public static readonly MapGlyph CORNER_SW = new MapGlyph('╚', Color.SaddleBrown, Color.Black);
        /// <summary>
        /// Room interior space
        /// </summary>
        public static readonly MapGlyph ROOM_INT = new MapGlyph('·', Color.Gray, Color.Black);
        /// <summary>
        /// Room door piece
        /// </summary>
        public static readonly MapGlyph ROOM_DOOR = new MapGlyph('╬', Color.SaddleBrown, Color.Black);
        /// <summary>
        /// Hallway space
        /// </summary>
        public static readonly MapGlyph HALLWAY = new MapGlyph('▒', Color.White, Color.Black);
        /// <summary>
        /// Stairway symbol
        /// </summary>
        public static readonly MapGlyph STAIRWAY = new MapGlyph('≣', Color.Black, Color.Green);
        /// <summary>
        /// Trap symbol
        /// </summary>
        public static readonly MapGlyph TRAP = new MapGlyph('⬥', Color.Brown, Color.Black);
        /// <summary>
        /// Gold map symbol
        /// </summary>
        public static readonly MapGlyph GOLD = new MapGlyph('*', Color.LightYellow, Color.Black);
        /// <summary>
        /// Amulet of Yendor symbol
        /// </summary>
        public static readonly MapGlyph AMULET = new MapGlyph('♀', Color.Yellow, Color.Black);
        /// <summary>
        /// Empty map space
        /// </summary>
        public static readonly MapGlyph EMPTY = new MapGlyph(' ', Color.Black, Color.Black);
        #endregion

        #region GameplayConstants

        // Etc.
        /// <summary>
        /// Number of turns between each health regen.
        /// </summary>
        public const int HEAL_RATE = 12;
        /// <summary>
        /// Maximum HP to add with each exp. level.
        /// </summary>
        public const int HP_LEVEL_INCREASE = 10;
        /// <summary>
        /// Probability of search revealing hidden doors, etc..
        /// </summary>
        public const int SEARCH_PCT = 20;
        /// <summary>
        /// Maximum dungeon level
        /// </summary>
        public const int MAX_LEVEL = 26;
        /// <summary>
        /// Probability of fainting at any given point when FAIN
        /// </summary>
        public const int FAINT_PCT = 33;
        /// <summary>
        /// Maximum turns to lose when fainting, etc..
        /// </summary>
        public const int MAX_TURN_LOSS = 5;
        /// <summary>
        /// Max number of spaces for monster to detect and pursue player.
        /// </summary>
        public const int MAX_PURSUIT = 7;
        /// <summary>
        /// Probability that wearables will be cursed.
        /// </summary>
        public const int ITEM_CURSE_PROB = 15;
        /// <summary>
        /// Max gold amount per stash.
        /// </summary>
        public const int MIN_GOLD_AMT = 10;
        /// <summary>
        /// Max gold amount per stash.
        /// </summary>
        public const int MAX_GOLD_AMT = 125;
        /// <summary>
        /// Probability of a monster appearing at any given point.
        /// </summary>
        public const int SPAWN_MONSTER = 90;
        /// <summary>
        /// Probability that a room will have gold.
        /// </summary>
        public const int ROOM_GOLD_PCT = 51;
        /// <summary>
        /// Maximum inventory on a level.
        /// </summary>
        public const int MAX_INVENTORY = 20;
        /// <summary>
        /// Minimum number of initial monsters on a level.
        /// </summary>
        public const int MIN_INIT_MONSTERS = 5;
        /// <summary>
        /// Probability of a trap being placed on the level.
        /// </summary>
        public const int TRAP_PCT = 25;
        /// <summary>
        /// Maximum number of initial monsters on a level.
        /// </summary>
        public const int MAX_INIT_MONSTERS = 15;
        /// <summary>
        /// Maximum turns gained from food ration.
        /// </summary>
        public const int MAX_FOODVALUE = 1700;
        /// <summary>
        /// Minimum turns gained from food ration.
        /// </summary>
        public const int MIN_FOODVALUE = 900;
        /// <summary>
        /// Maximum items in a batch of arrows or bolts
        /// </summary>
        public const int MAX_AMMO_BATCH = 15;
        /// <summary>
        /// Starting hit points
        /// </summary>
        public const int PLAYER_START_HP = 12;
        /// <summary>
        /// Starting strength points
        /// </summary>
        public const int PLAYER_START_STRENGTH = 16;
        /// <summary>
        /// Turns between hunger states
        /// </summary>
        public const int HUNGER_TURNS = 150;
        /// <summary>
        /// GameTools coin toss constant to be used when a little chaos is needed.
        /// </summary>
        public const int COIN_FLIP = 50;

        #endregion

        #region Functions
        public static Random rand = new Random();

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
        public static int MovesAllowed(decimal CharacterSpeed)
        {
            int movesAllowed = (int)CharacterSpeed;
            CharacterSpeed -= movesAllowed;

            // Calculate number of movesCollect based on character speed and probability.

            movesAllowed = (rand.Next(1, 101) <= 100 * (CharacterSpeed)) ? movesAllowed + 1 : movesAllowed;

            return movesAllowed;
        }

        /// <summary>
        /// Centers a text string for display.
        /// </summary>
        /// <param name="Text">Text to be centered.</param>
        /// <param name="Spaces">Total number of spaces in displayed string.</param>
        /// <returns></returns>
        public static string CenterString(string Text, int Spaces)
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

        #endregion
    }
}
