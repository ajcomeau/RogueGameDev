using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace RogueGame
{
    /// <summary>
    /// Encapsulates all inventory item properties and special functions.
    /// </summary>
    internal class Inventory
    {
        /// <summary>
        /// Inventory category
        /// </summary>
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

        /// <summary>
        /// Random number generator
        /// </summary>
        private static Random rand = new Random();

        /// <summary>
        /// Item templates - program grabs these at random to create new inventory on map.
        /// PriorityID values MUST BE UNIQUE. These will be used to identify the item elsewhere and to order the
        /// items in the inventory listing.
        /// </summary>
        private static List<Inventory> invItems = new List<Inventory>()
        {
            new Inventory(InvCategory.Food, 1, "some food", "some food", "rations of food", '♣', 20, null),
            new Inventory(InvCategory.Food, 2, "a mango", "a mango", "mangoes", '♣', 20, null),
            new Inventory(InvCategory.Armor, 3, "studded leather armor", "studded leather armor", "studded leather armor", true, false, false, false, 3, 1, 0, 0, 0, 0, 0, 15, '◘', null, null, null),
            new Inventory(InvCategory.Weapon, 4, "a mace", "a mace", "a mace", true, false, true, false, 0, 0, 1, 1, 2, 8, -3, 10, '↑', null, null, null),
            new Inventory(InvCategory.Weapon, 5, "a short bow", "a short bow", "a short bow", true, false, true, false, 0, 0, 0, 1, 1, 1, 0, 10, '↑', null, null, null),
            new Inventory(InvCategory.Ammunition, 6, "an arrow", "an arrow", "arrows", true, true, true, false, 0, 0, 0, 0, 1, 1, 3, 10, '↑', null, null, null),
            new Inventory(InvCategory.Amulet, 7, "The Amulet", "The Amulet", "The Amulet", true, false, false, false, 0, 0, 0, 0, 0, 0, 0, 0, MapLevel.AMULET, null, null, null)
        };

        /// <summary>
        /// Read-only collection of inventory templates.
        /// </summary>
        public static ReadOnlyCollection<Inventory> InventoryItems => invItems.AsReadOnly();
        /// <summary>
        /// Maximum turns gained from food ration.
        /// </summary>
        public const int MAX_FOODVALUE = 1700; 
        /// <summary>
        /// Minimum turns gained from food ration.
        /// </summary>
        public const int MIN_FOODVALUE = 900;
        /// <summary>
        /// Maximum items in a batch of arrows or bolts
        /// </summary>
        public const int MAX_AMMO_BATCH = 15;
        /// <summary>
        /// Item category from enumeration
        /// </summary>
        public InvCategory ItemCategory { get; set; }
        /// <summary>
        /// Unique ID used for ordering.
        /// </summary>
        public int PriorityId { get; set; } 
        /// <summary>
        /// Name if unidentified.
        /// </summary>
        public string? CodeName { get; set; }
        /// <summary>
        /// Identified name.
        /// </summary>
        public string RealName { get; set; } 
        /// <summary>
        /// Plural of RealName
        /// </summary>
        public string PluralName { get; set; }
        /// <summary>
        /// Has the item been identified?
        /// </summary>
        public bool IsIdentified { get; set; }
        /// <summary>
        /// Can more than one of these fit in an inventory slot?
        /// </summary>
        public bool IsGroupable { get; set; }
        /// <summary>
        /// How many items are there in the batch?
        /// </summary>
        public int Amount { get; set; } = 1;
        public bool IsWieldable { get; set; } 
        /// <summary>
        /// Is the item cursed?
        /// </summary>
        public bool IsCursed { get; set; }  
        /// <summary>
        /// Armor class rating
        /// </summary>
        public int ArmorClass { get; set; } 
        /// <summary>
        /// Effectiveness bonus
        /// </summary>
        public int Increment { get; set; } 
        /// <summary>
        /// Damage bonus
        /// </summary>
        public int DmgIncrement { get; set; } 
        /// <summary>
        /// Accuracy bonus
        /// </summary>
        public int AccIncrement { get; set; } 
        /// <summary>
        /// Minimum damaage for weapon
        /// </summary>
        public int MinDamage { get; set; } 
        /// <summary>
        /// Maximum damage for weapon
        /// </summary>
        public int MaxDamage { get; set; }
        /// <summary>
        /// Positive or negative bonus for weapon when thrown rather than wielded.
        /// </summary>
        public int ThrowingBonus { get; set; }
        /// <summary>
        /// Probability of item being generated when selected randomly.
        /// </summary>
        public int AppearancePct { get; set; } 
        /// <summary>
        /// Symbol to be displayed.
        /// </summary>
        public char DisplayCharacter { get; set; } 
        /// <summary>
        /// Delegate function for throwing item.
        /// </summary>
        public Func<Player, MapLevel.Direction, bool>? ThrowFunction { get; set; }
        /// <summary>
        /// Delegate function if item can be used to zap.
        /// </summary>
        public Func<Player, MapLevel.Direction, bool>? ZapFunction { get; set; }
        /// <summary>
        /// Default delegate function.
        /// </summary>
        public Func<Player, Inventory?, bool>? MainFunction { get; set; }

        /// <summary>
        /// Clone a new object off of another. To be used with inventory object templates.
        /// </summary>
        /// <param name="Original">Original object to be cloned.</param>
        /// <returns></returns>
        public Inventory(Inventory Original)
        {        
            this.ItemCategory = Original.ItemCategory;
            this.PriorityId = Original.PriorityId;
            this.CodeName = Original.CodeName;
            this.RealName = Original.RealName;
            this.PluralName = Original.PluralName;
            this.IsIdentified = Original.IsIdentified;
            this.IsGroupable = Original.IsGroupable;
            this.IsWieldable = Original.IsWieldable;
            this.IsCursed = Original.IsCursed;
            this.ArmorClass = Original.ArmorClass;
            this.Increment = Original.Increment;
            this.AccIncrement = Original.AccIncrement;
            this.DmgIncrement = Original.DmgIncrement;
            this.MinDamage = Original.MinDamage;
            this.MaxDamage = Original.MaxDamage;
            this.ThrowingBonus = Original.ThrowingBonus;
            this.AppearancePct = Original.AppearancePct;
            this.DisplayCharacter = Original.DisplayCharacter;
            this.ThrowFunction = Original.ThrowFunction;
            this.ZapFunction = Original.ZapFunction;
            this.MainFunction = Original.MainFunction;
        }

        // TODO:  Create category-specific constructors.
        /// <summary>
        /// Constructor for creating inventory item from scratch.
        /// </summary>
        /// <param name="InvType">Inventory category</param>
        /// <param name="PriorityID">Template ID</param>
        /// <param name="CodeName">Name for unidentified item</param>
        /// <param name="RealName">Actual item name</param>
        /// <param name="PluralName">Plural form of name for display</param>
        /// <param name="Identified">Is the item identified?</param>
        /// <param name="Groupable">Can the item be part of a collection in inventory?</param>
        /// <param name="Wieldable">Can this be used as a weapon?</param>
        /// <param name="Cursed">Is it cursed?</param>
        /// <param name="ArmorClass">Class level for armor items</param>
        /// <param name="Increment">Item increment level</param>
        /// <param name="DamageInc">Damage increment</param>
        /// <param name="AccuracyInc">Accuracy increment</param>
        /// <param name="MinDamage">For weapons - minimum damage inflicted</param>
        /// <param name="MaxDamage">For weapons - maximum damage inflicted</param>
        /// <param name="ThrowingBonus">Positive or negative bonus for weapon when thrown rather than wielded.</param>
        /// <param name="AppearancePct">Probability percentage of item being on map</param>
        /// <param name="DisplayChar">Symbol used for display</param>
        /// <param name="mainFunction">Delegate function for primary use</param>
        /// <param name="Throw">Delegate function when thrown</param>
        /// <param name="Zap">Delegate function for staffs and wands</param>
        public Inventory(InvCategory InvType, int PriorityID, string CodeName, string RealName, string PluralName, bool Identified,
            bool Groupable, bool Wieldable, bool Cursed, int ArmorClass, int Increment, int DamageInc, int AccuracyInc,
            int MinDamage, int MaxDamage, int ThrowingBonus, int AppearancePct, char DisplayChar, Func<Player, Inventory?, bool>? mainFunction, 
            Func<Player, MapLevel.Direction, bool>? Throw = null, Func<Player, MapLevel.Direction, bool>? Zap = null)
        {
            // Apply parameters
            this.ItemCategory = InvType; 
            this.PriorityId = PriorityID;
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
            this.ThrowingBonus = ThrowingBonus;
            this.AppearancePct = AppearancePct;
            this.DisplayCharacter = DisplayChar;
            this.MainFunction = mainFunction;
            this.ThrowFunction = (Throw != null) ? Throw : null;
            this.ZapFunction = (Zap != null) ? Zap : null;
        }

        /// <summary>
        /// Primary constructor for randomly selecting item from template collection
        /// </summary>
        /// <param name="InvType">Inventory category</param>
        /// <param name="PriorityID">Template ID</param>
        /// <param name="CodeName">Name for unidentified item</param>
        /// <param name="RealName">Actual item name</param>
        /// <param name="PluralName">Plural form of name for display</param>
        /// <param name="AppearancePct">Probability percentage of item being on map</param>
        /// <param name="DisplayChar">Symbol used for display</param>
        /// <param name="mainFunction">Delegate function for primary use</param>
        /// <param name="Throw">Delegate function when thrown</param>
        /// <param name="Zap">Delegate function for staffs and wands</param>
        public Inventory(InvCategory InvType, int PriorityID, string CodeName, string RealName, string PluralName, char DisplayChar, 
            int AppearancePct, Func<Player, Inventory?, bool>? mainFunction, Func<Player, MapLevel.Direction, bool>? Throw = null, 
            Func<Player, MapLevel.Direction, bool>? Zap = null)
        {
            // Apply parameters and most common settings
            this.ItemCategory = InvType;
            this.PriorityId = PriorityID;
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
            this.MainFunction = (mainFunction != null) ? mainFunction : null;
            this.ThrowFunction = (Throw != null) ? Throw : null;
            this.ZapFunction = (Zap != null) ? Zap : null;
        }


        /// <summary>
        /// Coonstructor for blank inventory item
        /// </summary>
        public Inventory()
        {
            // Contructor for blank item.
            this.RealName = "";
            this.DisplayCharacter = '0';
        }

        /// <summary>
        /// Generates grouped inventory listing for inventory display screen.
        /// </summary>
        /// <param name="PlayerInventory"></param>
        /// <returns></returns>
        public static List<InventoryLine> InventoryDisplay(List<Inventory> PlayerInventory)
        {
            char charID = 'a';

            // Get the player's current inventory in a grouped format.

            List<InventoryLine> lines = new List<InventoryLine>();

            // Get groupable inventory.
            var groupedInventory =
                (from invEntry in PlayerInventory
                    where invEntry.IsGroupable
                    group invEntry by invEntry.PriorityId into itemGroup
                    select itemGroup).ToList();

            // Get non-groupable inventory.
            var individualItems =
                (from invEntry in PlayerInventory
                    where !invEntry.IsGroupable
                    select invEntry).ToList();

            // Create a unique list of grouped items and count of each.
            foreach (var itemGroup in groupedInventory)
                lines.Add(new InventoryLine { Count = itemGroup.Count(), 
                    InvItem = itemGroup.First() });

            // Add non-grouped items.
            foreach (var invEntry in individualItems)
                lines.Add(new InventoryLine { Count = 1, InvItem = invEntry });

            // Order new list by item category.
            lines = lines.OrderBy(x => x.InvItem.PriorityId).ToList();

            // Call the ListingDescription function to get a finished description.
            foreach (InventoryLine line in lines)
            {
                line.ID = charID;
                line.Description = line.ID + ".) " + ListingDescription(line.Count, line.InvItem);
                charID++;
            }

            return lines;
        }

        /// <summary>
        /// Main function for generating item description for inventory display.
        /// </summary>
        /// <param name="Number">Number of items in inventory slot</param>
        /// <param name="Item">Actual item</param>
        /// <returns></returns>
        public static string ListingDescription(int Number, Inventory Item)
        {
            // Signle function to create inventory listing description for item and 
            // handle all the grammatical adjustments.

            string increments = "";
            string retValue = "";

            switch (Item.ItemCategory)
            {
                // Make adjustments by inventory category.
                case InvCategory.Food:
                    if (Number == 1)
                        retValue = Item.RealName;
                    else
                        retValue = Number.ToString() + " " + Item.PluralName;
                    break;
                case InvCategory.Ammunition:
                    retValue = Number == 1 ? Item.RealName : Number.ToString() + " " + Item.PluralName;
                    break;
                case InvCategory.Weapon:
                    retValue = Item.RealName;
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
                    retValue = Item.RealName;
                    break;
            }

            if (Item.IsIdentified)
            {
                // Add increments if there are any.
                switch (Item.ItemCategory)
                {
                    case InvCategory.Ring:
                        if (Item.Increment != 0) increments = Item.Increment.ToString("+0;-#");
                        break;
                    case InvCategory.Armor:
                        increments += $"class {Item.ArmorClass} ";
                        if (Item.Increment != 0) increments += Item.Increment.ToString("+0;-#");
                        break;
                    case InvCategory.Wand:
                    case InvCategory.Staff:
                    case InvCategory.Weapon:
                    case InvCategory.Ammunition:
                        if (Item.Increment != 0) increments = Item.Increment.ToString("+0;-#") + " ";
                        increments += Item.AccIncrement.ToString("+0;-#") + " ";
                        increments += Item.DmgIncrement.ToString("+0;-#");
                        break;
                    default:
                        break;
                }
            }

            if (increments.Length > 0)
            {
                increments = increments.TrimEnd();
                increments = $" ({increments})";
                retValue += increments;
            }

            return retValue;

        }

        /// <summary>
        /// Get a specific inventory item by name from the list of templates.
        /// </summary>
        /// <param name="ItemName">Real name of item.</param>
        /// <returns></returns>
        public static Inventory? GetInventoryItem(string ItemName)
        {
            
            List<Inventory> retList = (from Inventory item in InventoryItems
                        where item.RealName == ItemName
                        select item).ToList();

            // Clone a new object from template.
            if (retList.Count > 0) return new Inventory(retList[0]); else return null;
        }

        /// <summary>
        /// Generates random item of specific category from inventory template list.
        /// </summary>
        /// <param name="InvType">Specific category</param>
        /// <returns></returns>
        public static Inventory GetInventoryItem(InvCategory InvType)
        {
            // Get a random item from a specific inventory category.
            List<Inventory> invSelect = (from Inventory item in InventoryItems
                                            where item.ItemCategory == InvType
                                            select item).ToList();

            // Clone a new object from template.
            return new Inventory(invSelect[rand.Next(invSelect.Count)]); 
        }

        /// <summary>
        /// Generates random item from inventory template list.
        /// </summary>
        /// <returns></returns>
        public static Inventory GetInventoryItem()
        {
            // Clone a new object from template.
            return new Inventory(InventoryItems[rand.Next(InventoryItems.Count)]);
        }
    }

    /// <summary>
    /// Class used for constructing inventory display lines.
    /// </summary>
    internal class InventoryLine
    {
        // InventoryLine class for formatting inventory display.
        public char ID;
        public int Count;
        public Inventory InvItem;
        public string Description;
    }

}
