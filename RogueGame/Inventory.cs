using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Xml;

namespace RogueGame
{
    /// <summary>
    /// Encapsulates all inventory item properties and special functions.
    /// </summary>
    internal class Inventory
    {
        #region Constants and Properties
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
            Amulet = 9,
            Gold = 10
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
        private List<Inventory> invItems;
        /// <summary>
        /// Read-only collection of inventory templates.
        /// </summary>
        public ReadOnlyCollection<Inventory> InventoryItems => invItems.AsReadOnly();
        /// <summary>
        /// List to contain potential code names for non-identified items.
        /// </summary>
        private List<Tuple<InvCategory, string>> CodeNames;
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
        /// Is the item assigned to the player at the start of the game?
        /// </summary>
        public bool IsAssigned { get; set;  }
        /// <summary>
        /// How many items are there in the batch?
        /// </summary>
        public int Amount { get; set; } = 1;
        /// <summary>
        /// Can the item be wielded as a weapon?
        /// </summary>
        public bool IsWieldable { get; set; }
        /// <summary>
        /// Is the item cursed?
        /// </summary>
        public bool IsCursed { get; set; }
        /// <summary>
        /// Is item protected from curse, attack, theft, etc..
        /// </summary>
        public bool IsProtected { get; set; }
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
        public MapGlyph DisplayCharacter { get; set; }
        /// <summary>
        /// Location of the item on the map.
        /// </summary>
        public MapSpace Location { get; set; }
        /// <summary>
        /// Delegate function for inventory effects
        /// </summary>
        public Func<object, object?, string>? DelegateFunction { get; set; }
        #endregion

        #region Procedures
        /// <summary>
        /// Creates a dummy inventory object for use by Game class.
        /// </summary>
        public Inventory(bool InitInventory)
        {
            // Setup inventory list with random code names.
            if (InitInventory) { 
                LoadCodeNames();
                LoadInventory();
                InitializeInventory();
            }
        }
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
            this.IsAssigned = Original.IsAssigned;
            this.IsWieldable = Original.IsWieldable;
            this.IsCursed = Original.IsCursed;
            this.IsProtected = Original.IsProtected;
            this.ArmorClass = Original.ArmorClass;
            this.Increment = Original.Increment;
            this.AccIncrement = Original.AccIncrement;
            this.DmgIncrement = Original.DmgIncrement;
            this.MinDamage = Original.MinDamage;
            this.MaxDamage = Original.MaxDamage;
            this.ThrowingBonus = Original.ThrowingBonus;
            this.AppearancePct = Original.AppearancePct;
            this.DisplayCharacter = Original.DisplayCharacter;            
            this.DelegateFunction = Original.DelegateFunction;
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
        /// <param name="Protected">Is the item protected from attack, theft, etc.?</param>
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
            bool Groupable, bool Assigned, bool Wieldable, bool Cursed, bool Protected, int ArmorClass, int Increment, int DamageInc, int AccuracyInc,
            int MinDamage, int MaxDamage, int ThrowingBonus, int AppearancePct, MapGlyph DisplayChar, Func<object, object, string>? DelFunc = null)
        {
            // Apply parameters
            this.ItemCategory = InvType; 
            this.PriorityId = PriorityID;
            this.CodeName = CodeName; 
            this.RealName = RealName;
            this.PluralName = PluralName;
            this.IsIdentified = Identified;
            this.IsGroupable = Groupable;
            this.IsAssigned = Assigned;
            this.IsWieldable = Wieldable;
            this.IsCursed = Cursed;
            this.IsProtected = Protected;
            this.ArmorClass = ArmorClass;
            this.Increment = Increment;
            this.DmgIncrement = DamageInc;
            this.AccIncrement = AccuracyInc;
            this.MinDamage = MinDamage;
            this.MaxDamage = MaxDamage;
            this.ThrowingBonus = ThrowingBonus;
            this.AppearancePct = AppearancePct;
            this.DisplayCharacter = DisplayChar;
            this.DelegateFunction = DelFunc;
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
        public Inventory(InvCategory InvType, int PriorityID, string CodeName, string RealName, string PluralName, MapGlyph DisplayChar, 
            int AppearancePct, bool Assigned)
        {
            // Apply parameters and most common settings
            this.ItemCategory = InvType;
            this.PriorityId = PriorityID;
            this.CodeName = CodeName;
            this.RealName = RealName;
            this.PluralName = PluralName;
            this.DisplayCharacter = DisplayChar;
            this.IsGroupable = true;
            this.IsAssigned = Assigned;
            this.AppearancePct = AppearancePct;

            // If it's a weapon, it's wieldable.
            this.IsWieldable = (InvType == InvCategory.Weapon);

            // If the two names are the same or the type is greater than 6, it's identified.
            this.IsIdentified = (this.RealName == this.CodeName || (int)this.ItemCategory > 5);
        }
        /// <summary>
        /// For every inventory template that is marked as non-identified, select a random
        /// code name from the same category and then remove it from the list.
        /// </summary>
        public void InitializeInventory()
        {
            List<Tuple<InvCategory, string>> names = new List<Tuple<InvCategory, string>>();
            Tuple<InvCategory, string> code;

            // Iterate through template collection and assign code names.

            foreach (Inventory item in invItems)
            {
                if (!item.IsIdentified)
                {
                    names = CodeNames.Where(c => c.Item1 == item.ItemCategory).ToList();
                    if (names.Count > 0)
                    {
                        code = names[rand.Next(0, names.Count)];
                        item.CodeName = code.Item2;
                        CodeNames.Remove(code);
                    }
                }
            }
        }
        /// <summary>
        /// Instantiate code names list
        /// </summary>
        private void LoadCodeNames()
        {
            this.CodeNames =
            new List<Tuple<InvCategory, string>>()
            {
                new Tuple<InvCategory, string>(InvCategory.Ring, "agate"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "adamite"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "amethyst"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "beryl"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "bloodstone"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "carnelian"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "diamond"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "emerald"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "garnet"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "iolite"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "jade"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "lapi-lazuli"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "moonstone"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "onyx"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "opal"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "pearl"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "sapphire"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "stibotantalite"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "tiger-eye"),
                new Tuple<InvCategory, string>(InvCategory.Ring, "turquoise"),
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Forsan et haec olim meminisse iuvabit."), // Perhaps even these things will be pleasing to remember one day.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Ab antiquo"), // From antiquity
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Acta deos numquam mortalia fallunt."), // Mortal deeds never deceive the gods.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Ad astra per aspera"), // To the stars through difficulties
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Aquila non capit muscas."), // The eagle does not catch flies
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Audentes fortuna iuvat."), // Fortune favors the brave.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Audi, vide, tace."), // Hear, see, be silent.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Aurum potestas est."),  // Gold is power.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Carpe noctem."), // Seize the night.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Charta pardonationis utlagariae"), // A letter of pardon for the outlaw.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Comedamus et bibamus, cras enim moriemur."), // Let us eat and drink for tomorrow we die.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Cuncti adsint meritaeque expectent praemia palmae."), // Let all come who by merit deserve the most reward
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Defendit numerus"), // Safety in numbers.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Descensus in cuniculi cavum"), // The descent into the cave of the rabbit (Down the rabbit hole)
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Dies tenebrosa sicut nox"),  // A day as dark as night.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Ducunt volentem fata, nolentem trahunt."), // Fates lead the willing, drag the unwilling.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Troglodytae dormientes titillari non debent."), // Sleeping trolls should not be tickled.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Dum vivimus, vivamus."),  // While we live, let us live.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Heu, fugaces labuntur horae!"), // Alas the fleeting hours slip away.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Errare humanum est."), // To err is human.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Videre nec videre."), // To see and not be seen.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Experientia docet."),  // Experience teaches.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Faber est suae quisque fortunae."), // every man is the artisan of his own fortune
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Fac fortia et patere."), // Do brave deeds and endure.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Factum fieri infectum non potest."), // It is impossible for a deed to be undone.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Fortis cadere, cedere non potest."), // The brave may fall but cannot yield.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Hic sunt monstra."), // Here there be monsters.
                new Tuple<InvCategory, string>(InvCategory.Scroll, "Ignis aurum probat."), // Fire tests gold.
                new Tuple<InvCategory, string>(InvCategory.Wand, "copper"),
                new Tuple<InvCategory, string>(InvCategory.Wand, "gold"),
                new Tuple<InvCategory, string>(InvCategory.Wand, "iron"),
                new Tuple<InvCategory, string>(InvCategory.Wand, "nickel"),
                new Tuple<InvCategory, string>(InvCategory.Wand, "silver"),
                new Tuple<InvCategory, string>(InvCategory.Wand, "titanium"),
                new Tuple<InvCategory, string>(InvCategory.Wand, "steel"),
                new Tuple<InvCategory, string>(InvCategory.Wand, "aluminum"),
                new Tuple<InvCategory, string>(InvCategory.Wand, "brass"),
                new Tuple<InvCategory, string>(InvCategory.Wand, "bronze"),
                new Tuple<InvCategory, string>(InvCategory.Wand, "glass"),
                new Tuple<InvCategory, string>(InvCategory.Wand, "granite"),
                new Tuple<InvCategory, string>(InvCategory.Wand, "platinum"),
                new Tuple<InvCategory, string>(InvCategory.Staff, "birch"),
                new Tuple<InvCategory, string>(InvCategory.Staff, "cedar"),
                new Tuple<InvCategory, string>(InvCategory.Staff, "elm"),
                new Tuple<InvCategory, string>(InvCategory.Staff, "maple"),
                new Tuple<InvCategory, string>(InvCategory.Staff, "redwood"),
                new Tuple<InvCategory, string>(InvCategory.Staff, "teak"),
                new Tuple<InvCategory, string>(InvCategory.Staff, "walnut"),
                new Tuple<InvCategory, string>(InvCategory.Staff, "pine"),
                new Tuple<InvCategory, string>(InvCategory.Staff, "oak"),
                new Tuple<InvCategory, string>(InvCategory.Staff, "dogwood"),
                new Tuple<InvCategory, string>(InvCategory.Staff, "fir"),
                new Tuple<InvCategory, string>(InvCategory.Staff, "acacia"),
                new Tuple<InvCategory, string>(InvCategory.Potion, "crimson"),
                new Tuple<InvCategory, string>(InvCategory.Potion, "blue"),
                new Tuple<InvCategory, string>(InvCategory.Potion, "green"),
                new Tuple<InvCategory, string>(InvCategory.Potion, "brown"),
                new Tuple<InvCategory, string>(InvCategory.Potion, "orange"),
                new Tuple<InvCategory, string>(InvCategory.Potion, "jade"),
                new Tuple<InvCategory, string>(InvCategory.Potion, "pale"),
                new Tuple<InvCategory, string>(InvCategory.Potion, "electric blue"),
                new Tuple<InvCategory, string>(InvCategory.Potion, "pink"),
                new Tuple<InvCategory, string>(InvCategory.Potion, "clear"),
                new Tuple<InvCategory, string>(InvCategory.Potion, "black"),
            };
        }

        /// <summary>
        /// Load inventory items into this instance of the class. PriorityID and names values MUST BE UNIQUE.
        /// These will be used to identify the item elsewhere and to order the items in the inventory listing.
        /// </summary>
        private void LoadInventory()
        {
            this.invItems = new List<Inventory>()
            {
                new Inventory(InvCategory.Food, 1, "some food", "some food", "rations of food", new MapGlyph('♣', Color.Red, Color.Black), 25, true),
                new Inventory(InvCategory.Food, 2, "a mango", "a mango", "mangoes", new MapGlyph('♣', Color.Red, Color.Black), 20, false),
                new Inventory(InvCategory.Scroll, 3, "", "Identify", "Identify", false, true, false, false, false, false, 0, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('♪', Color.Blue, Color.Black), null ),
                new Inventory(InvCategory.Scroll, 4, "", "Magic Mapping", "Magic Mapping", false, true, false, false, false, false, 0, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('♪', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Scroll, 5, "", "Enchant Armor", "Enchant Armor", false, true, false, false, false, false, 0, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('♪', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Scroll, 6, "", "Enchant Weapon", "Enchant Weapon", false, true, false, false, false, false, 0, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('♪', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Scroll, 7, "", "Food Detection", "Food Detection", false, true, false, false, false, false, 0, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('♪', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Scroll, 8, "", "Light", "Light", false, true, false, false, false,  false,0, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('♪', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Scroll, 9, "", "Confuse Monster", "Confuse Monster", false, true, false, false, false, false, 0, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('♪', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Scroll, 10, "", "Remove Curse", "Remove Curse", false, true, false, false, false, false, 0, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('♪', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Scroll, 11, "", "Sleep", "Sleep", false, true, false, false, false, false, 0, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('♪', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Scroll, 12, "", "Teleportation", "Teleportation", false, true, false, false, false, false, 0, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('♪', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Scroll, 13, "", "Aggravate Monsters", "Aggravate Monsters", false, true, false, false, false, false, 0, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('♪', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Scroll, 14, "", "Create Monster", "Create Monster", false, true, false, false, false,  false,0, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('♪', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Scroll, 15, "", "Gold Detection", "Gold Detection", false, true, false, false, false, false, 0, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('♪', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Scroll, 16, "", "Hold Monsters", "Hold Monsters", false, true, false, false, false, false, 0, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('♪', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Scroll, 17, "", "Protect Armor", "Protect Armor", false, true, false, false, false, false, 0, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('♪', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Scroll, 18, "", "Clear Monsters", "Clear Monsters", false, true, false, false, false, false, 0, 0, 0, 0, 0, 0, 0, 85, new MapGlyph('♪', Color.HotPink, Color.Black), null),
                new Inventory(InvCategory.Scroll, 19, "", "Blank Paper", "Blank Paper", false, true, false, false, false, false, 0, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('♪', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Armor, 20, "studded leather armor", "studded leather armor", "studded leather armor", false, false, true, false, false,  false,3, 1, 0, 0, 0, 0, 0, 15, new MapGlyph('◘', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Armor, 21, "leather armor", "leather armor", "leather armor", false, false, false, false, false,  false,2, 1, 0, 0, 0, 0, 0, 20, new MapGlyph('◘', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Armor, 22, "ring mail", "ring mail", "ring mail", false, false, false, false, false,  false,3, 0, 0, 0, 0, 0, 0, 15, new MapGlyph('◘', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Armor, 23, "scale mail", "scale mail", "scale mail", false, false, false, false, false,  false,4, 0, 0, 0, 0, 0, 0, 13, new MapGlyph('◘', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Armor, 24, "chain mail", "chain mail", "chain mail", false, false, false, false, false,  false,5, 0, 0, 0, 0, 0, 0, 12, new MapGlyph('◘', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Armor, 25, "splint mail", "splint mail", "splint mail", false, false, false, false, false,  false,6, 0, 0, 0, 0, 0, 0, 10, new MapGlyph('◘', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Armor, 26, "banded mail", "banded mail", "banded mail", false, false, false, false, false,  false,6, 0, 0, 0, 0, 0, 0, 10, new MapGlyph('◘', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Armor, 27, "plate mail", "plate mail", "plate mail", false, false, false, false, false,  false,7, 0, 0, 0, 0, 0, 0, 5, new MapGlyph('◘', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Weapon, 28, "mace", "mace", "mace", false, false, true, true, false,  false,0, 0, 1, 1, 2, 8, -3, 10, new MapGlyph('↑', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Weapon, 29, "short bow", "short bow", "short bow", false, false, true, true, false, false, 0, 0, 0, 1, 1, 1, 0, 10, new MapGlyph('↑', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Weapon, 30, "crossbow", "crossbow", "crossbow", false, false, false, true, false, false, 0, 0, 0, 1, 1, 1, 0, 10, new MapGlyph('↑', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Weapon, 31, "dagger", "dagger", "dagger", false, false, false, true, false,  false,0, 0, 0, 1, 1, 4, 2, 10, new MapGlyph('↑', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Weapon, 32, "long sword", "long sword", "long sword", false, false, false, true, false,  false,0, 0, 0, 1, 3, 12, -10, 10, new MapGlyph('↑', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Weapon, 33, "spear", "spear", "spear", false, false, false, true, false,  false,0, 0, 0, 1, 1, 8, -2, 10, new MapGlyph('↑', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Weapon, 34, "two-handed sword", "two-handed sword", "two-handed sword", false, false, false, true, false,  false,0, 0, 0, 1, 4, 16, -14, 10, new MapGlyph('↑', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Ammunition, 35, "arrow", "arrow", "arrows", false, true, true, true, false,  false,0, 0, 0, 0, 1, 1, 4, 10, new MapGlyph('↑', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Ammunition, 36, "crossbow bolt", "crossbow bolt", "crossbow bolts", false, true, false, true, false, false, 0, 0, 0, 0, 1,2, 8, 10, new MapGlyph('↑', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Ammunition, 37, "dart", "dart", "darts", false, true, false, true, false,  false,0, 0, 0, 0, 1, 1, 2, 10, new MapGlyph('↑', Color.Blue, Color.Black), null),
                new Inventory(InvCategory.Amulet, 38, "The Amulet", "The Amulet", "The Amulet", true, false, false, false, false,  false,0, 0, 0, 0, 0, 0, 0, 0, new MapGlyph(MapLevel.AMULET.DisplayChar, Color.Yellow, Color.Black), null),
                new Inventory(InvCategory.Gold, 39, "gold", "gold", "gold", true, true, false, false, false,  false,0, 0, 0, 0, 0, 0, 0, 25, new MapGlyph('*', Color.Gold, Color.Black), null)
            };
        }

        /// <summary>
        /// Generates grouped inventory listing for inventory display screen.
        /// </summary>
        /// <param name="PlayerInventory"></param>
        /// <returns></returns>
        public List<InventoryLine> InventoryDisplay(List<Inventory> PlayerInventory)
        {
            char charID = 'a';

            // Get the player's current inventory in a grouped format.

            List<InventoryLine> lines = new List<InventoryLine>();

            // Get groupable inventory.
            var groupedInventory =
                (from invEntry in PlayerInventory
                    where invEntry.IsGroupable && invEntry.ItemCategory !=  InvCategory.Gold
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
        public string ListingDescription(int Number, Inventory Item)
        {
            // Single function to create inventory listing description for item and 
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
                    retValue = Number == 1 ? Game.AddEnglishArticle(Item.RealName) : Number.ToString() + " " + Item.PluralName;
                    break;
                case InvCategory.Weapon:
                    retValue = Game.AddEnglishArticle(Item.RealName);
                    break;
                case InvCategory.Ring:
                case InvCategory.Potion:
                case InvCategory.Wand:
                case InvCategory.Staff:
                    if (Item.IsIdentified)
                        retValue = Number == 1 ? "a " + Item.ItemCategory.ToString().ToLower() + " of " + Item.RealName
                            : Number.ToString() + " " + Item.ItemCategory.ToString().ToLower() + "s of " + Item.RealName;
                    else
                        retValue = Number == 1 ? "a " + Item.CodeName + " " + Item.ItemCategory.ToString().ToLower()
                            : Number.ToString() + " " + Item.CodeName + " " + Item.ItemCategory.ToString().ToLower() + "s";
                    break;
                case InvCategory.Scroll:
                    if (Item.IsIdentified)
                        retValue = Number == 1 ? "a " + Item.ItemCategory.ToString().ToLower() + " of " + Item.RealName 
                            : Number.ToString() + " " + Item.ItemCategory.ToString().ToLower() + "s of " + Item.RealName;
                    else
                        retValue = Number == 1 ? "a " + Item.ItemCategory.ToString().ToLower() + " called \"" + Item.CodeName + "\""
                            : Number.ToString() + " " + Item.ItemCategory.ToString().ToLower() + "s called \"" + Item.CodeName + "\"";
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
        public Inventory? GetInventoryItem(string ItemName)
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
        public Inventory GetInventoryItem(InvCategory InvType, MapSpace Location)
        {
            Inventory returnVal;

            // Get a random item from a specific inventory category.
            List<Inventory> invSelect = (from Inventory item in InventoryItems
                                            where item.ItemCategory == InvType
                                            select item).ToList();

            // Clone a new object from template.
            returnVal = new Inventory(invSelect[rand.Next(invSelect.Count)]);
            returnVal.Location = Location;

            return returnVal;
        }

        /// <summary>
        /// Generates random item from inventory template list.
        /// </summary>
        /// <returns></returns>
        public Inventory GetInventoryItem(MapSpace Location)
        {
            Inventory returnVal;

            // Clone a new object from template.  Exclude gold and amulet.
            List<Inventory> invSelect = (from Inventory item in InventoryItems
                                         where item.ItemCategory != InvCategory.Gold 
                                         && item.ItemCategory != InvCategory.Amulet
                                         select item).ToList();

            returnVal = new Inventory(invSelect[rand.Next(invSelect.Count)]);
            returnVal.Location = Location;

            return returnVal;
        }

        /// <summary>
        /// Get the list of inventory assigned at the start of the game.
        /// </summary>
        /// <returns></returns>
        public List<Inventory>? GetAssignedInventory()
        {

            List<Inventory> retList = (from Inventory item in this.InventoryItems
                                       where item.IsAssigned
                                       select item).ToList();

            // Clone a new object from template.
            if (retList.Count > 0) return retList; else return null;
        }


    }
    #endregion

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
