namespace RogueGame
{
    internal class Character
    {
        /// <summary>
        /// Current HP
        /// </summary>
        public int CurrentHP { get { return MaxHP - HPDamage; } }
        /// <summary>
        /// Maximum current hit points
        /// </summary>
        public int MaxHP { get; set; }
        /// <summary>
        /// Current damage in hit points
        /// </summary>
        public int HPDamage { get; set; } = 0;
        /// <summary>
        /// Current gold
        /// </summary>        
        public int Gold { get; set; }
        /// <summary>
        /// Confused - player moves erratically.
        /// </summary>
        public int Confused { get; set; } = 0;
        /// <summary>
        /// Paralysis, frozen by ice monster, etc..
        /// </summary>
        public int Immobile { get; set; } = 0;
        /// <summary>
        /// Blind from potion, etc..
        /// </summary>
        public int Blind { get; set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        public int Floating { get; set; } = 0;
        /// <summary>
        /// Character's relative speed to other characters. Anything above 1.0
        /// gives character a chance for an extra move. ExpTurn of 0 is permanent.
        /// </summary>
        public (int Speed, int ExpTurn) RelativeSpeed { get; set; } = (1, 0);
        /// <summary>
        /// Main inventory list.
        /// </summary>
        public List<Inventory> CharacterInventory { get; set; }
        /// <summary>
        /// Name provided by player
        /// </summary>
        public string CharacterName { get; set; }
        /// <summary>
        /// Current map space occupied
        /// </summary>
        public MapSpace? Location { get; set; }


        #region Procedures
        /// <summary>
        /// Search the player's inventory for a specific item.
        /// </summary>
        /// <param name="ItemName">Real name of item.</param>
        /// <returns></returns>
        public Inventory? SearchInventory(string ItemName)
        {
            return (from Inventory item in CharacterInventory
                    where item.RealName == ItemName
                    select item).FirstOrDefault();
        }
        /// <summary>
        /// Get first inventory item of a specific category.
        /// </summary>
        /// <param name="Category">Specific member of InvCategory enumeration</param>
        /// <returns></returns>
        public Inventory? SearchInventory(Inventory.InvCategory Category)
        {
            return (from Inventory item in CharacterInventory
                    where item.ItemCategory == Category
                    select item).FirstOrDefault();
        }
        #endregion
    }
}
