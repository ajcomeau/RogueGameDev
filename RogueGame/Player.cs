﻿using System;
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
        public const char CHARACTER = '☺';

        public enum HungerLevel
        {
            Satisfied = 3,
            Weak = 2,
            Faint = 1,
            Dead = 0
        }

        public string PlayerName { get; set; }
        public int HP { get; set; } = STARTING_HP;
        public int HPDamage { get; set; } = 0;
        public int Strength { get; set; } = STARTING_STRENGTH;
        public int StrengthMod { get; set; } = 0;
        public int Gold { get; set; }
        public int Experience { get; set; }
        public HungerLevel HungerState { get; set; } = HungerLevel.Satisfied;
        public int HungerTurn { get; set; }
        public int Confused { get; set; } = 0;
        public int Immobile { get; set; } = 0;
        public int Blind { get; set; } = 0;
        public bool HasAmulet { get; set; } = false;
        public Inventory? LeftHand { get; set; } = null;
        public Inventory? RightHand { get; set; } = null;
        public Inventory? Wielding { get; set; }


        public MapSpace? Location { get; set; }

        public Player(string PlayerName) {

            // Create a new player object.
            var rand = new Random();
            this.PlayerName = PlayerName;
            this.Gold = 0;
            this.Experience = 1;
            this.HungerTurn = rand.Next(MIN_FOODVALUE, MAX_FOODVALUE + 1);        
        }
    }
}
