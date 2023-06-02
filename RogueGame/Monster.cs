using RogueGame;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace RogueGame
{
    internal class Monster
    {
        public enum Activity {
            Resting = 0,
            Wandering = 1,
            Angered = 2
        }

        
        
        /// <summary>
        /// Monster templates - program grabs these at random to spawn new monsters on map.
        /// </summary>
        private static List<Monster> monsterIncubator = new List<Monster>()
        {
            new Monster("Aquator"       , 5, 40, 5, 7, 15, 50, 0, 0, 'A', 0, null, true, 10, false),
            new Monster("Bat"           , 1, 8, 2, 1, 5, 50, 1, 2, 'B', 0, null, true, 10, false),
            new Monster("Centaur"       , 4, 32, 5, 8, 17, 50, 3, 12, 'C', 0, null, true, 10, false),
            new Monster("Dragon"        , 10, 80, 10, 22, 26, 50, 5, 46, 'D', 0, null, true,  10, false),
            new Monster("Emu"           , 1, 8, 2, 2, 11, 50, 1, 2, 'E', 0, null, true,  10, false),
            new Monster("Flytrap"       , 8, 64, 9, 15, 20, 50, 0, 0, 'F', 0, null, true, 10,  false),
            new Monster("Griffin"       , 13, 104, 5, 6, 15, 50, 7, 27, 'G', 0, null, true, 10, false),
            new Monster("Hobgoblin"     , 1, 8, 3, 1, 10, 50, 1, 8, 'H', 0, null, true, 10, false),
            new Monster("Ice Monster"   , 1, 8, 2, 3, 12, 50, 0, 0, 'I', 0, null, true, 10, false),
            new Monster("Jabberwock"    , 15, 120, 9, 20, 26, 50, 4, 32, 'J', 0, null, true, 10, false),
            new Monster("Kestral"       , 1, 8, 2, 1, 6, 50, 1, 4, 'K', 0, null, true, 10, false),
            new Monster("Leprechaun"    , 3, 24, 5, 7, 16, 50, 1, 1, 'L', 0, null, true, 10, false),
            new Monster("Medusa"        , 8, 64, 5, 19, 26, 50, 8, 34, 'M', 0, null, true, 10, false),
            new Monster("Nymph"         , 3, 24, 5, 11, 20, 50, 0, 0, 'N', 0, null, true, 10, false),
            new Monster("Orc"           , 1, 8, 3, 4, 13, 50, 1, 8, 'O', 0, null, true, 10, false),
            new Monster("Phantom"       , 8, 64, 9, 20, 26, 50, 4, 16, 'P', 0, null, true, 10, false),
            new Monster("Quagga"        , 3, 24, 9, 10, 19, 50, 2, 10, 'Q', 0, null, true, 10, false),
            new Monster("Rattlesnake"   , 2, 16, 5, 9, 18, 50, 1, 6, 'R', 0, null, true, 10, false),
            new Monster("Snake"         , 1, 8, 2, 1, 9, 50, 1, 3, 'S', 0, null, true, 10, false),
            new Monster("Troll"         , 6, 48, 9, 13, 22, 50, 4, 28, 'T', 0, null, true, 10, false),
            new Monster("Ur-vile"       , 7, 56, 9, 18, 26, 50, 4, 36, 'U', 0, null, true, 10, false),
            new Monster("Vampire"       , 8, 64, 9, 20, 26, 50, 1, 10, 'V', 0, null, true, 10, false),
            new Monster("Wraith"        , 5, 40, 9, 14, 23, 50, 1, 6, 'W', 0, null, true, 10, false),
            new Monster("Xeroc"         , 7, 56, 9, 17, 26, 50, 4, 16, 'X', 0, null, true, 10, false),
            new Monster("Yeti"          , 4, 32, 5, 12, 21, 50, 2, 12, 'Y', 0, null, true, 10, false),
            new Monster("Zombie"        , 2, 16, 5, 7, 14, 50, 1, 8, 'Z', 0, null, true, 10, false),
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
        public int MinStartingHP { get; }
        /// <summary>
        /// Max limit for starting HP to be determined randomly.
        /// </summary>
        public int MaxStartingHP { get; }
        /// <summary>
        /// Actual starting HP
        /// </summary>
        public int MaxHP { get; }
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
        /// How likely is the monster to rest on a turn? Higher setting slows movement.
        /// </summary>
        public int Inertia { get; set; }        
        /// <summary>
        /// What is the monster's current activity level?
        /// </summary>
        public Activity CurrentState { get; set; } = Activity.Resting;
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
        public MapSpace? Destination { get; set; }
        public MapLevel.Direction? Direction { get; set; }



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
            Func<Player>? specialAttack, bool aggressive, int inertia, bool canRegenerate)
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
            this.Inertia = inertia;
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
            this.Inertia = original.Inertia;
            this.CanRegenerate = original.CanRegenerate;
            this.MonsterInventory = new List<Inventory>();            
        }


        public static Monster? SpawnMonster(int LevelNumber)
        {
            int itemSelect = 0;

            List<Monster> retList = (from Monster item in Monsters
                                        where item.MinLevel <= LevelNumber && item.MaxLevel >= LevelNumber
                                        select item).ToList();

            if (retList.Count > 0)
            {
                itemSelect = Game.rand.Next(0, retList.Count);
                return new Monster(retList[itemSelect]);
            }
            else return null;
                
        }

    }
}
