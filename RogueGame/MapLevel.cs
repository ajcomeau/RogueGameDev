using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing.Text;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace RogueGame{

    internal class MapLevel
    {
        public enum Direction
        {
            None = 0,
            North = 1,
            East = 2,
            South = -1,
            West = -2
        }

        // Dictionary to hold hallway endings during map generation.
        private Dictionary<MapSpace, Direction> deadEnds = 
            new Dictionary<MapSpace, Direction>();

        // Random generator
        
        // Box drawing constants and other symbols.
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
        private const char EMPTY = ' ';
        private const int REGION_WD = 26;           //  Width / height of region holding single room.
        private const int REGION_HT = 8;
        private const int MAP_WD = 78;              // Max width / height of map display.
        private const int MAP_HT = 24;
        private const int MAX_ROOM_WT = 24;         // Based on screen width of 80, 78 allowed
        private const int MAX_ROOM_HT = 6;          // Based on screen height of 25, 24 allowed
        private const int MIN_ROOM_WT = 4;          // Minimum width / height of single room.
        private const int MIN_ROOM_HT = 4;
        private const int ROOM_CREATE_PCT = 90;       // Probability that room will be created for one region.
        private const int ROOM_EXIT_PCT = 90;       // Probability that room wall will contain exit.
        private const int ROOM_GOLD_PCT = 50;       // Probability that a room will have gold.

        // Array to hold map definition.
        private MapSpace[,] levelMap = new MapSpace[80, 25];
        
        // Random number generator
        private static Random rand = new Random();

        public MapSpace[,] LevelMap
        {
            // Make map available to other classes.
            get { return levelMap; }
        }

        public MapLevel()
        {
            // Constructor - generate a new map for this level.
            MapGeneration();

            while (!VerifyMapLINQ())
            {
                Debug.WriteLine(MapText());
                MapGeneration();
            }
        }

        private bool VerifyMap()
        {
            // Verify that the generate map is free of isolated rooms or sections.
            // Old version - kept for comparison.  See VerifyMapLINQ

            bool retValue = true;
            List<char> dirCheck = new List<char>();

            // Check horizontal for blank rows which no hallways. Top and bottom might be legitimately blank
            // so just check a portion of the map.

            for (int y = REGION_HT - MIN_ROOM_HT; y < (REGION_HT * 2) + MIN_ROOM_HT; y++)
            {
                    dirCheck.Clear();
                    for (int x = 0; x <= MAP_WD - 1; x++)
                    {
                        if (!dirCheck.Contains(levelMap[x, y].MapCharacter))
                            dirCheck.Add(levelMap[x, y].MapCharacter);
                    }
                    retValue = dirCheck.Count > 1;
                    if (!retValue) { break; }
            }

            // Check vertical.

            if (retValue)
            {
                for (int x = REGION_WD - MIN_ROOM_WT; x < (REGION_WD * 2) + MIN_ROOM_WT; x++)
                {
                    dirCheck.Clear();
                    for (int y = 0; y <= MAP_HT - 1; y++)
                    {
                        if (!dirCheck.Contains(levelMap[x, y].MapCharacter))
                            dirCheck.Add(levelMap[x, y].MapCharacter);
                    }
                    retValue = dirCheck.Count > 1;
                    if (!retValue) { break; }
                }
            }

            return retValue;
        }

        private bool VerifyMapLINQ()
        {
            // Verify that the generate map is free of isolated rooms or sections.
            // Alternate version using LINQ.

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

        private void MapGeneration()
        {
            // Primary generation procedure
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
                    if (rand.Next(101) <= ROOM_CREATE_PCT)
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

        private void AddStairway()
        {
            // Search the array randomly for an interior room space
            // and mark it as a hallway.
            List <MapSpace> openSpaces = FindOpenSpaces(false);
            MapSpace stairway = openSpaces[rand.Next(openSpaces.Count)];
            levelMap[stairway.X, stairway.Y] = new MapSpace(STAIRWAY, stairway.X, stairway.Y);
        }

        private void RoomGeneration(int westWallX, int northWallY, int roomWidth, int roomHeight)
        {
            // Create room on map based on inputs

            int eastWallX = westWallX + roomWidth;          // Calculate room east
            int southWallY = northWallY + roomHeight;       // Calculate room south

            // Regions are defined 1 to 9, L to R, top to bottom.
            int regionNumber = GetRegionNumber(westWallX, northWallY);
            int doorway = 0, doorCount = 0, goldX, goldY;

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
                if (regionNumber >= 4 && rand.Next(101) <= ROOM_EXIT_PCT)  // North doorways
                {
                    doorway = rand.Next(westWallX + 1, eastWallX);
                    levelMap[doorway, northWallY] = new MapSpace(ROOM_DOOR, false, false, doorway, northWallY);
                    levelMap[doorway, northWallY - 1] = new MapSpace(HALLWAY, false, false, doorway, northWallY - 1);
                    deadEnds.Add(levelMap[doorway, northWallY - 1], Direction.North);
                    doorCount++;
                }

                if (regionNumber <= 6 && rand.Next(101) <= ROOM_EXIT_PCT)  // South doorways
                {
                    doorway = rand.Next(westWallX + 1, eastWallX);
                    levelMap[doorway, southWallY] = new MapSpace(ROOM_DOOR, false, false, doorway, southWallY);
                    levelMap[doorway, southWallY + 1] = new MapSpace(HALLWAY, false, false, doorway, southWallY + 1);
                    deadEnds.Add(levelMap[doorway, southWallY + 1], Direction.South);
                    doorCount++;
                }

                if ("147258".Contains(regionNumber.ToString()) && rand.Next(101) <= ROOM_EXIT_PCT)  // East doorways
                {
                    doorway = rand.Next(northWallY + 1, southWallY);
                    levelMap[eastWallX, doorway] = new MapSpace(ROOM_DOOR, false, false, eastWallX, doorway);
                    levelMap[eastWallX + 1, doorway] = new MapSpace(HALLWAY, false, false, eastWallX + 1, doorway);
                    deadEnds.Add(levelMap[eastWallX + 1, doorway], Direction.East);
                    doorCount++;
                }

                if ("258369".Contains(regionNumber.ToString()) && rand.Next(101) <= ROOM_EXIT_PCT)  // West doorways
                {
                    doorway = rand.Next(northWallY + 1, southWallY);
                    levelMap[westWallX, doorway] = new MapSpace(ROOM_DOOR, false, false, westWallX, doorway);
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

            // Evaluate for a gold deposit

            if(rand.Next(1, 101) > ROOM_GOLD_PCT)
            {
                goldX = westWallX; goldY = northWallY;
                // Search the room randomly for an empty interior room space
                // and mark it as a gold deposit.
                while (levelMap[goldX, goldY].MapCharacter != ROOM_INT)
                {
                    goldX = rand.Next(westWallX + 1, eastWallX);
                    goldY = rand.Next(northWallY + 1, southWallY);
                }

                levelMap[goldX, goldY].ItemCharacter = GOLD;
            }
        }

        private void HallwayGeneration()
        {
            // After all rooms are generated with exits and initial hallway characters, scan for any possible disconnected
            // rooms and look for other rooms to connect to.
            Direction hallDirection = Direction.None; Direction direction90; Direction direction270;
            MapSpace hallwaySpace, newSpace;
            Dictionary<Direction, MapSpace> adjacentChars = new Dictionary<Direction, MapSpace>();
            Dictionary<Direction, MapSpace> surroundingChars = new Dictionary<Direction, MapSpace>();

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
                    
                    // Look for distant hallways in three directions.  If one is found, connect to it.
                    if (hallDirection != Direction.None)
                    {
                        surroundingChars = SearchAllDirections(hallwaySpace.X, hallwaySpace.Y);

                        switch (true)
                        {
                            // If there's a hallway in the distance in the current hallway direction, connect to it.
                            case true when (surroundingChars[hallDirection] != null && 
                                    surroundingChars[hallDirection].MapCharacter == HALLWAY):

                                DrawHallway(hallwaySpace, surroundingChars[hallDirection], hallDirection);
                                    deadEnds.Remove(hallwaySpace);

                                break;
                            // Then look to one side for a distant hallway.
                            case true when (surroundingChars[direction90] != null && 
                                    surroundingChars[direction90].MapCharacter == HALLWAY):

                                DrawHallway(hallwaySpace, surroundingChars[direction90], direction90);
                                    deadEnds.Remove(hallwaySpace);

                                break;
                            // Then look to the other.
                            case true when (surroundingChars[direction270] != null && 
                                    surroundingChars[direction270].MapCharacter == HALLWAY):

                                DrawHallway(hallwaySpace, surroundingChars[direction270], direction270);
                                    deadEnds.Remove(hallwaySpace);

                            break;
                            default:
                                // If there's no hallway to connect to, just add another space where possible for the
                                // next iteration to pick up on.
                                adjacentChars = SearchAdjacent(EMPTY, hallwaySpace.X, hallwaySpace.Y);
                                if (adjacentChars.ContainsKey(hallDirection))
                                {
                                    newSpace = new MapSpace(HALLWAY, adjacentChars[hallDirection]);
                                    levelMap[adjacentChars[hallDirection].X, adjacentChars[hallDirection].Y] = newSpace;
                                    deadEnds.Remove(hallwaySpace);
                                    deadEnds.Add(newSpace, hallDirection);
                                }
                                else if (adjacentChars.ContainsKey(direction90))
                                {
                                    newSpace = new MapSpace(HALLWAY, adjacentChars[direction90]);
                                    levelMap[adjacentChars[direction90].X, adjacentChars[direction90].Y] = newSpace;
                                    deadEnds.Remove(hallwaySpace);
                                    deadEnds.Add(newSpace, direction90);
                                }                                    
                                else if (adjacentChars.ContainsKey(direction270))
                                {
                                    newSpace = new MapSpace(HALLWAY, adjacentChars[direction270]);
                                    levelMap[adjacentChars[direction270].X, adjacentChars[direction270].Y] = newSpace;
                                    deadEnds.Remove(hallwaySpace);
                                    deadEnds.Add(newSpace, direction270);
                                }
                                break;
                        }
                    }
                    else
                    {
                        deadEnds.Remove(hallwaySpace);
                    }

                    //Console.Write(MapText());
                }
            }
        }        

        private void DrawHallway(MapSpace start, MapSpace end, Direction hallDirection)
        {

            // Draw a hallway between specified spaces.  Break off if another hallway
            // is discovered to the side.

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

        private Direction GetDirection90(Direction startingDirection)
        {
            // Return direction 90 degrees from original based on forward direction.
            Direction retValue = (Math.Abs((int)startingDirection) == 1) ? (Direction)2 : (Direction)1;
            return retValue;
        }

        private Direction GetDirection270(Direction startingDirection)
        {
            // Return direction 270 degrees from original (opposite of 90 degrees) based on forward direction.
            Direction retValue = (Math.Abs((int)startingDirection) == 1) ? (Direction)2 : (Direction)1;
            retValue = (Direction)((int)retValue * -1);
            return retValue;
        }

        public Dictionary<Direction, MapSpace> SearchAdjacent(char character, int x, int y)
        {

            // Search for specific character in four directions around point for a 
            // specific character. Return list of directions and characters found.

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

    public List<MapSpace> GetSurrounding(int x, int y)
    {
        // Return a list of all spaces around given space in eight directions.

        List<MapSpace> surrounding = (from MapSpace space in levelMap
                                    where Math.Abs(space.X - x) <= 1
                                    && Math.Abs(space.Y - y) <= 1
                                    select space).ToList();

        return surrounding;
    }

        public List<MapSpace> FindAllOccupants()
        {
            // Return a list of all monsters and the player by checking the
            // display character.

            List<MapSpace> occupants = (from MapSpace space in levelMap
                                            where space.DisplayCharacter != null
                                            select space).ToList();

            return occupants;
        }

        public List<MapSpace> FindAllItems()
        {
            // Return a list of all items on the map by checking the
            // item character.

            List<MapSpace> items = (from MapSpace space in levelMap
                                        where space.ItemCharacter != null
                                        select space).ToList();
            
            return items;
        }

        public List<MapSpace> FindOpenSpaces(bool hallways)
        {
            // Return a list of all open spaces on the map by checking the
            // map character.
            string charList = hallways ? (HALLWAY.ToString() + ROOM_INT.ToString()) : ROOM_INT.ToString();

            List<MapSpace> spaces = (from MapSpace space in levelMap
                                    where charList.Contains(space.MapCharacter)
                                    && space.ItemCharacter == null
                                    && space.DisplayCharacter == null
                                    select space).ToList();

            return spaces;
        }

        public Dictionary<Direction, MapSpace> SearchAdjacent(int x, int y)
        {
            // Search in four directions around point. Return list of directions and characters found.

            Dictionary<Direction, MapSpace> retValue = new Dictionary<Direction, MapSpace>();
            retValue.Add(Direction.North, levelMap[x, y - 1]);
            retValue.Add(Direction.East, levelMap[x + 1, y]);
            retValue.Add(Direction.South, levelMap[x, y + 1]);
            retValue.Add(Direction.West, levelMap[x - 1, y]);

            return retValue;
        }

        public Dictionary<Direction, MapSpace> SearchAllDirections(int currentX, int currentY)
        {
            // Look in all directions and return a Dictionary of the first non-space characters found.
            Dictionary<Direction, MapSpace> retValue = new Dictionary<Direction, MapSpace>();

            retValue.Add(Direction.North, SearchDirection(Direction.North, currentX, currentY - 1));
            retValue.Add(Direction.South, SearchDirection(Direction.South, currentX, currentY + 1));
            retValue.Add(Direction.East, SearchDirection(Direction.East, currentX + 1, currentY));
            retValue.Add(Direction.West, SearchDirection(Direction.West, currentX - 1, currentY));

            return retValue;
        }

        public MapSpace SearchDirection(Direction direction, int startX, int startY)
        {
            // Get the next non-space object found in a given direction.
            // Return null if none is found.
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

        public MapSpace PlaceMapCharacter(char MapChar, bool Living)
        {
            // Find a random space within one of the rooms that 
            // hasn't been occupied and return the array reference.
            // This version searches by poking the map randomly. Kept for comparison.
            // See PlaceMapCharacterLINQ()

            int xPos = 1, yPos = 1;
            bool freeSpace = false;

            while (!freeSpace)
            {
                xPos = rand.Next(1, MAP_WD);
                yPos = rand.Next(1, MAP_HT);

                freeSpace = (levelMap[xPos, yPos].MapCharacter == ROOM_INT)
                    && levelMap[xPos, yPos].DisplayCharacter == null
                    && levelMap[xPos, yPos].ItemCharacter == null;
            }

            // If the character is for the player or a monster, add
            // it to the Display character. Otherwise, use the item character.
            if (Living) 
                levelMap[xPos, yPos].DisplayCharacter = MapChar;
            else
                levelMap[xPos, yPos].ItemCharacter = MapChar;

            return levelMap[xPos, yPos];
        }

        public MapSpace PlaceMapCharacterLINQ(char MapChar, bool Living)
        {
            // Find a random space within one of the rooms that 
            // hasn't been occupied and return the array reference.
            // This version uses LINQ to get a list of the open spaces.

            MapSpace select;

            List<MapSpace> spaces = FindOpenSpaces(false);
            
            select = spaces[rand.Next(0, spaces.Count)];                 

            // If the character is for the player or a monster, add
            // it to the Display character. Otherwise, use the item character.
            if (Living)
                select.DisplayCharacter = MapChar;
            else
                select.ItemCharacter = MapChar;

            return select;
        }

        public MapSpace MoveDisplayItem(MapSpace Start, MapSpace Destination)
        {
            // Change the display character for the specified map space
            // and return the reference as a confirmation.
            levelMap[Destination.X, Destination.Y].DisplayCharacter = Start.DisplayCharacter;
            levelMap[Start.X, Start.Y].DisplayCharacter = null;

            return Destination;
        }

        public int GetRegionNumber(int RoomAnchorX, int RoomAnchorY)
        {
            // The map is divided into a 3 x 3 grid of 9 equal regions.
            // This function returns 1 to 9 to indicate where the region is on the map.

            int returnVal;

            int regionX = ((int)RoomAnchorX / REGION_WD) + 1;
            int regionY = ((int)RoomAnchorY / REGION_HT) + 1;

            returnVal = (regionX) + ((regionY - 1) * 3);

            return returnVal;
        }

        public string MapText()
        {
            // Output the array to text for display.
            StringBuilder sbReturn = new StringBuilder();

            // Iterate through the two-dimensional array and use StringBuilder to 
            // concatenate the proper characters into rows and columns for display.

            for (int y = 0; y <= MAP_HT; y++)
            {
                for (int x = 0; x <= MAP_WD; x++)
                    if(levelMap[x, y].Visible)
                    {
                        // Prioritize, DisplayCharacter, ItemCharacter and then MapCharacter.
                        if (levelMap[x, y].DisplayCharacter != null)
                            sbReturn.Append(levelMap[x, y].DisplayCharacter);
                        else if (levelMap[x, y].ItemCharacter != null)
                            sbReturn.Append(levelMap[x, y].ItemCharacter);
                        else
                            sbReturn.Append(levelMap[x, y].MapCharacter);
                    }
                    else
                    {
                        // If space is not set to visible, just insert a blank.
                        sbReturn.Append(' ');
                    }
                sbReturn.Append("\n");     // Start new line.           
            }

            return sbReturn.ToString();
        }
    }


    internal class MapSpace{
        public char MapCharacter { get; set; } // Actual character on map (Room interior, hallway, wall, etc..).
        public char? ItemCharacter { get; set; } // Item sitting on map (potion, scroll, etc..).
        public char? DisplayCharacter { get; set; }  // Displayed character - override for mimics and hidden.
        public bool SearchRequired { get; set; }  // Does the player need to search to reveal?
        public bool Visible { get; set; } // Is space supposed to be visible.
        public int X { get; set; }
        public int Y { get; set; }

        public MapSpace()
        {
            // Create blank space for map
            this.MapCharacter = ' ';
            this.ItemCharacter = null;
            this.DisplayCharacter = null;
            this.SearchRequired = false;
            this.Visible = true;
            X = 0;
            Y = 0;
        }

        public MapSpace(char mapChar, MapSpace oldSpace)
        {
            this.MapCharacter = mapChar;
            this.ItemCharacter = null;
            this.DisplayCharacter = null;
            this.SearchRequired = oldSpace.SearchRequired;
            this.X = oldSpace.X; this.Y = oldSpace.Y; this.Visible = oldSpace.Visible;   
        }

        public MapSpace(char mapChar, int X, int Y)
        {
            // Create visible character
            this.MapCharacter = mapChar;
            this.ItemCharacter = null;
            this.DisplayCharacter = null;
            this.SearchRequired = false;
            this.Visible = true;
            this.X = X;
            this.Y = Y;
        }

        public MapSpace(char mapChar, Boolean hidden, Boolean search, int X, int Y)
        {
            this.MapCharacter = mapChar;
            this.ItemCharacter = null;
            this.DisplayCharacter = null;
            this.SearchRequired = search;
            this.Visible = !hidden;
            this.X = X;
            this.Y = Y;
        }
    }
}

