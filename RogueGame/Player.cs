using System.Runtime.InteropServices.Marshalling;
using static RogueGame.GameTools;
using static RogueGame.Inventory.InvTemplateID;

namespace RogueGame
{
    /// <summary>
    /// Encapsulates all player properties and functions.
    /// </summary>
    internal class Player : Character
    {
        #region Constants
        /// <summary>
        /// Display character
        /// </summary>
        public static readonly MapGlyph CHARACTER = new MapGlyph('☺', Color.LightYellow, Color.Black);
        /// <summary>
        /// Maximum items in inventory
        /// </summary>
        public const int INVENTORY_LIMIT = 20;
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
        #endregion

        #region Properties
        /// <summary>
        /// Current max strength
        /// </summary>
        public int MaxStrength { get; set; } = PLAYER_START_STRENGTH;
        /// <summary>
        /// Current strength modifier
        /// </summary>
        public int StrengthMod { get; set; }
        /// <summary>
        /// Current Strength
        /// </summary>
        public int CurrentStrength { get { return MaxStrength + StrengthMod; } }        
        /// <summary>
        /// Current experience
        /// </summary>
        public int Experience { get; set; }
        /// <summary>
        /// Hallucinating - player see things that aren't there.
        /// </summary>
        public int Hallucinating { get; set; } = 0;
        /// <summary>
        /// Current hunger level
        /// </summary>
        public HungerLevel HungerState { get; set; } = HungerLevel.Satisfied;
        /// <summary>
        /// Next turn at which hunger state will change
        /// </summary>
        public int HungerTurn { get; set; }

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
        /// Player experience level based on experience points.
        /// </summary>
        public int ExpLevel { get; set; } = 1;
        /// <summary>
        /// Hit points at which to level up player next.
        /// </summary>
        public int NextExpLevelUp { get; set; } = 10;
        /// <summary>
        /// If the player is currently in a fight, store the 
        /// opponent here.
        /// </summary>
        public Monster? Opponent { get; set; } = null;
        /// <summary>
        /// Tuple property to record ending turn for an inventory effect and
        /// next delegate to be called when activated.
        /// </summary>
        public (int EndingTurn, Action TargetFunction)? InventoryEffect { get; set; } = null;
        /// <summary>
        /// Primary constructor for creating new player when game starts.
        /// </summary>
        /// <param name="PlayerName">Player's name</param>
        /// <param name="AssignedInventory">List of assigned Inventory objects</param>
        public Player(string PlayerName, List<Inventory>? AssignedInventory) {

            Inventory dInv = new Inventory(false);
            List<Inventory>? assigned = AssignedInventory;

            // Create a new player object
            var rand = new Random();
            this.CharacterName = PlayerName;
            this.CharacterInventory = new List<Inventory>();
            this.Gold = 0;
            this.Experience = 1;
            this.HungerTurn = rand.Next(MIN_FOODVALUE, MAX_FOODVALUE + 1);
            this.MaxHP = PLAYER_START_HP;

            if (assigned != null)
            {
                foreach (Inventory item in assigned)
                {
                    // For ammunition, which is always groupable, add a random number.
                    if (item.ItemCategory == Inventory.InvCategory.Ammunition)
                    {
                        for (int i = 1; i <= rand.Next(1, MAX_AMMO_BATCH + 1); i++)
                            this.CharacterInventory.Add(item);
                    }
                    else
                        // For everything else, just add the item.
                        this.CharacterInventory.Add(item);

                    // Set the first armor added to the worn armor.
                    if (item.ItemCategory == Inventory.InvCategory.Armor && this.Armor == null) 
                        this.Armor = item; 
                    
                    // Set the first weapon to be wielded.
                    if (item.ItemCategory == Inventory.InvCategory.Weapon && this.Wielding == null) 
                        this.Wielding = item; 
                }
            }
        }
        /// <summary>
        /// Heal the player by the specified number of hit points.
        /// </summary>
        /// <param name="healingPoints">The number of hitpoints of healing to give the player.</param>
        /// <param name="expTurn">The new expiration turn for blindness, confusion and hallucination.</param>
        public void Healing(int healingPoints, int expTurn)
        {
            if (this.HPDamage == 0)
                this.MaxHP += 1;

            this.HPDamage = (healingPoints > this.HPDamage) ? 0 :
                this.HPDamage - healingPoints;

            // For blindness, confusion and hallucination, set any remaining time to one
            // less than the current turn to let EvaluatePlayer() display
            // the appropriate messages.
            if (this.Blind > 0)
                this.Blind = expTurn;

            if (this.Confused > 0)
                this.Confused = expTurn;

            if (this.Hallucinating > 0)
                this.Hallucinating = expTurn;
        }
        /// <summary>
        /// Get the player's actual strength with any rings and other bonuses.
        /// </summary>
        /// <returns></returns>
        public int TotalStrength()
        {
            int retValue = 0;

            retValue = this.CurrentStrength;

            if (this.LeftHand != null && this.LeftHand.PriorityId == RingOfAddStrength)
                retValue += this.LeftHand.Increment;

            if (this.RightHand != null && this.RightHand.PriorityId == RingOfAddStrength)
                retValue += this.RightHand.Increment;

            return retValue;
        }
        /// <summary>
        /// If the player has any searching assists, 
        /// return the degree of assistance.
        /// </summary>
        /// <returns></returns>
        public int AutoSearch()
        {
            int retValue = 0;

            // Look for the Ring of Searching on both hands.
            if (this.LeftHand != null &&
                this.LeftHand.PriorityId == RingOfSearching)
                retValue += this.LeftHand.Increment;

            if (this.RightHand != null &&
                this.RightHand.PriorityId == RingOfSearching)
                retValue += this.RightHand.Increment;

            // This ring makes the player hungry faster.
            this.HungerTurn -= retValue;

            return retValue;
        }
        /// <summary>
        /// Combines player's armor with other kinds of protection
        /// for total.
        /// </summary>
        /// <returns>Player's protection rating</returns>
        public int TotalProtection()
        {
            int retValue = 0;
            
            // Get player's armor.
            if (this.Armor != null)
                retValue = this.Armor.ArmorClass + this.Armor.Increment;

            // Look for the Ring of Protection on both hands.
            if (this.LeftHand != null &&
                this.LeftHand.PriorityId == RingOfProtection)
            {
                retValue += this.LeftHand.Increment;
                this.HungerTurn -= this.LeftHand.Increment;
            }                

            if (this.RightHand != null &&
                this.RightHand.PriorityId == RingOfProtection)
            {
                retValue += this.RightHand.Increment;
                this.HungerTurn -= this.RightHand.Increment;
            }

            // Return total
            return retValue;

        }
        /// <summary>
        /// Minimum and maximum damage potential for player.
        /// </summary>
        /// <returns></returns>
        public (int Min, int Max) DamagePotential()
        {
            (int Min, int Max) retValue = (0, 0);
            int stregthAdj = 0;

            // Start with weapon potential.
            if (this.Wielding != null)
            {
                retValue.Min = Wielding.MinDamage + Wielding.DmgIncrement;
                retValue.Max = Wielding.MaxDamage + Wielding.DmgIncrement;
            }
            else
            {
                retValue.Min = 1;
                retValue.Max = 4;
            }

            // Factor in player strength
            switch (this.TotalStrength())
            {
                case < 4:
                    stregthAdj = -4;
                    break;
                case 4:
                    stregthAdj = -3;
                    break;
                case 5:
                    stregthAdj = -2;
                    break;
                case 6:
                    stregthAdj = -1;
                    break;
                case <= 15:
                    stregthAdj = 0;
                    break;
                case <= 17:
                    stregthAdj = 1;
                    break;
                case 18:
                    stregthAdj = 2;
                    break;
                case <= 20:
                    stregthAdj = 3;
                    break;
                case 21:
                    stregthAdj = 4;
                    break;
                case <= 30:
                    stregthAdj = 5;
                    break;
                case >= 31:
                    stregthAdj = 6;
                    break;

            }

            retValue.Min += stregthAdj;
            retValue.Max += stregthAdj;

            // Look for the Ring of Increase Damage on both hands.
            if (this.LeftHand != null &&
                this.LeftHand.PriorityId == RingOfIncreaseDamage)
            {
                retValue.Min += this.LeftHand.Increment;
                retValue.Max += this.LeftHand.Increment;
                this.HungerTurn -= this.LeftHand.Increment;
            }

            if (this.RightHand != null &&
                this.RightHand.PriorityId == RingOfIncreaseDamage)
            {
                retValue.Min += this.RightHand.Increment;
                retValue.Max += this.RightHand.Increment;
                this.HungerTurn -= this.RightHand.Increment;
            }

            return retValue;
        }

        #endregion

    }
}
