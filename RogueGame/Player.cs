using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RogueGame
{
    /// <summary>
    /// Encapsulates all player properties and functions.
    /// </summary>
    internal class Player
    {
        /// <summary>
        /// Starting hit points
        /// </summary>
        private const int STARTING_HP = 12;
        /// <summary>
        /// Starting strength points
        /// </summary>
        private const int STARTING_STRENGTH = 16;
        /// <summary>
        /// Turns between hunger states
        /// </summary>
        public const int HUNGER_TURNS = 150;
        /// <summary>
        /// Display character
        /// </summary>
        public const char CHARACTER = '☺'; 
        /// <summary>
        /// Maximum items in inventory
        /// </summary>
        public const int INVENTORY_LIMIT = 21;
        /// <summary>
        /// Player's hunger stages. Decrement to increase hunger.
        /// </summary>
        public enum HungerLevel
        {            
            Satisfied = 4,
            Hungry = 3,
            Weak = 2,
            Faint = 1,
            Dead = 0
        }

        /// <summary>
        /// Name provided by player
        /// </summary>
        public string PlayerName { get; set; }  
        /// <summary>
        /// Maximum current hit points
        /// </summary>
        public int MaxHP { get; set; } = STARTING_HP; 
        /// <summary>
        /// Current damage in hit points
        /// </summary>
        public int HPDamage { get; set; }
        /// <summary>
        /// Current HP
        /// </summary>
        public int CurrentHP { get { return MaxHP - HPDamage; } } 
        /// <summary>
        /// Current max strength
        /// </summary>
        public int MaxStrength { get; set; } = STARTING_STRENGTH;
        /// <summary>
        /// Current strength modifier
        /// </summary>
        public int StrengthMod { get; set; } 
        /// <summary>
        /// Current Strength
        /// </summary>
        public int CurrentStrength { get { return MaxStrength - StrengthMod; } } 
        /// <summary>
        /// Current gold
        /// </summary>        
        public int Gold { get; set; }
        /// <summary>
        /// Current experience
        /// </summary>
        public int Experience { get; set; } 
        /// <summary>
        /// Current hunger level
        /// </summary>
        public HungerLevel HungerState { get; set; } = HungerLevel.Satisfied;
        /// <summary>
        /// Next turn at which hunger state will change
        /// </summary>
        public int HungerTurn { get; set; }     
        /// <summary>
        /// Confused - player moves erratically.
        /// </summary>
        public int Confused { get; set; } 
        /// <summary>
        /// Paralysis, frozen by ice monster, etc..
        /// </summary>
        public int Immobile { get; set; }
        /// <summary>
        /// Blind from potion, etc..
        /// </summary>
        public int Blind { get; set; }
        /// <summary>
        /// Whether player has found the amulet
        /// </summary>
        public bool HasAmulet { get; set; }
        /// <summary>
        /// Armor currently worn
        /// </summary>
        public Inventory? Armor { get; set; } 
        /// <summary>
        /// Left hand ring
        /// </summary>
        public Inventory? LeftHand { get; set; }   
        /// <summary>
        /// Right hand ring
        /// </summary>
        public Inventory? RightHand { get; set; }
        /// <summary>
        /// Weapon
        /// </summary>
        public Inventory? Wielding { get; set; } 
        /// <summary>
        /// Main inventory list.
        /// </summary>
        public List<Inventory> PlayerInventory { get; set; } 
        /// <summary>
        /// Current map space occupied
        /// </summary>
        public MapSpace? Location { get; set; } 
        /// <summary>
        /// Calculates user experience level based on experience points.
        /// </summary>
        /// <returns>Experience level number</returns>
        public int ExperienceLevel()
        {
            // Calculate player's experience level based on 
            // experience points.

            int returnLevel = 0;
            int nextLevelReq = 5;

            do { 
                nextLevelReq *= 2; 
                returnLevel += 1;
            }
            while (this.Experience > nextLevelReq);

            return returnLevel;
        }

        /// <summary>
        /// Primary constructor for creating new player when game starts.
        /// </summary>
        /// <param name="PlayerName"></param>
        public Player(string PlayerName) {

            // Create a new player object
            var rand = new Random();
            this.PlayerName = PlayerName;
            this.PlayerInventory = new List<Inventory>();
            this.Gold = 0;
            this.Experience = 1;
            this.HungerTurn = rand.Next(Inventory.MIN_FOODVALUE, Inventory.MAX_FOODVALUE + 1);

            // Add inventory items
            this.PlayerInventory.Add(Inventory.GetInventoryItem("some food")!);
            this.PlayerInventory.Add(Inventory.GetInventoryItem("studded leather armor")!);
            this.PlayerInventory.Add(Inventory.GetInventoryItem("a mace")!);
            this.PlayerInventory.Add(Inventory.GetInventoryItem("a short bow")!);

            // Add batch of arrows
            for (int i = 1; i <= rand.Next(1, Inventory.MAX_AMMO_BATCH + 1); i++)
                this.PlayerInventory.Add(Inventory.GetInventoryItem("an arrow")!);

            // Check for null items in list and remove
            this.PlayerInventory = this.PlayerInventory.Where(x => x != null).ToList();

        }
    }
}
