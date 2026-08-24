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
        #region Constants
        // Gameplay

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


    }
}
