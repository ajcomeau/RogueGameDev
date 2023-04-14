using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Diagnostics;

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

        // Random number generator
        private static Random rand = new Random();

        private static List<Inventory> invItems = new List<Inventory>()
        {
            new Inventory(InventoryType.Food, "some food", "some food", "rations of food", '♣', 95, ConsumeFood),
            new Inventory(InventoryType.Food, "a mango", "a mango", "mangoes", '♣', 95, ConsumeFood)
        };

        public static ReadOnlyCollection<Inventory> InventoryItems => invItems.AsReadOnly();


        public InventoryType ItemType { get; set; }
        public string? CodeName { get; set; }
        public string RealName { get; set; }
        public string PluralName { get; set; }
        public bool IsIdentified { get; set; }
        public bool IsGroupable { get; set; }
        public bool IsWieldable { get; set; }
        public bool IsCursed { get; set; }
        public int ArmorClass { get; set; } 
        public int Increment { get; set; }
        public int DmgIncrement { get; set; }
        public int AccIncrement { get; private set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public int AppearancePct { get; set; }
        public char DisplayCharacter { get; set; }
        public Func<Player, MapLevel.Direction, bool>? ThrowFunction { get; set; }
        public Func<Player, MapLevel.Direction, bool>? ZapFunction { get; set; }
        public Func<Player, bool> MainFunction { get; set; }

        public Inventory(InventoryType InvType, string CodeName, string RealName, string PluralName, bool Identified,
            bool Groupable, bool Wieldable, bool Cursed, int ArmorClass, int Increment, int DamageInc, int AccuracyInc,
            int MinDamage, int MaxDamage, int AppearancePct, char DisplayChar, Func<Player, bool> mainFunction, 
            Func<Player, MapLevel.Direction, bool>? Throw = null, Func<Player, MapLevel.Direction, bool>? Zap = null)
        {
            // Apply parameters
            this.ItemType = InvType; 
            this.CodeName = CodeName; 
            this.RealName = RealName;
            this.PluralName = PluralName;
            this.IsIdentified = Identified;
            this.IsGroupable = Groupable;
            this.IsWieldable = Wieldable;
            this.IsCursed = Cursed;
            this.ArmorClass = ArmorClass;
            this.Increment = Increment;
            this.DmgIncrement = DamageInc;
            this.AccIncrement = AccuracyInc;
            this.MinDamage = MinDamage;
            this.MaxDamage = MaxDamage;
            this.AppearancePct = AppearancePct;
            this.DisplayCharacter = DisplayChar;
            this.MainFunction = mainFunction;
            this.ThrowFunction = (Throw != null) ? Throw : null;
            this.ZapFunction = (Zap != null) ? Zap : null;
        }

        public Inventory(InventoryType InvType, string CodeName, string RealName, string PluralName, char DisplayChar, 
            int AppearancePct, Func<Player, bool> mainFunction, Func<Player, MapLevel.Direction, bool>? Throw = null, 
            Func<Player, MapLevel.Direction, bool>? Zap = null)
        {
            // Apply parameters and most common settings
            this.ItemType = InvType;
            this.CodeName = CodeName;
            this.RealName = RealName;
            this.PluralName = PluralName;
            this.DisplayCharacter = DisplayChar;
            this.IsGroupable = true;
            this.AppearancePct = AppearancePct;

            // If it's a weapon, it's wieldable.
            this.IsWieldable = (InvType == InventoryType.Weapon);

            // If the two names are the same, it's identified.
            this.IsIdentified = (this.RealName == this.CodeName);

            this.MainFunction = mainFunction;
            this.ThrowFunction = (Throw != null) ? Throw : null;
            this.ZapFunction = (Zap != null) ? Zap : null;
        }

        public Inventory()
        {
            this.RealName = "";
            this.DisplayCharacter = '0';
        }


        public static Inventory GetInventoryItem(InventoryType InvType)
        {
            // Get a random item from a specific inventory type.
            List<Inventory> invSelect = (from Inventory item in InventoryItems
                                            where item.ItemType == InvType
                                            select item).ToList();
            
            return invSelect[rand.Next(invSelect.Count)]; 
        }

        public static Inventory GetInventoryItem()
        {
            // Get a random item from the inventory types.
            return InventoryItems[rand.Next(InventoryItems.Count)];
        }


        public static bool ConsumeFood(Player currentPlayer)
        {
            Debug.WriteLine("Chow down, " + currentPlayer.PlayerName + "!");
            currentPlayer.PlayerInventory[0].AccIncrement  = 0;
            return true;
        }
    }
}
