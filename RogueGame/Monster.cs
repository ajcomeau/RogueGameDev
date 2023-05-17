using RogueGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueLike
{
    internal class Monster
    {
        /// <summary>
        /// Aquator, Centaur, Griffin, etc..
        /// </summary>
        public string MonsterName { get; set; }
        /// <summary>
        /// Min limit for starting HP to be determined randomly.
        /// </summary>
        public int MinStartingHP { get; set; }
        /// <summary>
        /// Max limit for starting HP to be determined randomly.
        /// </summary>
        public int MaxStartingHP { get; set; }
        /// <summary>
        /// Actual starting HP
        /// </summary>
        public int MaxHP { get; set; }
        /// <summary>
        /// Subtracted damage
        /// </summary>
        public int HPDamage { get; set; }
        /// <summary>
        /// Current hit points
        /// </summary>
        public int CurrentHP { get { return MaxHP - HPDamage; } }
        /// <summary>
        /// Armor class
        /// </summary>
        public int ArmorClass { get; set; }
        /// <summary>
        /// First level on which monster appears
        /// </summary>
        public int MinLevel { get; set; }
        /// <summary>
        /// Last level on which monster appears
        /// </summary>
        public int MaxLevel { get; set; }
        /// <summary>
        /// Probability of monster appearing
        /// </summary>
        public int AppearancePct { get; set; }
        /// <summary>
        /// Minimum amount of damage to be dealt by monster
        /// </summary>
        public int MinAttackDmg { get; set; }
        /// <summary>
        /// Maximum amount of damage to be dealt by monster
        /// </summary>
        public int MaxAttackDmg { get; set; }
        /// <summary>
        /// Character used to show monster
        /// </summary>
        public char DisplayCharacter { get; set; }
        /// <summary>
        /// Probability of monster using special attack
        /// </summary>
        public int SpecialAttackPct { get; set; }
        /// <summary>
        /// Special attack function
        /// </summary>
        public Func<Player> SpecialAttack { get; set; }
        /// <summary>
        /// Does the monster initiate attacks on sight?
        /// </summary>
        public bool Aggressive { get; set; }
        /// <summary>
        /// Is the monster currently angry and persisting in an atack?
        /// </summary>
        public bool Angered { get; set; }
        /// <summary>
        /// Can the monster regenerate hit points?
        /// </summary>
        public bool CanRegenerate { get; set; }
        public int Confused { get; set; }
        public int Immobile { get; set; }
        public int Blind { get; set; }
        /// Inventory
        public List<Inventory> MonsterInventory { get; set; }

    }
}
