﻿using System;
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
        public enum InvCategory
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

        // Item templates - program grabs these at random to create new inventory on map.
        // PriorityID values must be unique. These will be used to identify the item elsewhere and to order the
        // items in the inventory listing.
        private static List<Inventory> invItems = new List<Inventory>()
        {
            new Inventory(InvCategory.Food, 1, "some food", "some food", "rations of food", '♣', 95, ConsumeFood),
            new Inventory(InvCategory.Food, 2, "a mango", "a mango", "mangoes", '♣', 95, ConsumeFood)
        };

        public static ReadOnlyCollection<Inventory> InventoryItems => invItems.AsReadOnly();

        public const int MAX_FOODVALUE = 1700;      // Maximum turns gained from food ration.
        public const int MIN_FOODVALUE = 900;       // Minimum turns gained from food ration.


        public InvCategory ItemCategory { get; set; }
        public int PriorityId { get; set; }     // Unique ID used for ordering.
        public string? CodeName { get; set; }   // Name if unidentified.
        public string RealName { get; set; }    // Identified name.
        public string PluralName { get; set; }  // Plural of RealName
        public bool IsIdentified { get; set; }
        public bool IsGroupable { get; set; }   // Can more than one of these fit in an inventory slot?
        public bool IsWieldable { get; set; }   // Can it be used as a weapon?
        public bool IsCursed { get; set; }  
        public int ArmorClass { get; set; } 
        public int Increment { get; set; }      // Effectiveness bonus
        public int DmgIncrement { get; set; }   // Damage bonus
        public int AccIncrement { get; set; }   // Accuracy bonus
        public int MinDamage { get; set; }      // Minimum damaage for weapon
        public int MaxDamage { get; set; }      // Maximum damage for weapon
        public int AppearancePct { get; set; }  // Probability of item being generated when selected randomly.
        public char DisplayCharacter { get; set; }  // Symbol to be displayed.
        public Func<Player, MapLevel.Direction, bool>? ThrowFunction { get; set; }  // Delegate function for throwing item.
        public Func<Player, MapLevel.Direction, bool>? ZapFunction { get; set; } // Delegate function if item can be used to zap.
        public Func<Player, Inventory?, bool>? MainFunction { get; set; }  // Default delegate function.

        public Inventory(InvCategory InvType, int PriorityID, string CodeName, string RealName, string PluralName, bool Identified,
            bool Groupable, bool Wieldable, bool Cursed, int ArmorClass, int Increment, int DamageInc, int AccuracyInc,
            int MinDamage, int MaxDamage, int AppearancePct, char DisplayChar, Func<Player, Inventory?, bool>? mainFunction, 
            Func<Player, MapLevel.Direction, bool>? Throw = null, Func<Player, MapLevel.Direction, bool>? Zap = null)
        {
            // Apply parameters
            this.ItemCategory = InvType; 
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

        public Inventory(InvCategory InvType, int PriorityID, string CodeName, string RealName, string PluralName, char DisplayChar, 
            int AppearancePct, Func<Player, Inventory?, bool> mainFunction, Func<Player, MapLevel.Direction, bool>? Throw = null, 
            Func<Player, MapLevel.Direction, bool>? Zap = null)
        {
            // Apply parameters and most common settings
            this.ItemCategory = InvType;
            this.CodeName = CodeName;
            this.RealName = RealName;
            this.PluralName = PluralName;
            this.DisplayCharacter = DisplayChar;
            this.IsGroupable = true;
            this.AppearancePct = AppearancePct;

            // If it's a weapon, it's wieldable.
            this.IsWieldable = (InvType == InvCategory.Weapon);

            // If the two names are the same or the type is greater than 6, it's identified.
            this.IsIdentified = (this.RealName == this.CodeName || (int)this.ItemCategory > 5);

            // Delegates
            this.MainFunction = mainFunction;
            this.ThrowFunction = (Throw != null) ? Throw : null;
            this.ZapFunction = (Zap != null) ? Zap : null;
        }

        public Inventory()
        {
            // Contructor for blank item.
            this.RealName = "";
            this.DisplayCharacter = '0';
        }


        public static List<InventoryLine> InventoryDisplay(List<Inventory> PlayerInventory)
        {
            char charID = 'a';

            // Get the player's current inventory in a grouped format.

            List<InventoryLine> lines = new List<InventoryLine>();

            // Get groupable identified inventory.
            var groupedInventory =
                (from invEntry in PlayerInventory
                    where invEntry.IsGroupable && invEntry.IsIdentified
                    group invEntry by invEntry.RealName into itemGroup
                    select itemGroup).ToList();

            // Add groupable non-identified inventory.
            groupedInventory.Concat(
                from invEntry in PlayerInventory
                where invEntry.IsGroupable && !invEntry.IsIdentified
                group invEntry by invEntry.CodeName into itemGroup
                select itemGroup).ToList();

            // Get non-groupable identified
            var individualItems =
                (from invEntry in PlayerInventory
                    where !invEntry.IsGroupable
                    select invEntry).ToList();

            // Create a unique list of grouped items and count of each.
            foreach (var itemGroup in groupedInventory)
                lines.Add(new InventoryLine { Count = itemGroup.Count(), InvItem = itemGroup.First() });

            // Add non-grouped items.
            foreach (var invEntry in individualItems)
                lines.Add(new InventoryLine { Count = 1, InvItem = invEntry });

            // Order new list by item category.
            lines = lines.OrderBy(x => x.InvItem.ItemCategory).ToList();

            // Call the ListingDescription function to get a finished description.
            foreach (InventoryLine line in lines)
            {
                line.ID = charID;
                line.Description = line.ID + ".) " + ListingDescription(line.Count, line.InvItem);
                charID++;
            }

            return lines;

        }

        public static string ListingDescription(int Number, Inventory Item)
        {
            // Signle function to create inventory listing description for item and 
            // handle all the grammatical adjustments.

            string retValue = "";

            switch (Item.ItemCategory)
            {
                // Make adjustments by inventory category.
                case InvCategory.Food:
                    if (Number == 1)
                    {
                        if (Item.RealName == "some food")
                            retValue = Item.RealName;
                        else
                            retValue = "1 " + Item.RealName;
                    }
                    else
                        retValue = Number.ToString() + " " + Item.PluralName;
                    break;
                case InvCategory.Ammunition:
                    retValue = Number == 1 ? "1 " + Item.RealName : Number.ToString() + " " + Item.PluralName;
                    break;
                case InvCategory.Ring:
                case InvCategory.Scroll:
                case InvCategory.Wand:
                case InvCategory.Staff:
                case InvCategory.Potion:
                    if (Item.IsIdentified)
                        retValue = Number == 1 ? "1 " + " " + Item.ItemCategory.ToString() + " of " + Item.RealName 
                            : Number.ToString() + " " + Item.ItemCategory.ToString() + "s of " + Item.RealName;
                    else
                        retValue = Number == 1 ? "1 " + " " + Item.ItemCategory.ToString() + " called " + Item.CodeName
                            : Number.ToString() + " " + Item.ItemCategory.ToString() + "s called " + Item.CodeName;
                    break;

                default:
                    retValue = "A" + Item.RealName;
                    break;
            }

            return retValue;

        }


        public static Inventory GetInventoryItem(InvCategory InvType)
        {
            // Get a random item from a specific inventory category.
            List<Inventory> invSelect = (from Inventory item in InventoryItems
                                            where item.ItemCategory == InvType
                                            select item).ToList();
            
            return invSelect[rand.Next(invSelect.Count)]; 
        }

        public static Inventory GetInventoryItem()
        {
            // Get a random item from the inventory templates.
            return InventoryItems[rand.Next(InventoryItems.Count)];
        }


        public static bool ConsumeFood(Player currentPlayer, Inventory? inventoryItem = null)
        {
            // Delegate for consuming food
            //TODO: In development.
            Debug.WriteLine("Chow down, " + currentPlayer.PlayerName + "!");
            currentPlayer.PlayerInventory[0].AccIncrement  = 0;
            return true;
        }
    }

    internal class InventoryLine
    {
        // InventoryLine class for formatting inventory display.
        public char ID;
        public int Count;
        public Inventory InvItem;
        public string Description;
    }

}
