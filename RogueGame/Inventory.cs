﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueGame
{
    internal class Inventory
    {
        public enum InventoryType
        {
            Food = 0,
            Ring = 1,
            Scroll = 2,
            Wand = 3,
            Staff = 4,
            Potion = 5,
            Armor = 6,
            Weapon = 7,
            Ammunition = 8,
            Amulet = 9
        }

        public InventoryType ItemType { get; set; }
        public string? CodeName { get; set; }
        public string RealName { get; set; }
        public bool IsIdentified { get; set; }
        public bool IsGroupable { get; set; }
        public bool IsWieldable { get; set; }
        public bool IsCursed { get; set; }
        public int Increment { get; set; }
        public int DmgIncrement { get; set; }
        public int AccIncrement { get; set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public Func<MapLevel.Direction, bool>? ThrowFunction { get; set; }
        public Func<MapLevel.Direction, bool>? ZapFunction { get; set; }
        public Func<bool> MainFunction { get; set; }


    }
}
