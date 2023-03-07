using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace RogueGame
{
    internal class Player
    {
        private const int STARTING_HP = 12;
        private const int STARTING_STRENGTH = 16;
        public const int MAX_FOODVALUE = 1700;
        public const int MIN_FOODVALUE = 900;
        private const int HUNGER_TURNS = 150;

        public enum HungerLevel
        {
            Satisfied = 3,
            Weak = 2,
            Faint = 1,
            Dead = 0
        }

        public string PlayerName { get; set; }
        public int HP { get; set; }
        public int HPDamage { get; set; }
        public int Strength { get; set; }
        public int StrengthMod { get; set; }
        public int Gold { get; set; }
        public int Experience { get; set; }
        public HungerLevel HungerState { get; set; }
        public int HungerTurn { get; set; }


        public Player(string PlayerName) {

            // Create a new player object.
            var rand = new Random();
            this.PlayerName = PlayerName;
            this.HP = STARTING_HP;
            this.HPDamage = 0;
            this.Strength = STARTING_STRENGTH;
            this.StrengthMod = 0;
            this.Gold = 0;
            this.Experience = 1;
            this.HungerState = HungerLevel.Satisfied;
            this.HungerTurn = rand.Next(MIN_FOODVALUE, MAX_FOODVALUE + 1);            
        }
    }
}
