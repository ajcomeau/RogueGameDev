using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace RogueGame{

    internal class MapLevel
    {
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
        /// Dictionary to hold hallway endings during map generation.
        /// </summary>
        private Dictionary<MapSpace, Direction> deadEnds = 
            new Dictionary<MapSpace, Direction>();

        /// <summary>
        /// Box drawing constants and other symbols.
        /// </summary>
        private const char HORIZONTAL = '═';        // Unicode symbols can be copy-pasted from https://www.w3.org/TR/xml-entity-names/025.html.  
        private const char VERTICAL = '║';
        private const char CORNER_NW = '╔';
        private const char CORNER_SE = '╝';
        private const char CORNER_NE = '╗';
        private const char CORNER_SW = '╚';
        public const char ROOM_INT = '·';
        public const char ROOM_DOOR = '╬';
        public const char HALLWAY = '▒';
        public const char STAIRWAY = '≣';
        public const char GOLD = '*';
        public const char AMULET = '♀';
        private const char EMPTY = ' ';
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
        private const int ROOM_GOLD_PCT = 70; 
        /// <summary>
        /// Maximum inventory in a room.
        /// </summary>
        private const int MAX_INVENTORY = 3; 
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
        /// List of characters that will be marked as Visible during map discovery.
        /// </summary>
        public static List<char> MapDiscovery = new List<char>(){HORIZONTAL, VERTICAL,
            CORNER_NW, CORNER_SE, CORNER_NE, CORNER_SW, ROOM_DOOR, HALLWAY, STAIRWAY};

        /// <summary>
        /// List of characters that occur inside a room.
        /// </summary>
        public static List<char> RoomInterior = new List<char>(){ROOM_DOOR, ROOM_INT, STAIRWAY};

        /// <summary>
        /// List of characters a player or monster can move onto.
        /// </summary>
        public static List<char> SpacesAllowed = new List<char>(){ROOM_INT, STAIRWAY, ROOM_DOOR, HALLWAY};

        /// <summary>
        /// List of characters that can be moved past on Fast Play.
        /// </summary>
        public static List<char> GlideSpaces = new List<char>(){ROOM_INT, HORIZONTAL, VERTICAL, CORNER_NE,
                CORNER_NW, CORNER_SE, CORNER_SW, HALLWAY, EMPTY};

        /// <summary>
        /// List of monsters on current map.
        /// </summary>
        public List<Monster> ActiveMonsters = new List<Monster>();

        /// <summary>
        /// Array to hold map definition.
        /// </summary>
        private MapSpace[,] levelMap = new MapSpace[80, 25];
        
        /// <summary>
        /// Random number generator
        /// </summary>
        private static Random rand = new Random();

        /// <summary>
        /// Make map available to other classes.
        /// </summary>
        public MapSpace[,] LevelMap
        {
            get { return levelMap; }
        }

        public int CurrentLevel { get; set; }

        /// <summary>
        /// Constructor - generate a new map for this level.
        /// </summary>
        public MapLevel(int levelNumber)
        {
            CurrentLevel = levelNumber;
            do
            {
                MapGeneration();
            } while (!VerifyMap());            
        }

        /// <summary>
        /// Verify that the generate map is free of isolated rooms or sections.
        /// </summary>
        /// <returns>True / False based on validity of map.</returns>
        private bool VerifyMap()
        {
            bool retValue = true;
            List<char> dirCheck = new List<char>();

            // Check horizontal for blank rows which no hallways. Top and bottom might be legitimately blank
            // so just check a portion of the map.

            for (int y = REGION_HT - MIN_ROOM_HT; y < (REGION_HT * 2) + MIN_ROOM_HT; y++)
            {
                dirCheck = (from MapSpace space in levelMap
                                where space.X <= MAP_WD
                                && space.Y == y
                                select space).Select(c => c.MapCharacter).Distinct().ToList();

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
                                select space).ToList().Select(c => c.MapCharacter).Distinct().ToList();

                    retValue = dirCheck.Count > 1;
                    if (!retValue) { break; }
                }
            }

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
                        levelMap[x, y] = new MapSpace(' ', false, false, x, y);
                }
            }

            // Create hallways and add stairway
            HallwayGeneration();
            AddStairway();
        }


        /// <summary>
        /// Search the array randomly for an interior room space and mark it as a hallway.
        /// </summary>
        private void AddStairway()
        {
            List <MapSpace> openSpaces = FindOpenSpaces(false);
            MapSpace stairway = openSpaces[rand.Next(openSpaces.Count)];

            levelMap[stairway.X, stairway.Y] = new MapSpace(STAIRWAY, stairway.X, stairway.Y);
        }

        /// <summary>
        /// Create room on map based on inputs
        /// </summary>
        /// <param name="westWallX"></param>
        /// <param name="northWallY"></param>
        /// <param name="roomWidth"></param>
        /// <param name="roomHeight"></param>
        private void RoomGeneration(int westWallX, int northWallY, int roomWidth, int roomHeight)
        {
            int eastWallX = westWallX + roomWidth;          // Calculate room east
            int southWallY = northWallY + roomHeight;       // Calculate room south

            // Regions are defined 1 to 9, L to R, top to bottom.
            int regionNumber = GetRegionNumber(westWallX, northWallY);
            int doorway = 0, doorCount = 0, openX, openY;

            bool searchRequired;

            // Inventory variables.
            int maxInventoryItems = rand.Next(1, MAX_INVENTORY + 1);
            int mapInventory = 0;
            Inventory invItem;
            Monster? spawned;
            MapSpace itemSpace;

            // Create horizontal and vertical walls for room.
            for (int y = northWallY; y <= southWallY; y++)
            {
                for (int x = westWallX; x <= eastWallX; x++)
                {
                    if (y == northWallY || y == southWallY)
                    {
                        levelMap[x, y] = new MapSpace(HORIZONTAL, false, false, x, y);
                    }
                    else if (x == westWallX || x == eastWallX)
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
                    doorway = rand.Next(westWallX + 1, eastWallX);  // Random point on wall.
                    searchRequired = rand.Next(1, 101) <= HIDDEN_EXIT_PCT;  // Is the doorway hidden?
                    levelMap[doorway, northWallY] = new MapSpace(ROOM_DOOR, false, searchRequired, doorway, northWallY);
                    levelMap[doorway, northWallY].AltMapCharacter = searchRequired ? HORIZONTAL : null;
                    levelMap[doorway, northWallY - 1] = new MapSpace(HALLWAY, false, false, doorway, northWallY - 1);
                    deadEnds.Add(levelMap[doorway, northWallY - 1], Direction.North);  // Add to dead ends list.
                    doorCount++;  // Increment the count of doors created.
                }

                if (regionNumber <= 6 && rand.Next(1, 101) <= ROOM_EXIT_PCT)  // South doorways
                {
                    doorway = rand.Next(westWallX + 1, eastWallX);
                    searchRequired = rand.Next(1, 101) <= HIDDEN_EXIT_PCT;
                    levelMap[doorway, southWallY] = new MapSpace(ROOM_DOOR, false, searchRequired, doorway, southWallY);
                    levelMap[doorway, southWallY].AltMapCharacter = searchRequired ? HORIZONTAL : null;
                    levelMap[doorway, southWallY + 1] = new MapSpace(HALLWAY, false, false, doorway, southWallY + 1);
                    deadEnds.Add(levelMap[doorway, southWallY + 1], Direction.South);
                    doorCount++;
                }

                if ("147258".Contains(regionNumber.ToString()) && rand.Next(1, 101) <= ROOM_EXIT_PCT)  // East doorways
                {
                    doorway = rand.Next(northWallY + 1, southWallY);
                    searchRequired = rand.Next(1, 101) <= HIDDEN_EXIT_PCT;
                    levelMap[eastWallX, doorway] = new MapSpace(ROOM_DOOR, false, searchRequired, eastWallX, doorway);
                    levelMap[eastWallX, doorway].AltMapCharacter = searchRequired ? VERTICAL : null;
                    levelMap[eastWallX + 1, doorway] = new MapSpace(HALLWAY, false, false, eastWallX + 1, doorway);
                    deadEnds.Add(levelMap[eastWallX + 1, doorway], Direction.East);
                    doorCount++;
                }

                if ("258369".Contains(regionNumber.ToString()) && rand.Next(1, 101) <= ROOM_EXIT_PCT)  // West doorways
                {
                    doorway = rand.Next(northWallY + 1, southWallY);
                    searchRequired = rand.Next(1, 101) <= HIDDEN_EXIT_PCT;
                    levelMap[westWallX, doorway] = new MapSpace(ROOM_DOOR, false, searchRequired, westWallX, doorway);
                    levelMap[westWallX, doorway].AltMapCharacter = searchRequired ? VERTICAL : null;
                    levelMap[westWallX - 1, doorway] = new MapSpace(HALLWAY, false, false, westWallX - 1, doorway);
                    deadEnds.Add(levelMap[westWallX - 1, doorway], Direction.West);
                    doorCount++;
                }
            }

            // Set the room corners.
            levelMap[westWallX, northWallY] = new MapSpace(CORNER_NW, false, false, westWallX, northWallY);
            levelMap[eastWallX, northWallY] = new MapSpace(CORNER_NE, false, false, eastWallX, northWallY);
            levelMap[westWallX, southWallY] = new MapSpace(CORNER_SW, false, false, westWallX, southWallY);
            levelMap[eastWallX, southWallY] = new MapSpace(CORNER_SE, false, false, eastWallX, southWallY);

            // Set starting point to northwest corner
            openX = westWallX; openY = northWallY;
            itemSpace = levelMap[openX, openY];

            // Evaluate room for a gold deposit
            if (rand.Next(1, 101) < ROOM_GOLD_PCT)
            {
                // Search the room randomly for an empty interior room space
                // and mark it as a gold deposit.
                while (levelMap[openX, openY].MapCharacter != ROOM_INT)
                {
                    openX = rand.Next(westWallX + 1, eastWallX);
                    openY = rand.Next(northWallY + 1, southWallY);
                }

                levelMap[openX, openY].ItemCharacter = GOLD;
            }

            // Add up to the number of specified inventory items.
            while (mapInventory < maxInventoryItems)
            {
                invItem = Inventory.GetInventoryItem();

                // Place the inventory according to its chances of showing up.
                if (rand.Next(1, 101) <= invItem.AppearancePct)
                {
                    // For ammunition that's groupable, decide how many items are in the batch.
                    if (invItem.ItemCategory == Inventory.InvCategory.Ammunition 
                        && invItem.IsGroupable)
                        invItem.Amount = rand.Next(1, Inventory.MAX_AMMO_BATCH + 1);

                    // Look for an interior space that hasn't been used by gold.
                    while (itemSpace.MapCharacter != ROOM_INT || itemSpace.ItemCharacter != null)
                    {
                        openX = rand.Next(westWallX + 1, eastWallX);
                        openY = rand.Next(northWallY + 1, southWallY);
                        itemSpace = levelMap[openX, openY];
                    }
                    
                    // Update the space and increment the count.
                    itemSpace.MapInventory = invItem;

                    mapInventory++;
                }
            }

            // Add a monster to room based on probability.
            if(rand.Next(1, 101) < SPAWN_MONSTER)
            {
                do
                {
                    spawned = Monster.SpawnMonster(CurrentLevel);
                } while (spawned != null && rand.Next(1, 101) <= spawned.AppearancePct);

                if (spawned != null)
                {
                    // Look for an interior space. Monsters can sit on top of
                    // anything within the room.
                    do
                    {
                        openX = rand.Next(westWallX + 1, eastWallX);
                        openY = rand.Next(northWallY + 1, southWallY);
                        itemSpace = levelMap[openX, openY];
                    } while (itemSpace.MapCharacter != ROOM_INT);

                    ActiveMonsters.Add(spawned);
                    itemSpace.DisplayCharacter = spawned.DisplayCharacter;
                    spawned.Location = itemSpace;
                }
            }
        }

        /// <summary>
        /// Create hallways between all the existing rooms.
        /// </summary>
        private void HallwayGeneration()
        {
            Direction hallDirection = Direction.None; Direction direction90; Direction direction270;
            MapSpace hallwaySpace, newSpace;
            Dictionary<Direction, MapSpace> adjacentChars = new Dictionary<Direction, MapSpace>();
            Dictionary<Direction, MapSpace> surroundingChars = new Dictionary<Direction, MapSpace>();
            bool hallwayDug = false;

            // Grab a copy of the deadEnds collection so we can do a final pass at the end.
            Dictionary<MapSpace, Direction> deadEndsCopy = new Dictionary<MapSpace, Direction>(deadEnds);

            // Iterate through the list of hallway endings (deadends) until all are resolved one way or another.
            // Count backwards so we can remove processed items.

            // If there are doors on more than one side, the hallway is already connected.
            for (int i = deadEnds.Count - 1; i >= 0; i--)
            {
                hallwaySpace = deadEnds.ElementAt(i).Key;

                if (SearchAdjacent(ROOM_DOOR, hallwaySpace.X, hallwaySpace.Y).Count > 1)
                    deadEnds.Remove(hallwaySpace);
            }

            while (deadEnds.Count > 0)
            {
                // If there's a neighboring hallway space, this one is already connected.
                for (int i = deadEnds.Count - 1; i >= 0; i--)
                {
                    hallwaySpace = deadEnds.ElementAt(i).Key;

                    if (SearchAdjacent(HALLWAY, hallwaySpace.X, hallwaySpace.Y).Count > 1)
                        deadEnds.Remove(hallwaySpace);
                }

                for (int i = deadEnds.Count - 1; i >= 0; i--)
                {
                    // Establish current space and three directions - forward and to the sides.
                    hallwaySpace = deadEnds.ElementAt(i).Key;
                    hallDirection = deadEnds.ElementAt(i).Value;
                    direction90 = GetDirection90(hallDirection);
                    direction270 = GetDirection270(hallDirection);
                    hallwayDug = false;

                    // Look for distant hallways in three directions.  If one is found, connect to it.
                    if (hallDirection != Direction.None)
                    {
                        surroundingChars = SearchAllDirections(hallwaySpace.X, hallwaySpace.Y);

                        // Forward ...
                        if ((surroundingChars[hallDirection] != null &&
                            surroundingChars[hallDirection].MapCharacter == HALLWAY))
                        {
                            DrawHallway(hallwaySpace, surroundingChars[hallDirection], hallDirection);
                            hallwayDug = true;
                        }
                        
                        // To one side ... 
                        if ((surroundingChars[direction90] != null &&
                            surroundingChars[direction90].MapCharacter == HALLWAY))
                        {
                            DrawHallway(hallwaySpace, surroundingChars[direction90], direction90);
                            hallwayDug = true;
                        }
                        
                        // To the other side.
                        if ((surroundingChars[direction270] != null &&
                            surroundingChars[direction270].MapCharacter == HALLWAY))
                        {
                            DrawHallway(hallwaySpace, surroundingChars[direction270], direction270);
                            hallwayDug = true;
                        }

                        if (!hallwayDug)
                        {
                            // If there's no hallway to connect to, just add another space where possible for the
                            // next iteration to pick up on.
                            adjacentChars = SearchAdjacent(EMPTY, hallwaySpace.X, hallwaySpace.Y);
                            if (adjacentChars.ContainsKey(hallDirection))
                            {
                                // Forward ...
                                newSpace = new MapSpace(HALLWAY, adjacentChars[hallDirection]);
                                levelMap[adjacentChars[hallDirection].X, adjacentChars[hallDirection].Y] = newSpace;
                                deadEnds.Remove(hallwaySpace);
                                deadEnds.Add(newSpace, hallDirection);
                            }
                            else if (adjacentChars.ContainsKey(direction90))
                            {
                                // To one side ...
                                newSpace = new MapSpace(HALLWAY, adjacentChars[direction90]);
                                levelMap[adjacentChars[direction90].X, adjacentChars[direction90].Y] = newSpace;
                                deadEnds.Remove(hallwaySpace);
                                deadEnds.Add(newSpace, direction90);
                            }
                            else if (adjacentChars.ContainsKey(direction270))
                            {
                                // Then the other side.
                                newSpace = new MapSpace(HALLWAY, adjacentChars[direction270]);
                                levelMap[adjacentChars[direction270].X, adjacentChars[direction270].Y] = newSpace;
                                deadEnds.Remove(hallwaySpace);
                                deadEnds.Add(newSpace, direction270);
                            }
                            break;
                        }

                        deadEnds.Remove(hallwaySpace);
                    }
                    else
                    {
                        deadEnds.Remove(hallwaySpace);
                    }

                    //Console.Write(MapText());
                }
            }
        }        

        /// <summary>
        /// Draw a hallway between specified spaces.  Break off if another hallway
            /// is discovered to the side.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="hallDirection"></param>
        private void DrawHallway(MapSpace start, MapSpace end, Direction hallDirection)
        {
            switch (hallDirection) {
                case Direction.North:
                    for (int y = start.Y; y >= end.Y; y--)
                    {
                        levelMap[end.X, y] = new MapSpace(HALLWAY, end.X, y);
                        if (SearchAdjacent(HALLWAY, end.X, y).Count > 1)
                            break;
                    }
                    break;
                case Direction.South:
                    for (int y = start.Y; y <= end.Y; y++)
                    {
                        levelMap[end.X, y] = new MapSpace(HALLWAY, end.X, y);
                        if (SearchAdjacent(HALLWAY, end.X, y).Count > 1)
                            break;
                    }
                    break;
                case Direction.East:
                    for (int x = start.X; x <= end.X; x++)
                    {
                        levelMap[x, end.Y] = new MapSpace(HALLWAY, x, end.Y);
                        if (SearchAdjacent(HALLWAY, x, end.Y).Count > 1)
                            break;
                    }
                    break;
                case Direction.West:
                    for (int x = start.X; x >= end.X; x--)
                    {
                        levelMap[x, end.Y] = new MapSpace(HALLWAY, x, end.Y);
                        if (SearchAdjacent(HALLWAY, x, end.Y).Count > 1)
                            break;
                    }
                    break;           
            }

        }        

        /// <summary>
        /// Raise the fog of war and hide the map.
        /// </summary>
        public void ShroudMap()
        {
            List<MapSpace> mapSpaces = (from MapSpace space in levelMap
                                            select space).ToList();

            // Iterate through MapSpaces
            foreach (MapSpace space in mapSpaces)
            {
                space.Discovered = false;
                space.Visible = false;
            }

        }

        /// <summary>
        /// Return direction 90 degrees from original based on forward direction.
        /// </summary>
        /// <param name="startingDirection"></param>
        /// <returns></returns>
        public Direction GetDirection90(Direction startingDirection)
        {
            Direction retValue = (Math.Abs((int)startingDirection) == 1) ? (Direction)2 : (Direction)1;
            return retValue;
        }

        /// <summary>
        /// Return direction 270 degrees from original (opposite of 90 degrees) based on forward direction.
        /// </summary>
        /// <param name="startingDirection"></param>
        /// <returns></returns>
        public Direction GetDirection270(Direction startingDirection)
        {
            Direction retValue = (Math.Abs((int)startingDirection) == 1) ? (Direction)2 : (Direction)1;
            retValue = (Direction)((int)retValue * -1);
            return retValue;
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

            if (y - 1 >= 0 && levelMap[x, y - 1].MapCharacter == character)  // North
                retValue.Add(Direction.North, levelMap[x, y - 1]);

            if (x + 1 <= MAP_WD && levelMap[x + 1, y].MapCharacter == character) // East
                retValue.Add(Direction.East, levelMap[x + 1, y]);

            if (y + 1 <= MAP_HT && levelMap[x, y + 1].MapCharacter == character)  // South
                retValue.Add(Direction.South, levelMap[x, y + 1]);

            if ((x - 1) >= 0 && levelMap[x - 1, y].MapCharacter == character)  // West
                retValue.Add(Direction.West, levelMap[x - 1, y]);

            return retValue;

        }

        /// <summary>
        /// Search in four directions around point. Return list of directions and characters found.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Dictionary of directions and characters found.</returns>
        public Dictionary<Direction, MapSpace> SearchAdjacent(int x, int y)
        {
            Dictionary<Direction, MapSpace> retValue = new Dictionary<Direction, MapSpace>();
            retValue.Add(Direction.North, levelMap[x, y - 1]);
            retValue.Add(Direction.East, levelMap[x + 1, y]);
            retValue.Add(Direction.South, levelMap[x, y + 1]);
            retValue.Add(Direction.West, levelMap[x - 1, y]);

            return retValue;
        }

        /// <summary>
        /// Look in all directions and return a Dictionary of the first non-space characters found.
        /// </summary>
        /// <param name="currentX"></param>
        /// <param name="currentY"></param>
        /// <returns></returns>
        public Dictionary<Direction, MapSpace> SearchAllDirections(int currentX, int currentY)
        {
            Dictionary<Direction, MapSpace> retValue = new Dictionary<Direction, MapSpace>();

            retValue.Add(Direction.North, SearchDirection(Direction.North, currentX, currentY - 1));
            retValue.Add(Direction.South, SearchDirection(Direction.South, currentX, currentY + 1));
            retValue.Add(Direction.East, SearchDirection(Direction.East, currentX + 1, currentY));
            retValue.Add(Direction.West, SearchDirection(Direction.West, currentX - 1, currentY));

            return retValue;
        }

        /// <summary>
        /// Get the next non-space object found in a given direction. Return null if none is found.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <returns></returns>
        public MapSpace SearchDirection(Direction direction, int startX, int startY)
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
                    while (levelMap[currentX, currentY].MapCharacter == EMPTY && currentY > 0)
                        currentY--;
                    break;
                case Direction.East:
                    while (levelMap[currentX, currentY].MapCharacter == EMPTY && currentX < MAP_WD)
                        currentX++;
                    break;
                case Direction.South:
                    while (levelMap[currentX, currentY].MapCharacter == EMPTY && currentY < MAP_HT)
                        currentY++;
                    break;
                case Direction.West:
                    while (levelMap[currentX, currentY].MapCharacter == EMPTY && currentX > 0)
                        currentX--;
                    break;
            }

            if (levelMap[currentX, currentY].MapCharacter != EMPTY)
                retValue = levelMap[currentX, currentY];

            return retValue;
        }


        /// <summary>
        /// Return a list of all spaces around given space in eight directions.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public List<MapSpace> GetSurrounding(int x, int y)
        {
            List<MapSpace> surrounding = (from MapSpace space in levelMap
                                        where Math.Abs(space.X - x) <= 1
                                        && Math.Abs(space.Y - y) <= 1
                                        select space).ToList();

            return surrounding;
        }

        /// <summary>
        /// Return the mapspace containing the player.
        /// </summary>
        /// <returns></returns>
        public MapSpace FindPlayer()
        {
            MapSpace playerSpace = (from MapSpace space in levelMap
                                        where space.DisplayCharacter == Player.CHARACTER
                                        select space).First();

            return playerSpace;
        }

        /// <summary>
        /// Return a list of all monsters and the player by checking the display character.
        /// </summary>
        /// <returns></returns>
        public List<MapSpace> FindAllOccupants()
        {
            List<MapSpace> occupants = (from MapSpace space in levelMap
                                            where space.DisplayCharacter != null
                                            select space).ToList();

            return occupants;
        }

        /// <summary>
        /// Return a list of all items on the map by checking the item character.
        /// </summary>
        /// <returns></returns>
        public List<MapSpace> FindAllItems()
        {
            List<MapSpace> items = (from MapSpace space in levelMap
                                        where space.ItemCharacter != null || space.MapInventory != null
                                        select space).ToList();
            
            return items;
        }

        /// <summary>
        /// Return a list of all open spaces on the map by checking the map character.
        /// </summary>
        /// <param name="hallways"></param>
        /// <returns></returns>
        public List<MapSpace> FindOpenSpaces(bool hallways)
        {

            string charList = hallways ? (HALLWAY.ToString() + ROOM_INT.ToString()) : ROOM_INT.ToString();

            List<MapSpace> spaces = (from MapSpace space in levelMap
                                     where charList.Contains(space.MapCharacter)
                                     && space.ItemCharacter == null
                                     && space.DisplayCharacter == null
                                     && space.MapInventory == null
                                     select space).ToList();

            return spaces;
        }

        /// <summary>
        /// Find a random space within one of the rooms that hasn't been occupied 
        /// and add the player or monster.
        /// </summary>
        /// <param name="MapChar">Character to be added to map.</param>
        /// <returns></returns>
        public MapSpace AddCharacterToMap(char MapChar)
        {
            MapSpace select;

            List<MapSpace> spaces = FindOpenSpaces(false);
            
            select = spaces[rand.Next(0, spaces.Count)];                 

            // If the character is for the player or a monster, add
            // it to the Display character. Otherwise, use the item character.
            select.DisplayCharacter = MapChar;

            return select;
        }

        /// <summary>
        /// Add amulet to the map.
        /// </summary>
        /// <returns></returns>
        public MapSpace AddAmuletToMap()
        {
            MapSpace select;

            List<MapSpace> spaces = FindOpenSpaces(false);
            select = spaces[rand.Next(0, spaces.Count)];
            select.MapInventory = Inventory.GetInventoryItem("The Amulet");
            return select;
        }

        /// <summary>
        /// Change the display character for the specified map space and return the reference as a confirmation.
        /// </summary>
        /// <param name="Start"></param>
        /// <param name="Destination"></param>
        /// <returns></returns>
        public MapSpace MoveDisplayItem(MapSpace Start, MapSpace Destination)
        {
            levelMap[Destination.X, Destination.Y].DisplayCharacter = Start.DisplayCharacter;
            levelMap[Start.X, Start.Y].DisplayCharacter = null;

            return Destination;
        }

        /// <summary>
        /// For all room spaces in region, set Discovered = True and 
        /// Visible according to ROOM_LIGHTED probability
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        public void DiscoverRoom(int xPos, int yPos)
        {
            // Get region limits
            Tuple<MapSpace, MapSpace> corners = GetRegionLimits(xPos, yPos);
            
            // Decide if room is lighted.
            bool roomLights = (rand.Next(1, 101) <= ROOM_LIGHTED);

            Debug.WriteLine($"Opening room {corners.Item1.X}, {corners.Item1.Y} to " +
                $"{corners.Item2.X}, {corners.Item2.Y} in region " +
                $"{GetRegionNumber(corners.Item1.X, corners.Item1.Y)}");

            // For all room spaces in region, set Discovered = True and 
            // Visible according to probability. Leave HALLWAY spaces alone
            // and just focus on rooms.
            for (int y = corners.Item1.Y; y <= corners.Item2.Y; y++)
            {
                for (int x = corners.Item1.X; x <= corners.Item2.X; x++)
                {
                    if (!levelMap[x, y].Discovered)
                    {
                        if(levelMap[x, y].MapCharacter != HALLWAY)
                        {
                            levelMap[x, y].Discovered = true;
                            levelMap[x, y].Visible = roomLights;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set surrounding spaces to Discovered.  Return True if there's something in one of them.
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <returns></returns>
        public bool DiscoverSurrounding(int xPos, int yPos)
        {
            bool retValue = false;

            foreach (MapSpace space in GetSurrounding(xPos, yPos))
            {
                // Mark the space as discovered.
                if (!space.Discovered)
                    space.Discovered = true;

                // If this is a wall, stairway or anything else
                // that should remain visible, mark it as visible.
                if (!space.Visible)
                {
                    if (MapDiscovery.Contains(space.MapCharacter))
                        space.Visible = true;
                }

                // If there's something one of the spaces, return True.
                // Ignore player's space.
                if (!retValue) 
                    if (space.X != xPos || space.Y != yPos)
                        retValue = space.Occupied() ||
                            (!GlideSpaces.Contains(space.PriorityChar()));
            }

            return retValue;
        }

        /// <summary>
        /// Return region number 1 through 9 based on map point.
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <returns></returns>
        public int GetRegionNumber(int xPos, int yPos)
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
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <returns></returns>
        public Tuple<MapSpace, MapSpace> GetRegionLimits(int xPos, int yPos)
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
        /// <param name="RegionNumber"></param>
        /// <returns></returns>
        public Tuple<MapSpace, MapSpace> GetRegionLimits(int RegionNumber)
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
        /// For Dev mode. Output the array to text for display with no alternate characters and everything visible.
        /// </summary>
        /// <returns></returns>
        public string MapCheck()
        { 
            StringBuilder sbReturn = new StringBuilder();
            char? priorityChar;

            // Iterate through the two-dimensional array and use StringBuilder to 
            // concatenate the proper characters into rows and columns for display.

            for (int y = 0; y <= MAP_HT; y++)
            {
                for (int x = 0; x <= MAP_WD; x++)
                {
                    // Priority - DisplayCharacter, ItemCharacter, MapCharacter.
                    if (levelMap[x, y].DisplayCharacter != null)
                        priorityChar = levelMap[x, y].DisplayCharacter;
                    else if (levelMap[x, y].ItemCharacter != null)
                        priorityChar = levelMap[x, y].ItemCharacter;
                    else if (levelMap[x, y].MapInventory != null)
                        priorityChar = levelMap[x, y].MapInventory.DisplayCharacter;
                    else
                        priorityChar = levelMap[x, y].MapCharacter;

                    sbReturn.Append(priorityChar);                    
                }

                sbReturn.Append("\n");     // Start new line.           
            }

            return sbReturn.ToString();
        }

        /// <summary>
        /// Output the array to text for display.
        /// </summary>
        /// <param name="PlayerLocation"></param>
        /// <returns></returns>
        public string MapText(MapSpace PlayerLocation)
        {
            StringBuilder sbReturn = new StringBuilder();
            MapSpace playerSpace = PlayerLocation;
            List<MapSpace> surroundingSpaces = GetSurrounding(playerSpace.X, playerSpace.Y);
            int playerRegion = GetRegionNumber(playerSpace.X, playerSpace.Y);
            char? priorityChar, appendChar;
            bool inRoom = false;    

            // Iterate through the two-dimensional array and use StringBuilder to 
            // concatenate the proper characters into rows and columns for display.

            for (int y = 0; y <= MAP_HT; y++)
            {
                for (int x = 0; x <= MAP_WD; x++)
                {
                    // Get priority character
                    priorityChar = levelMap[x, y].PriorityChar();

                    // Determine if player is actually in the room.
                    inRoom = (GetRegionNumber(x, y) == playerRegion &&
                                (RoomInterior.Contains(playerSpace.MapCharacter)));

                    // If the space is within one space of the character, show standard priority character.  
                    appendChar = surroundingSpaces.Contains(levelMap[x, y]) ? priorityChar : null;

                    // If the space is set to visible
                    if (appendChar == null)
                    {
                        if (levelMap[x, y].Visible)
                        {
                            // If the player is in the room, or the space represents the player,
                            // show the standard priority character. Otherwise, just show the map character.
                            if (inRoom)
                                appendChar = priorityChar;
                            else
                                appendChar = (levelMap[x, y].SearchRequired) ?
                                    levelMap[x, y].AltMapCharacter : levelMap[x, y].MapCharacter;
                        }
                    }

                    if(appendChar == null) { appendChar = ' '; }

                    sbReturn.Append(appendChar);
                }

                sbReturn.Append("\n");     // Start new line.           
            }

            return sbReturn.ToString();
        }
    }

    /// <summary>
    /// Class to hold information for a specific space on the map.
    /// </summary>
    internal class MapSpace{
        /// <summary>
        /// Actual character on map (Room interior, hallway, wall, etc..).
        /// </summary>
        public char MapCharacter { get; set; }
        /// <summary>
        /// Map character to display if search is required.
        /// </summary>
        public char? AltMapCharacter { get; set; }
        /// <summary>
        /// Item sitting on map, usually gold.
        /// </summary>
        public char? ItemCharacter { get; set; }
        /// <summary>
        /// Player and monsters.
        /// </summary>
        public char? DisplayCharacter { get; set; }
        /// <summary>
        /// Inventory items found on the map.
        /// </summary>
        public Inventory? MapInventory { get; set; }
        /// <summary>
        /// Does the player need to search to reveal?
        /// </summary>
        public bool SearchRequired { get; set; }
        /// <summary>
        /// Has the player discovered this space?
        /// </summary>
        public bool Discovered { get; set; }
        /// <summary>
        /// Is space supposed to be visible.
        /// </summary>
        public bool Visible { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>
        /// Constructor to create blank space for map
        /// </summary>
        public MapSpace()
        {
            this.MapCharacter = ' ';
            this.AltMapCharacter = null;
            this.ItemCharacter = null;
            this.DisplayCharacter = null;
            this.SearchRequired = false;
            this.Visible = true;
            this.Discovered = false;
            X = 0;
            Y = 0;
        }

        /// <summary>
        /// Constructor to create new MapSpace, reusing settings from the previous one.
        /// </summary>
        /// <param name="mapChar"></param>
        /// <param name="oldSpace"></param>
        public MapSpace(char mapChar, MapSpace oldSpace)
        {
            this.MapCharacter = mapChar;
            this.AltMapCharacter = null; 
            this.ItemCharacter = null;
            this.DisplayCharacter = null;
            this.SearchRequired = false;
            this.X = oldSpace.X; 
            this.Y = oldSpace.Y; 
            this.Visible = oldSpace.Visible;
            this.Discovered = oldSpace.Discovered;
        }

        /// <summary>
        /// Constructor to create visible character
        /// </summary>
        /// <param name="mapChar"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public MapSpace(char mapChar, int X, int Y)
        {
            this.MapCharacter = mapChar;
            this.AltMapCharacter = null;
            this.ItemCharacter = null;
            this.DisplayCharacter = null;
            this.SearchRequired = false;
            this.Visible = true;
            this.Discovered = true;
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
        public MapSpace(char mapChar, Boolean hidden, Boolean search, int X, int Y)
        {
            this.MapCharacter = mapChar;
            this.AltMapCharacter = null;
            this.ItemCharacter = null;
            this.DisplayCharacter = null;
            this.SearchRequired = search;
            this.Visible = !hidden;
            this.Discovered = false;
            this.X = X;
            this.Y = Y;
        }

        /// <summary>
        /// Return character to be displayed for space based on standard priority.
        /// DisplayCharacter, ItemCharacter, MapInventory.DisplayCharacter AltMapCharacter, MapCharacter.
        /// </summary>
        /// <returns></returns>
        public char PriorityChar()
        {
            char retValue;

            if (this.DisplayCharacter != null)
                retValue = (char)this.DisplayCharacter;
            else if (this.ItemCharacter != null)
                retValue = (char)this.ItemCharacter;
            else if (this.MapInventory != null)
                retValue = this.MapInventory.DisplayCharacter;
            else if (this.AltMapCharacter != null)
                retValue = (char)this.AltMapCharacter;
            else
                retValue = this.MapCharacter;

            return retValue;
        }

        /// <summary>
        /// Determine if there's something on the map space.
        /// </summary>
        /// <returns></returns>
        public bool Occupied()
        {
            char priorityChar = PriorityChar();

            return (priorityChar != this.MapCharacter 
                && priorityChar != this.AltMapCharacter
                && priorityChar != Player.CHARACTER);
        }

        /// <summary>
        /// Determine if the space contains an inventory item or gold.
        /// </summary>
        /// <returns></returns>
        public bool ContainsItem()
        {
            return(this.ItemCharacter != null ||  this.MapInventory != null);
        }        
    }
}

