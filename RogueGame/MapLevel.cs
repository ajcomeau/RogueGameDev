using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using static RogueGame.MapLevel;

namespace RogueGame{

    internal class MapLevel
    {
        /* See https://www.andrewcomeau.com/programming/testing-mapping-review/ for an general overview of the MapLevel class */

        #region Supporting Lists
        /// <summary>
        /// Horizontal wall piece
        /// </summary>
        public static readonly MapGlyph HORIZONTAL = new MapGlyph('═', Color.SaddleBrown, Color.Black);      // Unicode symbols can be copy-pasted from https://www.w3.org/TR/xml-entity-names/025.html.
        /// <summary>
        /// Vertical wall piece.
        /// </summary>
        public static readonly MapGlyph VERTICAL = new MapGlyph('║', Color.SaddleBrown, Color.Black);
        /// <summary>
        /// Northwest room corner
        /// </summary>
        public static readonly MapGlyph CORNER_NW = new MapGlyph('╔', Color.SaddleBrown, Color.Black);
        /// <summary>
        /// Southeast room corner
        /// </summary>
        public static readonly MapGlyph CORNER_SE = new MapGlyph('╝', Color.SaddleBrown, Color.Black);
        /// <summary>
        /// Northeast room corner
        /// </summary>
        public static readonly MapGlyph CORNER_NE = new MapGlyph('╗', Color.SaddleBrown, Color.Black);
        /// <summary>
        /// Southwest room corner
        /// </summary>
        public static readonly MapGlyph CORNER_SW = new MapGlyph('╚', Color.SaddleBrown, Color.Black);
        /// <summary>
        /// Room interior space
        /// </summary>
        public static readonly MapGlyph ROOM_INT = new MapGlyph('·', Color.Gray, Color.Black);
        /// <summary>
        /// Room door piece
        /// </summary>
        public static readonly MapGlyph ROOM_DOOR = new MapGlyph('╬', Color.SaddleBrown, Color.Black);
        /// <summary>
        /// Hallway space
        /// </summary>
        public static readonly MapGlyph HALLWAY = new MapGlyph('▒', Color.White, Color.Black);
        /// <summary>
        /// Stairway symbol
        /// </summary>
        public static readonly MapGlyph STAIRWAY = new MapGlyph('≣', Color.Black, Color.Green);
        /// <summary>
        /// Gold map symbol
        /// </summary>
        public static readonly MapGlyph GOLD = new MapGlyph('*', Color.LightYellow, Color.Black);
        /// <summary>
        /// Amulet of Yendor symbol
        /// </summary>
        public static readonly MapGlyph AMULET = new MapGlyph('♀', Color.Yellow, Color.Black);
        /// <summary>
        /// Empty map space
        /// </summary>
        public static readonly MapGlyph EMPTY = new MapGlyph(' ', Color.Black, Color.Black);
        /// <summary>
        /// Private array of MapSpace objects to hold map definitions.
        /// </summary>
        private MapSpace[,] levelMap = new MapSpace[80, 25]; // Internal game map.
        /// <summary>
        /// Public array of MapGlyph objects to define user's view of finalized map.
        /// </summary>
        public MapGlyph[,] DisplayMap = new MapGlyph[80, 25]; // Map to be shown to user.
        /// <summary>
        /// List of characters that will be marked as Lighted during map discovery.
        /// </summary>
        private static List<char> MapDiscoveryGlyphList = new List<char>(){HORIZONTAL.DisplayChar, VERTICAL.DisplayChar,
            CORNER_NW.DisplayChar, CORNER_SE.DisplayChar, CORNER_NE.DisplayChar, CORNER_SW.DisplayChar,
            ROOM_DOOR.DisplayChar, HALLWAY.DisplayChar, STAIRWAY.DisplayChar};
        /// <summary>
        /// List of characters that occur inside a room.
        /// </summary>
        private static List<char> RoomInteriorGlyphList = new List<char>(){ROOM_DOOR.DisplayChar, ROOM_INT.DisplayChar,
            STAIRWAY.DisplayChar };
        /// <summary>
        /// List of characters a player or monster can move onto.
        /// </summary>
        public static List<char> InhabitableSpacesGlyphList = new List<char>(){ROOM_INT.DisplayChar, STAIRWAY.DisplayChar,
            ROOM_DOOR.DisplayChar, HALLWAY.DisplayChar };
        /// <summary>
        /// List of characters that can be moved past on Fast Play.
        /// </summary>
        private static List<char> PassableSpacesGlyphList = new List<char>(){ROOM_INT.DisplayChar, HORIZONTAL.DisplayChar,
            VERTICAL.DisplayChar, CORNER_NE.DisplayChar, CORNER_NW.DisplayChar, CORNER_SE.DisplayChar,
            CORNER_SW.DisplayChar, HALLWAY.DisplayChar, EMPTY.DisplayChar};
        /// <summary>
        /// Dictionary to hold hallway endings during map generation.
        /// </summary>
        private Dictionary<MapSpace, Direction> hallwayDeadEnds = new Dictionary<MapSpace, Direction>();
        /// <summary>
        /// List of monsters on current map.
        /// </summary>
        public List<Monster> ActiveMonsters = new List<Monster>();
        /// <summary>
        /// List of inventory on current map, including gold.
        /// </summary>
        public List<Inventory> MapInventory = new List<Inventory>();
        #endregion

        #region Constants, Properties
        /// <summary>
        /// Enumeration used to establish relative directions.
        /// </summary>
        public enum Direction
        {
            None = 0,
            North = 1,            
            East = 2,
            South = -1,
            West = -2
        }
        /// <summary>
        /// Max gold amount per stash.
        /// </summary>
        public const int MIN_GOLD_AMT = 10;
        /// <summary>
        /// Max gold amount per stash.
        /// </summary>
        public const int MAX_GOLD_AMT = 125;
        /// <summary>
        /// Probability of a monster appearing at any given point.
        /// </summary>
        public const int SPAWN_MONSTER = 90;
        /// <summary>
        /// Width of region holding single room.
        /// </summary>
        private const int REGION_WD = 26;
        /// <summary>
        /// Height of region holding single room.
        /// </summary>
        private const int REGION_HT = 8;
        /// <summary>
        /// Max width of map display.
        /// </summary>
        private const int MAP_WD = 78;
        /// <summary>
        /// Max height of map display.
        /// </summary>
        private const int MAP_HT = 24;
        /// <summary>
        /// Room width based on screen width of 80, 78 allowed
        /// </summary>
        private const int MAX_ROOM_WT = 24;
        /// <summary>
        /// Room height based on screen width of 80, 78 allowed
        /// </summary>
        private const int MAX_ROOM_HT = 6;          
        /// <summary>
        /// Minimum exterior room width
        /// </summary>
        private const int MIN_ROOM_WT = 4;          
        /// <summary>
        /// Minimum exterior room height
        /// </summary>
        private const int MIN_ROOM_HT = 4;
        /// <summary>
        /// Probability that room will be created for one region.
        /// </summary>
        private const int ROOM_CREATE_PCT = 95; 
        /// <summary>
        /// Probability that room wall will contain exit.
        /// </summary>
        private const int ROOM_EXIT_PCT = 90; 
        /// <summary>
        /// Probability that doorway will be hidden.
        /// </summary>
        private const int HIDDEN_EXIT_PCT = 25;
        /// <summary>
        /// Probablility that room will be lighted.
        /// </summary>
        private const int ROOM_LIGHTED = 75; 
        /// <summary>
        /// Probability that a room will have gold.
        /// </summary>
        private const int ROOM_GOLD_PCT = 51; 
        /// <summary>
        /// Maximum inventory on a level.
        /// </summary>
        private const int MAX_INVENTORY = 20;
        /// <summary>
        /// Maximum number of initial monsters on a level.
        /// </summary>
        private const int MAX_INIT_MONSTERS = 15;
        /// <summary>
        /// Random number generator
        /// </summary>
        private static Random rand = new Random();
        /// <summary>
        /// Current game level
        /// </summary>
        private int CurrentLevel { get; set; }
        /// <summary>
        /// Reference to current player to get location and anything else needed.
        /// </summary>
        private Player CurrentPlayer { get; }
        /// <summary>
        /// Class inventory object to be used as reference to game inventory instance.
        /// </summary>
        private Inventory GameInventory { get; }
        #endregion
        /// <summary>
        /// Constructor - generate a new map for this level.
        /// </summary>
        /// <param name="levelNumber">Map level</param>
        /// <param name="currentPlayer">Current Player object</param>
        /// <param name="GameInventory">Inventory dummy object reference for method access</param>
        public MapLevel(int levelNumber, Player currentPlayer, Inventory GameInventory)
        {
            this.CurrentLevel = levelNumber;
            this.CurrentPlayer = currentPlayer;
            this.GameInventory = GameInventory;

            do
            {
                MapGeneration();
            } while (!VerifyMap());
            
            this.CurrentPlayer.Location = GetOpenSpace(false);
        }
        /// <summary>
        /// Verify that the generate map is free of isolated rooms or sections.
        /// </summary>
        /// <returns>True / False based on validity of map.</returns>
        private bool VerifyMap()
        {
            bool retValue = true;
            List<char> dirCheck = new List<char>();

            // Check horizontal for blank rows with no hallways. Top and bottom might be legitimately blank
            // so just check a portion of the map.

            for (int y = REGION_HT - MIN_ROOM_HT; y < (REGION_HT * 2) + MIN_ROOM_HT; y++)
            {
                dirCheck = (from MapSpace space in levelMap
                                where space.X <= MAP_WD
                                && space.Y == y
                                select space).Select(c => c.MapCharacter.DisplayChar).Distinct().ToList();

                retValue = dirCheck.Count > 1;
                if (!retValue) { break; }
            }

            // Check vertical.

            if (retValue)
            {
                for (int x = REGION_WD - MIN_ROOM_WT; x < (REGION_WD * 2) + MIN_ROOM_WT; x++)
                {
                    dirCheck = (from MapSpace space in levelMap
                                where space.Y <= MAP_HT
                                && space.X == x
                                select space).ToList().Select(c => c.MapCharacter.DisplayChar).Distinct().ToList();

                    retValue = dirCheck.Count > 1;
                    if (!retValue) { break; }
                }
            }

            // Verify there's a stairway

            if(retValue)
                retValue = (from MapSpace space in levelMap
                            where space.MapCharacter.DisplayChar == STAIRWAY.DisplayChar
                            select space).ToList().Count > 0;

            // On the game's final level, verify the amulet is there.

            if (retValue && CurrentLevel == Game.MAX_LEVEL)
                retValue = (from MapSpace space in levelMap
                            where space.MapCharacter.DisplayChar == AMULET.DisplayChar
                            select space).ToList().Count > 0;

            return retValue;
        }
        /// <summary>
        /// Primary map generation procedure
        /// </summary>
        private void MapGeneration()
        {
            // Screen is divided into nine cell regions and a room is randomly generated in each.
            // Room exterior must be at least four spaces in each direction but not more than the
            // size of its cell region, minus one space, to allow for hallways between rooms.
            
            int roomWidth = 0, roomHeight = 0, roomAnchorX = 0, roomAnchorY = 0;
            MapSpace? stairway, amulet;

            // Clear map by creating new array of map spaces.
            levelMap = new MapSpace[80, 25];

            // Define the map left to right, top to bottom.
            // Increment the count based on a third of the way in each direction.
            // First row and first column of array are skipped so everything is 1 based.
            for (int y = 1; y < 18; y += REGION_HT)
            {
                for (int x = 1; x < 54; x += REGION_WD)
                {
                    if (rand.Next(1, 101) <= ROOM_CREATE_PCT)
                    {
                        // Room size
                        roomHeight = rand.Next(MIN_ROOM_HT, MAX_ROOM_HT + 1);
                        roomWidth = rand.Next(MIN_ROOM_WT, MAX_ROOM_WT + 1);

                        // Center room in region
                        roomAnchorY = (int)((REGION_HT - roomHeight) / 2) + y;
                        roomAnchorX = (int)((REGION_WD - roomWidth) / 2) + x;

                        // Create room - let's section this out in its own procedure
                        RoomGeneration(roomAnchorX, roomAnchorY, roomWidth, roomHeight);
                    }
                }
            }

            // After the rooms are generated, fill in the
            // blanks for the remaining cells.
            for (int y = 0; y <= levelMap.GetUpperBound(1); y++)
            {
                for (int x = 0; x <= levelMap.GetUpperBound(0); x++)
                {
                    if (levelMap[x, y] is null)
                        levelMap[x, y] = new MapSpace(EMPTY, false, false, x, y);
                }
            }

            // Create hallways 
            HallwayGeneration();

            // Add a random number of monsters to start.
            AddMonsters(rand.Next(MAX_INIT_MONSTERS));

            // Add a random number of inventory items.
            AddInventory(rand.Next(MAX_INVENTORY));

            // Add stairway
            stairway = GetOpenSpace(false);

            if(stairway != null)
                levelMap[stairway.X, stairway.Y] = new MapSpace(STAIRWAY, stairway.X, stairway.Y);

            // Add Amulet to final level.
            if (CurrentLevel == Game.MAX_LEVEL)
            {
                amulet = GetOpenSpace(false);
                if (amulet != null)
                    MapInventory.Add(GameInventory.GetInventoryItem(Inventory.InvCategory.Amulet, amulet));
            }           

        }
        /// <summary>
        /// Add a specific number of monsters to the map.
        /// </summary>
        /// <param name="Number">Number of monsters to add.</param>
        /// <param name="spaces">List of MapSpace objects in which monsters may be placed.</param>
        public void AddMonsters(int Number, List<MapSpace>? spaces = null)
        {
            Monster? spawned;
            MapSpace? itemSpace;
            MapSpace? playerSpace = CurrentPlayer.Location;
            int startingCount = ActiveMonsters.Count();
            int maxMonster = 50, maxSpace = 50;

            // Pick random monsters until its probability of appearing is 
            // within the random limit generated.
            while (ActiveMonsters.Count < startingCount + Number && --maxMonster > 0)
            {
                do
                {
                    spawned = Monster.SpawnMonster(CurrentLevel);
                } while (spawned != null && rand.Next(1, 101) <= spawned.AppearancePct);

                // Place monster on map. If the spaces are specified, then make sure
                // it's not in the same region as the player.
                do
                {
                    itemSpace = GetOpenSpace(true, spaces);

                    if (itemSpace != null)
                    {
                        if (spaces == null && playerSpace != null &&
                                GetRegionNumber(itemSpace.X, itemSpace.Y) == GetRegionNumber(playerSpace.X, playerSpace.Y))
                            itemSpace = null;
                    }

                } while (itemSpace == null & --maxSpace > 0);
                
                if (spawned != null && itemSpace != null)
                {                    
                    spawned.Location = itemSpace;
                    ActiveMonsters.Add(spawned);
                }
            }
        }
        /// <summary>
        /// Add specified number of inventory items to map.
        /// </summary>
        /// <param name="Number">Number of Inventory items to add.</param>
        public void AddInventory(int Number)
        {            
            Inventory invItem;
            MapSpace? itemSpace;
            int startingCount = MapInventory.Count;
            int maxAttempts = 100;

            // Add up to the number of specified inventory items.
            while (MapInventory.Count < startingCount + Number && --maxAttempts > 0) 
            {
                do
                    itemSpace = GetOpenSpace(false);
                while (itemSpace == null);

                do
                    invItem = GameInventory.GetInventoryItem(itemSpace);
                while (invItem != null
                        && rand.Next(1, 101) >= invItem.AppearancePct);

                if (itemSpace != null && invItem != null)
                {
                    // For ammunition that's groupable, decide how many items are in the batch.
                    if (invItem.ItemCategory == Inventory.InvCategory.Ammunition
                        && invItem.IsGroupable)
                        invItem.Amount = rand.Next(1, Inventory.MAX_AMMO_BATCH + 1);

                    // Update the space and increment the count.
                    MapInventory.Add(invItem);
                }
            }
        }
        /// <summary>
        /// Add a specific item to the map at a specific location.
        /// </summary>
        /// <param name="Item">Inventory object</param>
        /// <param name="Location">MapSpace location for inventory.</param>
        /// <param name="Clone">True value copies the inventory object.</param>
        public void AddInventory(Inventory Item, MapSpace Location, Boolean Clone)
        {
            Inventory invItem;

            if (Clone)
                invItem = GameInventory.GetInventoryItem(Item.RealName)!;
            else
                invItem = Item;

            invItem.Location = new MapSpace(Location.MapCharacter, Location);

            MapInventory.Add(invItem);

        }
        /// <summary>
        /// Create room on map based on inputs
        /// </summary>
        /// <param name="westWallX">X-coord of northwest corner</param>
        /// <param name="northWallY">Y-coord of northwest corner</param>
        /// <param name="roomWidth">Width of room in spaces</param>
        /// <param name="roomHeight">Height of room in spaces</param>
        private void RoomGeneration(int WestWallX, int NorthWallY, int RoomWidth, int RoomHeight)
        {
            int eastWallX = WestWallX + RoomWidth;          // Calculate room east
            int southWallY = NorthWallY + RoomHeight;       // Calculate room south

            // Regions are defined 1 to 9, L to R, top to bottom.
            int regionNumber = GetRegionNumber(WestWallX, NorthWallY);
            int doorway = 0, doorCount = 0, openX, openY;

            bool searchRequired;

            // Create horizontal and vertical walls for room and fill interior spaces.
            for (int y = NorthWallY; y <= southWallY; y++)
            {
                for (int x = WestWallX; x <= eastWallX; x++)
                {
                    if (y == NorthWallY || y == southWallY)
                    {
                        levelMap[x, y] = new MapSpace(HORIZONTAL, false, false, x, y);
                    }
                    else if (x == WestWallX || x == eastWallX)
                    {
                        levelMap[x, y] = new MapSpace(VERTICAL, false, false, x, y);
                    }
                    else if (levelMap[x, y] == null)
                        levelMap[x, y] = new MapSpace(ROOM_INT, false, false, x, y);
                }
            }

            // Add doorways and initial hallways on room. Room walls facing the edges of the map do not get exits
            // so the ROOM_EXIT_PCT constant needs to be high to ensure that every room gets at least one and we
            // still might need to repeat the process anyway.
            while (doorCount == 0) { 
                if (regionNumber >= 4 && rand.Next(1, 101) <= ROOM_EXIT_PCT)  // North doorways
                {
                    doorway = rand.Next(WestWallX + 1, eastWallX);  // Random point on wall.
                    searchRequired = rand.Next(1, 101) <= HIDDEN_EXIT_PCT;  // Is the doorway hidden?
                    levelMap[doorway, NorthWallY] = new MapSpace(ROOM_DOOR, false, searchRequired, doorway, NorthWallY);
                    levelMap[doorway, NorthWallY].AltMapCharacter = searchRequired ? HORIZONTAL : null;
                    levelMap[doorway, NorthWallY - 1] = new MapSpace(HALLWAY, false, false, doorway, NorthWallY - 1);
                    hallwayDeadEnds.Add(levelMap[doorway, NorthWallY - 1], Direction.North);  // Add to dead ends list.
                    doorCount++;  // Increment the count of doors created.
                }

                if (regionNumber <= 6 && rand.Next(1, 101) <= ROOM_EXIT_PCT)  // South doorways
                {
                    doorway = rand.Next(WestWallX + 1, eastWallX);
                    searchRequired = rand.Next(1, 101) <= HIDDEN_EXIT_PCT;
                    levelMap[doorway, southWallY] = new MapSpace(ROOM_DOOR, false, searchRequired, doorway, southWallY);
                    levelMap[doorway, southWallY].AltMapCharacter = searchRequired ? HORIZONTAL : null;
                    levelMap[doorway, southWallY + 1] = new MapSpace(HALLWAY, false, false, doorway, southWallY + 1);
                    hallwayDeadEnds.Add(levelMap[doorway, southWallY + 1], Direction.South);
                    doorCount++;
                }

                if ("147258".Contains(regionNumber.ToString()) && rand.Next(1, 101) <= ROOM_EXIT_PCT)  // East doorways
                {
                    doorway = rand.Next(NorthWallY + 1, southWallY);
                    searchRequired = rand.Next(1, 101) <= HIDDEN_EXIT_PCT;
                    levelMap[eastWallX, doorway] = new MapSpace(ROOM_DOOR, false, searchRequired, eastWallX, doorway);
                    levelMap[eastWallX, doorway].AltMapCharacter = searchRequired ? VERTICAL : null;
                    levelMap[eastWallX + 1, doorway] = new MapSpace(HALLWAY, false, false, eastWallX + 1, doorway);
                    hallwayDeadEnds.Add(levelMap[eastWallX + 1, doorway], Direction.East);
                    doorCount++;
                }

                if ("258369".Contains(regionNumber.ToString()) && rand.Next(1, 101) <= ROOM_EXIT_PCT)  // West doorways
                {
                    doorway = rand.Next(NorthWallY + 1, southWallY);
                    searchRequired = rand.Next(1, 101) <= HIDDEN_EXIT_PCT;
                    levelMap[WestWallX, doorway] = new MapSpace(ROOM_DOOR, false, searchRequired, WestWallX, doorway);
                    levelMap[WestWallX, doorway].AltMapCharacter = searchRequired ? VERTICAL : null;
                    levelMap[WestWallX - 1, doorway] = new MapSpace(HALLWAY, false, false, WestWallX - 1, doorway);
                    hallwayDeadEnds.Add(levelMap[WestWallX - 1, doorway], Direction.West);
                    doorCount++;
                }
            }

            // Set the room corners.
            levelMap[WestWallX, NorthWallY] = new MapSpace(CORNER_NW, false, false, WestWallX, NorthWallY);
            levelMap[eastWallX, NorthWallY] = new MapSpace(CORNER_NE, false, false, eastWallX, NorthWallY);
            levelMap[WestWallX, southWallY] = new MapSpace(CORNER_SW, false, false, WestWallX, southWallY);
            levelMap[eastWallX, southWallY] = new MapSpace(CORNER_SE, false, false, eastWallX, southWallY);

            // Set starting point to northwest corner
            openX = WestWallX; openY = NorthWallY;

            // Evaluate room for a gold deposit
            if (rand.Next(1, 101) < ROOM_GOLD_PCT)
            {
                // Search the room randomly for an empty interior room space
                // and mark it as a gold deposit.
                while (PriorityChar(levelMap[openX, openY], true).DisplayChar != ROOM_INT.DisplayChar)
                {
                    openX = rand.Next(WestWallX + 1, eastWallX);
                    openY = rand.Next(NorthWallY + 1, southWallY);
                }

                MapInventory.Add(GameInventory.GetInventoryItem(Inventory.InvCategory.Gold, levelMap[openX, openY]));
            }
        }
        /// <summary>
        /// Create hallways between all the existing rooms.
        /// </summary>
        private void HallwayGeneration()
        {
            Direction hallDirection = Direction.None; Direction direction90; Direction direction270;
            MapSpace hallwaySpace, newSpace;
            Dictionary<Direction, MapSpace> adjacentChars;
            Dictionary<Direction, MapSpace> surroundingChars;
            bool hallwayDug = false, hallwayLimit = false;

            // Iterate through the list of hallway endings (deadends) until all are resolved one way or another.
            // Count backwards so we can remove processed items.

            while (hallwayDeadEnds.Count > 0)
            {
                for (int i = hallwayDeadEnds.Count - 1; i >= 0; i--)
                {
                    // Establish current space and three directions - forward and to the sides.
                    hallwaySpace = hallwayDeadEnds.ElementAt(i).Key;
                    hallDirection = hallwayDeadEnds.ElementAt(i).Value;
                    direction90 = GetDirection90(hallDirection);
                    direction270 = GetDirection270(hallDirection);
                    hallwayDug = false;
                    // Bugfix - hallways were being drawn to edge and this was causing a problem with
                    // SearchAdjacent(). This is part of the fix. 
                    hallwayLimit = hallwaySpace.X < 2 || hallwaySpace.Y < 2 || 
                        hallwaySpace.X > MAP_WD || hallwaySpace.Y > MAP_HT;
                    
                    // Look for distant hallways in three directions.  If one is found, connect to it.
                    if (hallDirection != Direction.None)
                    {
                        surroundingChars = SearchAllDirections(hallwaySpace.X, hallwaySpace.Y);

                        foreach (Direction direct in new List<Direction> { hallDirection, direction90, direction270 })
                        {
                            if ((surroundingChars[direct] != null &&
                                surroundingChars[direct].MapCharacter.DisplayChar == HALLWAY.DisplayChar))
                            {
                                DrawHallway(hallwaySpace, surroundingChars[direct], direct);
                                hallwayDug = true;
                            }
                        }

                        if (!hallwayDug && !hallwayLimit)
                        {
                            // If there's no hallway to connect to, just add another space where possible for the
                            // next iteration to pick up on.
                            adjacentChars = SearchAdjacent(EMPTY.DisplayChar, hallwaySpace.X, hallwaySpace.Y);
                            
                            foreach (Direction direct in new List<Direction>{ hallDirection, direction90, direction270})
                            {
                                if (adjacentChars.ContainsKey(direct))
                                {
                                    newSpace = new MapSpace(HALLWAY, adjacentChars[direct]);
                                    levelMap[adjacentChars[direct].X, adjacentChars[direct].Y] = newSpace;
                                    hallwayDeadEnds.Remove(hallwaySpace);
                                    hallwayDeadEnds.Add(newSpace, direct);
                                    break;
                                }
                            }
                            break;
                        }

                        hallwayDeadEnds.Remove(hallwaySpace);
                    }
                    else
                        hallwayDeadEnds.Remove(hallwaySpace);
                }
            }
        }        
        /// <summary>
        /// Draw a hallway between specified spaces.  Break off if another hallway
        /// is discovered to the side.
        /// </summary>
        /// <param name="start">Starting space</param>
        /// <param name="end">Ending space</param>
        /// <param name="hallDirection"></param>
        private void DrawHallway(MapSpace start, MapSpace end, Direction hallDirection)
        {
            switch (hallDirection) {
                case Direction.North:
                    for (int y = start.Y; y >= end.Y; y--)
                    {
                        levelMap[end.X, y] = new MapSpace(HALLWAY, end.X, y);
                        if (SearchAdjacent(HALLWAY.DisplayChar, end.X, y).Count > 1)
                            break;
                    }
                    break;
                case Direction.South:
                    for (int y = start.Y; y <= end.Y; y++)
                    {
                        levelMap[end.X, y] = new MapSpace(HALLWAY, end.X, y);
                        if (SearchAdjacent(HALLWAY.DisplayChar, end.X, y).Count > 1)
                            break;
                    }
                    break;
                case Direction.East:
                    for (int x = start.X; x <= end.X; x++)
                    {
                        levelMap[x, end.Y] = new MapSpace(HALLWAY, x, end.Y);
                        if (SearchAdjacent(HALLWAY.DisplayChar, x, end.Y).Count > 1)
                            break;
                    }
                    break;
                case Direction.West:
                    for (int x = start.X; x >= end.X; x--)
                    {
                        levelMap[x, end.Y] = new MapSpace(HALLWAY, x, end.Y);
                        if (SearchAdjacent(HALLWAY.DisplayChar, x, end.Y).Count > 1)
                            break;
                    }
                    break;
            }

        }        
        /// <summary>
        /// Raise the fog of war and hide the map contents.
        /// </summary>
        public void ShroudMap()
        {
            List<MapSpace> mapSpaces = (from MapSpace space in levelMap
                                            select space).ToList();

            // Iterate through MapSpaces
            foreach (MapSpace space in mapSpaces)
            {
                space.Discovered = false;
                space.Lighted = false;
            }
        }
        /// <summary>
        /// Return direction 90 degrees from original based on forward direction.
        /// </summary>
        /// <param name="startingDirection">Initial direction from which to calculate new direction.</param>
        /// <returns></returns>
        public Direction GetDirection90(Direction startingDirection)
        {
            Direction retValue = (Math.Abs((int)startingDirection) == 1) ? (Direction)2 : (Direction)1;
            return retValue;
        }
        /// <summary>
        /// Return direction 270 degrees from original (opposite of 90 degrees) based on forward direction.
        /// </summary>
        /// <param name="startingDirection">Initial direction from which to calculate new direction.</param>
        /// <returns></returns>
        public Direction GetDirection270(Direction startingDirection)
        {
            Direction retValue = (Math.Abs((int)startingDirection) == 1) ? (Direction)2 : (Direction)1;
            retValue = (Direction)((int)retValue * -1);
            return retValue;
        }
        /// <summary>
        /// Return direction 180 degrees from original (opposite) based on forward direction.
        /// </summary>
        /// <param name="startingDirection">Initial direction from which to calculate new direction.</param>
        /// <returns></returns>
        public Direction GetDirection180(Direction startingDirection)
        {
            return (Direction)((int)startingDirection * -1); 
        }
        /// <summary>
        /// Search for specific character in four directions around point for a specific character. 
        /// Return list of directions and characters found.
        /// </summary>
        /// <param name="character">Character to search for.</param>
        /// <param name="x">Starting X point</param>
        /// <param name="y">Starting Y point</param>
        /// <returns>Dictionary of directions and characters found.</returns>
        public Dictionary<Direction, MapSpace> SearchAdjacent(char character, int x, int y)
        {
            Dictionary<Direction, MapSpace> retValue = new Dictionary<Direction, MapSpace>();

            if (y - 1 >= 0 && levelMap[x, y - 1].MapCharacter.DisplayChar == character)  // North
                retValue.Add(Direction.North, levelMap[x, y - 1]);

            if (x + 1 <= MAP_WD && levelMap[x + 1, y].MapCharacter.DisplayChar == character) // East
                retValue.Add(Direction.East, levelMap[x + 1, y]);

            if (y + 1 <= MAP_HT && levelMap[x, y + 1].MapCharacter.DisplayChar == character)  // South
                retValue.Add(Direction.South, levelMap[x, y + 1]);

            if ((x - 1) >= 0 && levelMap[x - 1, y].MapCharacter.DisplayChar == character)  // West
                retValue.Add(Direction.West, levelMap[x - 1, y]);

            return retValue;

        }
        /// <summary>
        /// Search in four directions around point. Return list of directions and characters found.
        /// </summary>
        /// <param name="x">Starting X point</param>
        /// <param name="y">Starting Y point</param>
        /// <returns>Dictionary of directions and characters found.</returns>
        public Dictionary<Direction, MapSpace> SearchAdjacent(int x, int y)
        {
            // For each direction, add the existing mapspace if available.
            Dictionary<Direction, MapSpace> retValue = new Dictionary<Direction, MapSpace>();
         
            if (y - 1 >= 0)  // North
                retValue.Add(Direction.North, levelMap[x, y - 1]);

            if (x + 1 <= MAP_WD) // East
                retValue.Add(Direction.East, levelMap[x + 1, y]);

            if (y + 1 <= MAP_HT)  // South
                retValue.Add(Direction.South, levelMap[x, y + 1]);

            if ((x - 1) >= 0)  // West
                retValue.Add(Direction.West, levelMap[x - 1, y]);

            return retValue;
        }
        /// <summary>
        /// Look in all directions and return a Dictionary of the first non-space characters found.
        /// </summary>
        /// <param name="currentX">Starting X point</param>
        /// <param name="currentY">Starting Y point</param>
        /// <returns></returns>
        public Dictionary<Direction, MapSpace> SearchAllDirections(int currentX, int currentY)
        {
            Dictionary<Direction, MapSpace> retValue = new Dictionary<Direction, MapSpace>();

            retValue.Add(Direction.North, SearchDirection(Direction.North, currentX, currentY - 1)!);
            retValue.Add(Direction.South, SearchDirection(Direction.South, currentX, currentY + 1)!);
            retValue.Add(Direction.East, SearchDirection(Direction.East, currentX + 1, currentY)!);
            retValue.Add(Direction.West, SearchDirection(Direction.West, currentX - 1, currentY)!);

            return retValue;
        }
        /// <summary>
        /// Get the next non-space object found in a given direction. Return null if none is found.
        /// </summary>
        /// <param name="direction">Direction to search</param>
        /// <param name="startX">Starting X point</param>
        /// <param name="startY">Starting Y point</param>
        /// <returns></returns>
        public MapSpace? SearchDirection(Direction direction, int startX, int startY)
        {
            int currentX = startX, currentY = startY;
            MapSpace? retValue = null;

            currentY = (currentY > MAP_HT) ? MAP_HT : currentY;
            currentY = (currentY < 0) ? 0 : currentY;
            currentX = (currentX > MAP_WD) ? MAP_WD : currentX;
            currentX = (currentX < 0) ? 0 : currentX;

            switch (direction)
            {
                case Direction.North:
                    while (levelMap[currentX, currentY].MapCharacter.DisplayChar == EMPTY.DisplayChar && currentY > 0)
                        currentY--;
                    break;
                case Direction.East:
                    while (levelMap[currentX, currentY].MapCharacter.DisplayChar == EMPTY.DisplayChar && currentX < MAP_WD)
                        currentX++;
                    break;
                case Direction.South:
                    while (levelMap[currentX, currentY].MapCharacter.DisplayChar == EMPTY.DisplayChar && currentY < MAP_HT)
                        currentY++;
                    break;
                case Direction.West:
                    while (levelMap[currentX, currentY].MapCharacter.DisplayChar == EMPTY.DisplayChar && currentX > 0)
                        currentX--;
                    break;
            }

            if (levelMap[currentX, currentY].MapCharacter.DisplayChar != EMPTY.DisplayChar)
                retValue = levelMap[currentX, currentY];

            return retValue;
        }
        /// <summary>
        /// Look for and return an inventory item at a specific location.
        /// </summary>
        /// <param name="Location">MapSpace object to search</param>
        /// <returns></returns>
        public Inventory? DetectInventory(MapSpace Location)
        {
            Inventory? foundItem = (from Inventory inv in MapInventory
                                     where inv.Location!.X == Location.X
                                     && inv.Location.Y == Location.Y
                                     select inv).FirstOrDefault();

            return foundItem;
        }
        /// <summary>
        /// Search for a monster at a specific location based on the locations
        /// recorded in the ActiveMonsters list.
        /// </summary>
        /// <param name="Location">MapSpace object to search</param>
        /// <returns></returns>
        public Monster? DetectMonster(MapSpace Location)
        {
            Monster? foundMonster = (from Monster monster in ActiveMonsters
                                    where monster.Location!.X == Location.X
                                    && monster.Location.Y == Location.Y
                                    select monster).FirstOrDefault();
            
            return foundMonster;
        }
        /// <summary>
        /// Get the highest priority character for display to the user.
        /// </summary>
        /// <param name="Space">MapSpace object to search</param>
        /// <param name="ShowHidden">Show objects that are marked as hidden and require searching.</param>
        /// <returns></returns>
        public MapGlyph PriorityChar(MapSpace Space, bool ShowHidden)
        {
            Monster? monster = DetectMonster(Space);
            Inventory? invItem = DetectInventory(Space);
            
            MapGlyph retValue;

            if (monster != null)  // If there's a monster present, show it first.
                retValue = monster.DisplayCharacter;
            else if (Space == CurrentPlayer.Location) // Player is next highest.
                retValue = Player.CHARACTER;
            // Inventory comes third as player and monsters can sit  on top.
            else if (invItem != null)
                retValue = invItem.DisplayCharacter;
            // Finally show the alternate char if the current space is hidden.
            else if (Space.AltMapCharacter != null && !ShowHidden)
                retValue = (MapGlyph)Space.AltMapCharacter;
            else
                retValue = Space.MapCharacter;  // Otherwise just show normal char.

            return retValue;
        }
        /// <summary>
        /// Return a list of all spaces around given space in eight directions.
        /// </summary>
        /// <param name="x">Starting X point</param>
        /// <param name="y">Starting Y point</param>
        /// <param name="spaces">Number of spaces outward to search</param>
        /// <returns></returns>
        public List<MapSpace> GetSurrounding(int x, int y, int spaces)
        {
            List<MapSpace> surrounding = (from MapSpace space in levelMap
                                        where Math.Abs(space.X - x) <= spaces
                                        && Math.Abs(space.Y - y) <= spaces
                                        select space).ToList();

            return surrounding;
        }
        /// <summary>
        /// Return a random open space on the map.
        /// </summary>
        /// <param name="hallways">Include hallway spaces</param>
        /// <param name="limitTo">List of MapSpace objects to filter the selection</param>
        /// <returns></returns>
        public MapSpace? GetOpenSpace(bool hallways, List<MapSpace>? limitTo = null)
        {
            // List of map characters that qualify as open for this list.
            string charList = hallways ? (HALLWAY.DisplayChar.ToString() + ROOM_INT.DisplayChar.ToString()) : 
                ROOM_INT.DisplayChar.ToString();

            // Get qualifying open spaces with no inventory or monsters or the current player.
            List<MapSpace> spaces = (from MapSpace space in levelMap
                                     where charList.Contains(space.MapCharacter.DisplayChar)
                                     && DetectInventory(space) == null
                                     && DetectMonster(space) == null
                                     && space != CurrentPlayer.Location
                                     select space).ToList();

            // Limit further based on any list passed in.
            if (limitTo != null)
                spaces = (from MapSpace space in spaces
                          where limitTo.Contains(space)
                          select space).ToList();

            // Return random space in remaining list or null if there are none.
            return (spaces.Count > 0) ? spaces[rand.Next(0, spaces.Count)] : null;
        }
        /// <summary>
        /// For all room spaces in region, set Discovered = True and 
        /// Lighted according to ROOM_LIGHTED probability
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        public void DiscoverRoom(int xPos, int yPos)
        {
            // When the player enters a room, decide if the room should be lighted.

            // Get region limits
            Tuple<MapSpace, MapSpace> corners = GetRegionLimits(xPos, yPos);
            
            // Decide if room is lighted.
            bool roomLights = (rand.Next(1, 101) <= ROOM_LIGHTED);

            Debug.WriteLine($"Opening room {corners.Item1.X}, {corners.Item1.Y} to " +
                $"{corners.Item2.X}, {corners.Item2.Y} in region " +
                $"{GetRegionNumber(corners.Item1.X, corners.Item1.Y)}");

            // For all room spaces in region that have not been discovered,
            // set Discovered = True and Lighted according to probability.
            // Leave HALLWAY and already discovered spaces alone and just focus on rooms.
            for (int y = corners.Item1.Y; y <= corners.Item2.Y; y++)
            {
                for (int x = corners.Item1.X; x <= corners.Item2.X; x++)
                {
                    if (!levelMap[x, y].Discovered)
                    {
                        if(levelMap[x, y].MapCharacter.DisplayChar != HALLWAY.DisplayChar)
                        {
                            levelMap[x, y].Discovered = true;
                            levelMap[x, y].Lighted = roomLights;
                        }
                    }
                    //Turn off remote sight for spaces in the room if they're already visible.
                    levelMap[x, y].RemoteSight = false;
                }
            }
        }
        /// <summary>
        /// Reveals a specific room, discovered or not.
        /// </summary>
        /// <param name="xPos">Starting X point</param>
        /// <param name="yPos">Startign Y point</param>
        public void LightUpRoom(int xPos, int yPos)
        {
            // Get region limits
            Tuple<MapSpace, MapSpace> corners = GetRegionLimits(xPos, yPos);

            // For all room spaces in region, set Discovered = True and 
            // Lighted. Leave HALLWAY spaces alone and just focus on the room.
            for (int y = corners.Item1.Y; y <= corners.Item2.Y; y++)
            {
                for (int x = corners.Item1.X; x <= corners.Item2.X; x++)
                {
                    if (levelMap[x, y].MapCharacter.DisplayChar != HALLWAY.DisplayChar)
                    {
                        levelMap[x, y].Discovered = true;
                        levelMap[x, y].Lighted = true;
                    }
                }
            }
        }

        /// <summary>
        /// Set surrounding spaces to Discovered and Lighted.
        /// </summary>
        /// <param name="xPos">Starting X point</param>
        /// <param name="yPos">Starting Y point</param>
        public void ShowSurrounding(int xPos, int yPos)
        {
            foreach (MapSpace space in GetSurrounding(xPos, yPos, 1))
            {
                // Mark the space as discovered.
                if (!space.Discovered)
                    space.Discovered = true;

                // If this is a wall, stairway or anything else
                // that should remain visible, mark it as lighted.
                if (!space.Lighted)
                {
                    if (MapDiscoveryGlyphList.Contains(space.MapCharacter.DisplayChar))
                        space.Lighted = true;
                }
            }
        }

        /// <summary>
        /// Return True if there's something in one of the surrounding spaces.
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <returns></returns>
        public bool DetectObstruction(int xPos, int yPos)
        {
            bool retValue = false;

            foreach (MapSpace space in GetSurrounding(xPos, yPos, 1))
            {
                // If there's something one of the spaces, return True.
                // Ignore player's space.
                if (!retValue) 
                    if (space.X != xPos || space.Y != yPos)
                        retValue = (!PassableSpacesGlyphList.Contains(PriorityChar(space, false).DisplayChar));
            }

            return retValue;
        }
        /// <summary>
        /// Set the entire map to discovered and visible.
        /// </summary>
        /// <returns></returns>
        public bool DiscoverMap()
        {
            // Set the entire map to discovered and visible.
            bool retValue = false;

            List<MapSpace> spaces = (from MapSpace space in levelMap
                                        where MapDiscoveryGlyphList.Contains(space.MapCharacter.DisplayChar)
                                        select space).ToList();

            spaces.ForEach(space => { space.Discovered = true; space.Lighted = true;
                space.SearchRequired = false; space.AltMapCharacter = null; });            

            return retValue;
        }
        /// <summary>
        /// Show all the inventory of a particular category.
        /// </summary>
        /// <param name="Category">Member of InvCategory enumeration</param>
        /// <returns></returns>
        public bool DiscoverInventoryByCat(Inventory.InvCategory Category)
        {
            // Set all the food on the map to discovered and visible.
            bool retValue = false;
            
            List<Inventory> mapInventory = (from Inventory inv in MapInventory
                                     where inv.ItemCategory == Category
                                     select inv).ToList();

            mapInventory.ForEach(inv => {
                inv.Location.Discovered = true; inv.Location.Lighted = true;
                inv.Location.RemoteSight = true;
                retValue = true;
            });

            return retValue;
        }
        /// <summary>
        /// Return region number 1 through 9 based on map point.
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <returns></returns>
        private int GetRegionNumber(int xPos, int yPos)
        {
            // The map is divided into a 3 x 3 grid of 9 equal regions.
            // This function returns 1 to 9 to indicate where the region is on the map.

            int returnVal;

            //int regionX = ((int)RoomAnchorX / REGION_WD) + 1;
            // Using Ceiling works regardless of where in the region the coords are,
            // even if they're on the edge.
            int regionX = (int)(Math.Ceiling((decimal)xPos / REGION_WD));
            int regionY = (int)(Math.Ceiling((decimal)yPos / REGION_HT));

            returnVal = (regionX) + ((regionY - 1) * 3);

            return returnVal;
        }
        /// <summary>
        /// Get northwest and south east corners of region based on X,Y coordintes.
        /// </summary>
        /// <param name="xPos">Map X coordinate within region</param>
        /// <param name="yPos">Map Y coordinate within region</param>
        /// <returns></returns>
        private Tuple<MapSpace, MapSpace> GetRegionLimits(int xPos, int yPos)
        {
            // Get a pair of MapSpaces defining the limits of the region based
            // on an internal x and y coordinate.
            int xTopLeft = (int)((Math.Ceiling((decimal)xPos / REGION_WD)) - 1) * REGION_WD + 1;
            int yTopLeft = (int)((Math.Ceiling((decimal)yPos / REGION_HT)) - 1) * REGION_HT + 1;

            int xBottomRight = xTopLeft + REGION_WD - 1;
            int yBottomRight = yTopLeft + REGION_HT - 1;

            return new Tuple<MapSpace, MapSpace>(levelMap[xTopLeft, yTopLeft], levelMap[xBottomRight, yBottomRight]);
        }
        /// <summary>
        /// Get northwest and south east corners of specific region.
        /// </summary>
        /// <param name="RegionNumber">Specific region number</param>
        /// <returns></returns>
        private Tuple<MapSpace, MapSpace> GetRegionLimits(int RegionNumber)
        {
            // Get a pair of MapSpaces defining the limits of the region based
            // on the region number from the 3 x 3 grid of regions.
            int gridY = (int)Math.Ceiling((decimal)RegionNumber / 3);
            int gridX = RegionNumber % 3;

            int xTopLeft = ((gridX - 1) * REGION_WD) + 1;
            int yTopLeft = ((gridY - 1) * REGION_HT) + 1;

            int xBottomRight = xTopLeft + REGION_WD - 1;
            int yBottomRight = yTopLeft + REGION_HT - 1;

            return new Tuple<MapSpace, MapSpace>(levelMap[xTopLeft, yTopLeft], levelMap[xBottomRight, yBottomRight]);
        }
        /// <summary>
        /// For Dev mode. Output the array with no alternate characters and everything visible.
        /// </summary>
        /// <returns></returns>
        public MapGlyph[,] MapCheck()
        { 

            // Iterate through the two-dimensional array and transfer the appropriate characters
            // to the output map to display to the user.

            for (int y = 0; y <= MAP_HT; y++)
            {
                for (int x = 0; x <= MAP_WD; x++)
                    DisplayMap[x, y] = PriorityChar(levelMap[x, y], true);
            }

            return DisplayMap;
        }
        /// <summary>
        /// Output the levelMap array DisplayMap for display to the user.
        /// </summary>
        /// <returns></returns>
        public MapGlyph[,] MapText()
        {
            StringBuilder sbReturn = new StringBuilder();
            List<MapSpace> surroundingSpaces = GetSurrounding(CurrentPlayer.Location!.X, CurrentPlayer.Location.Y, 1);
            int playerRegion = GetRegionNumber(CurrentPlayer.Location.X, CurrentPlayer.Location.Y); 
            MapGlyph? priorityChar, appendChar;
            bool playerInRoom = false;
            int regionNo;

            // Iterate through the two-dimensional levelMap MapSpace array and use determine which MapGlyph objects 
            // to output to the DisplayMap array for display to the user.

            for (int y = 0; y <= MAP_HT; y++)
            {
                for (int x = 0; x <= MAP_WD; x++)
                {
                    appendChar = null;
                    regionNo = GetRegionNumber(x, y);
                    // Get priority character
                    priorityChar = PriorityChar(levelMap[x, y], false);

                    // Determine if player is actually in the current region's room.
                    playerInRoom = (regionNo == playerRegion &&
                                (RoomInteriorGlyphList.Contains(CurrentPlayer.Location.MapCharacter.DisplayChar)));

                    // If the space is within one space of the character, show standard 
                    // priority character no matter what.
                    appendChar = surroundingSpaces.Contains(levelMap[x, y]) ? priorityChar : null;

                    // Otherwise, if the space is lighted, check if the player is in the same region and within
                    // the room's walls or if the space is marked for RemoteSight. If so, show the priority character.
                    // Else, just show the map character or the alternate map character as appropriate.
                    if (appendChar == null)
                    {
                        if (levelMap[x, y].Lighted)
                        {
                            if (playerInRoom && RoomInteriorGlyphList.Contains(levelMap[x,y].MapCharacter.DisplayChar) || levelMap[x, y].RemoteSight)
                                appendChar = priorityChar;
                            else
                                appendChar = (levelMap[x, y].SearchRequired) ?
                                    levelMap[x, y].AltMapCharacter : levelMap[x, y].MapCharacter;
                        }

                        // If nothing has been selectd at this point, just pass an empty map space.
                        if (appendChar == null) { appendChar = EMPTY; }
                    }

                    // Set the corresponding space on the DisplayMap to the resulting MapGlyph character.
                    DisplayMap[x, y] = (MapGlyph)appendChar;
                }
            }

            return DisplayMap;
        }

        /// <summary>
        /// Reads directly from monster and inventory lists to provide a list
        /// of the occupied spaces.
        /// </summary>
        /// <returns></returns>
        public List<MapSpace> CurrentMapItems()
        {
            List<MapSpace> retList = new List<MapSpace>();

            // Add the current player's location.
            if (CurrentPlayer.Location != null)
                retList.Add(CurrentPlayer.Location);

            // Add the monsters.
            foreach (Monster monster in ActiveMonsters)
                retList.Add(monster.Location!);

            // Add inventory
            foreach (Inventory item in MapInventory)
                retList.Add(item.Location!);

            return retList;
        }
        /// <summary>
        /// Accepts ASCII screen string and converts it to array of MapGlyphs
        /// </summary>
        /// <param name="TextOutput"></param>
        public void UpdateDisplayFromText(string TextOutput)
        {
            string[] lines = TextOutput.Split('\n');
            int cx = 0, cy = 0, nx = 0, ny = 0;

            // Clear existing text
            for (cy = 0; cy < 25; cy++)
            {
                for (cx = 0; cx < 80; cx++)
                {
                    DisplayMap[cx, cy] =
                        new MapGlyph(MapLevel.EMPTY.DisplayChar, Color.Black, Color.Black);
                }
            }

            // Add new text
            foreach (string line in lines)
            {
                foreach (char c in line)
                {
                    DisplayMap[nx, ny] =
                        new MapGlyph(c, Color.FromArgb(255, 128, 0), Color.Black);

                    nx += 1;
                }
                nx = 0;
                ny += 1;
            }
        }    }

    /// <summary>
    /// Class to hold information for a specific space on the map.
    /// </summary>
    internal class MapSpace{
        /// <summary>
        /// Actual character on map (Room interior, hallway, wall, etc..).
        /// </summary>
        public MapGlyph MapCharacter { get; set; }
        /// <summary>
        /// Map character to display if search is required.
        /// </summary>
        public MapGlyph? AltMapCharacter { get; set; }
        /// <summary>
        /// Inventory items found on the map.
        /// </summary>
        public bool SearchRequired { get; set; }
        /// <summary>
        /// Has the player discovered this space?
        /// </summary>
        public bool Discovered { get; set; }
        /// <summary>
        /// Should an item in this space be revealed regardless of
        /// where the player is? Used by scrolls and potions.
        /// </summary>
        public bool RemoteSight { get; set; } = false;
        /// <summary>
        /// Is space supposed to be visible.
        /// </summary>
        public bool Lighted { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>
        /// Constructor to create new MapSpace, reusing settings from the previous one.
        /// </summary>
        /// <param name="mapChar"></param>
        /// <param name="oldSpace"></param>
        public MapSpace(MapGlyph mapChar, MapSpace oldSpace)
        {
            this.MapCharacter = mapChar;
            this.AltMapCharacter = null; 
            this.SearchRequired = false;
            this.X = oldSpace.X; 
            this.Y = oldSpace.Y; 
            this.Lighted = oldSpace.Lighted;
            this.Discovered = oldSpace.Discovered;
            this.RemoteSight = false;
        }

        /// <summary>
        /// Constructor to create visible character
        /// </summary>
        /// <param name="mapChar"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public MapSpace(MapGlyph mapChar, int X, int Y)
        {
            this.MapCharacter = mapChar;
            this.AltMapCharacter = null;
            this.SearchRequired = false;
            this.Lighted = true;
            this.Discovered = true;
            this.RemoteSight = false;
            this.X = X;
            this.Y = Y;
        }

        /// <summary>
        /// Basic constructor to create mapspace.
        /// </summary>
        /// <param name="mapChar"></param>
        /// <param name="hidden"></param>
        /// <param name="search"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public MapSpace(MapGlyph mapChar, Boolean hidden, Boolean search, int X, int Y)
        {
            this.MapCharacter = mapChar;
            this.AltMapCharacter = null;
            this.SearchRequired = search;
            this.Lighted = !hidden;
            this.Discovered = false;
            this.RemoteSight = false;
            this.X = X;
            this.Y = Y;
        }
    }

    /// <summary>
    /// Class to hold display character and colors.
    /// Used within MapSpace class.
    /// </summary>
    public struct MapGlyph
    {
        public char DisplayChar;
        public Color Foreground;
        public Color Background;

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="displayChar"></param>
        /// <param name="foreground"></param>
        /// <param name="background"></param>
        public MapGlyph(char displayChar, Color foreground, Color background)
        {
            DisplayChar = displayChar;
            Foreground = foreground;
            Background = background;
        }
    }
}

