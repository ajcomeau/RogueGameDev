﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RogueGame
{
    internal class Player
    {
        private const int STARTING_HP = 12;         // Starting hit points
        private const int STARTING_STRENGTH = 16;   // Starting strength points
        private const int HUNGER_TURNS = 150;       // Turns between hunger states
        public const char CHARACTER = '☺';          // Display character
        public const int INVENTORY_LIMIT = 50;      // Maximum items in inventory

        public enum HungerLevel
        {
            Satisfied = 3,
            Weak = 2,
            Faint = 1,
            Dead = 0
        }

        public string PlayerName { get; set; }  
        public int HP { get; set; } = STARTING_HP;  // Maximum current hit points
        public int HPDamage { get; set; }           // Current damage in hit points
        public int Strength { get; set; } = STARTING_STRENGTH;  // Current max strength
        public int StrengthMod { get; set; }        // Current strength modifier
        public int Gold { get; set; }               // Current gold
        public int Experience { get; set; }         // Current experience
        public HungerLevel HungerState { get; set; } = HungerLevel.Satisfied;
        public int HungerTurn { get; set; }         // Next turn at which hunger state will change      
        public int Confused { get; set; }           // Temporary disabilities
        public int Immobile { get; set; }
        public int Blind { get; set; }
        public bool HasAmulet { get; set; }
        public Inventory? LeftHand { get; set; }   // Rings
        public Inventory? RightHand { get; set; }
        public Inventory? Wielding { get; set; }    // Weapon
        public List<Inventory> PlayerInventory { get; set; }   // Main inventory list.

        public MapSpace? Location { get; set; }     

        public Player(string PlayerName) {

            // Create a new player object.
            var rand = new Random();
            this.PlayerName = PlayerName;
            this.PlayerInventory = new List<Inventory>();
            this.Gold = 0;
            this.Experience = 1;
            this.HungerTurn = rand.Next(Inventory.MIN_FOODVALUE, Inventory.MAX_FOODVALUE + 1); 
        }



    }
}
