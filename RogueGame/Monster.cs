using RogueGame;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueLike
{
    internal class Monster
    {

        /// <summary>
        /// Monster templates - program grabs these at random to spawn new monsters on map.
        /// </summary>
        private static List<Monster> monsterIncubator = new List<Monster>()
        {
            new Monster("Kestral", 1, 8, 2, 1, 6, 25, 1, 4, 'K', 0, null, true, false),
            new Monster("Snake", 1, 8, 2, 1, 6, 25, 1, 3, 'S', 0, null, true, false)
        };

        /// <summary>
        /// Read-only collection of monster templates.
        /// </summary>
        public static ReadOnlyCollection<Monster> Monsters => monsterIncubator.AsReadOnly();

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
        public int HPDamage { get; set; } = 0;
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
        public Func<Player>? SpecialAttack { get; set; }
        /// <summary>
        /// Does the monster initiate attacks on sight?
        /// </summary>
        public bool Aggressive { get; set; }
        /// <summary>
        /// Is the monster currently angry and persisting in an atack?
        /// </summary>
        public bool Angered { get; set; } = false;
        /// <summary>
        /// Can the monster regenerate hit points?
        /// </summary>
        public bool CanRegenerate { get; set; }
        public int Confused { get; set; } = 0;
        public int Immobile { get; set; } = 0;
        public int Blind { get; set; } = 0;
        /// <summary>
        /// Current gold
        /// </summary>        
        public int Gold { get; set; } = 0;
        /// Inventory
        public List<Inventory> MonsterInventory { get; set; }
        public MapSpace? Location { get; set; }

        /// <summary>
        /// Main constructor for creating monster from scratch with defaults.
        /// </summary>
        /// <param name="monsterName">Aquator, Centaur, Griffin, etc..</param>
        /// <param name="minStartingHP">Min limit for starting HP to be determined randomly.</param>
        /// <param name="maxStartingHP">Max limit for starting HP to be determined randomly.</param>
        /// <param name="armorClass">Armor class</param>
        /// <param name="minLevel">First level on which monster appears</param>
        /// <param name="maxLevel">Last level on which monster appears</param>
        /// <param name="appearancePct">Probability of monster appearing</param>
        /// <param name="minAttackDmg">Minimum amount of damage to be dealt by monster</param>
        /// <param name="maxAttackDmg">Maximum amount of damage to be dealt by monster</param>
        /// <param name="displayCharacter">Character used to show monster</param>
        /// <param name="specialAttackPct">Probability of monster using special attack</param>
        /// <param name="specialAttack">Special attack function</param>
        /// <param name="aggressive">Does the monster initiate attacks on sight?</param>
        /// <param name="canRegenerate">Is the monster currently angry and persisting in an atack?</param>
        /// <param name="monsterInventory">Inventory list</param>
        public Monster(string monsterName, int minStartingHP, int maxStartingHP,  
            int armorClass, int minLevel, int maxLevel, int appearancePct, 
            int minAttackDmg, int maxAttackDmg, char displayCharacter, int specialAttackPct, 
            Func<Player>? specialAttack, bool aggressive, bool canRegenerate)
        {
            this.MonsterName = monsterName;
            this.MinStartingHP = minStartingHP;
            this.MaxStartingHP = maxStartingHP;
            this.MaxHP = Game.rand.Next(this.MinStartingHP, this.MaxStartingHP + 1);
            this.ArmorClass = armorClass;
            this.MinLevel = minLevel;
            this.MaxLevel = maxLevel;
            this.AppearancePct = appearancePct;
            this.MinAttackDmg = minAttackDmg;
            this.MaxAttackDmg = maxAttackDmg;
            this.DisplayCharacter = displayCharacter;
            this.SpecialAttackPct = specialAttackPct;
            this.SpecialAttack = specialAttack;
            this.Aggressive = aggressive;
            this.CanRegenerate = canRegenerate;
            this.MonsterInventory = new List<Inventory>();
        }

        /// <summary>
        /// Clone a monster from one of the incubator definitions.
        /// </summary>
        /// <param name="original">Original monster object for cloning.</param>
        public Monster(Monster original)
        {
            this.MonsterName = original.MonsterName;
            this.MinStartingHP = original.MinStartingHP;
            this.MaxStartingHP = original.MaxStartingHP;
            this.MaxHP = Game.rand.Next(this.MinStartingHP, this.MaxStartingHP + 1);
            this.HPDamage = original.HPDamage;
            this.ArmorClass = original.ArmorClass;
            this.MinLevel = original.MinLevel;
            this.MaxLevel = original.MaxLevel;
            this.AppearancePct = original.AppearancePct;
            this.MinAttackDmg = original.MinAttackDmg;
            this.MaxAttackDmg = original.MaxAttackDmg;
            this.DisplayCharacter = original.DisplayCharacter;
            this.SpecialAttackPct = original.SpecialAttackPct;
            this.SpecialAttack = original.SpecialAttack;
            this.Aggressive = original.Aggressive;
            this.CanRegenerate = original.CanRegenerate;
            this.MonsterInventory = new List<Inventory>();
        }   
    
    
    }
}
