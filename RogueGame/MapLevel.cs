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

namespace RogueGame{

    internal class MapLevel
    {

        private enum Direction{
            None = 0,
            North = 1,
            East = 2,
            South = -1,
            West = -2
        }

        // Box drawing constants and other symbols.
        private const char HORIZONTAL = '═';
        private const char VERTICAL = '║';
        private const char CORNER_NW = '╔';
        private const char CORNER_SE = '╝';
        private const char CORNER_NE = '╗';
        private const char CORNER_SW = '╚';
        private const char ROOM_INT = '.';
        private const char ROOM_DOOR = '╬';
        private const char HALLWAY = '▓';
        private const char EMPTY = ' ';
        private const int REGION_WD = 26;
        private const int REGION_HT = 8;
        private const int MAP_WD = 79;
        private const int MAP_HT = 24;
        private const int MAX_ROOM_WT = 24;  // Based on screen width of 80, 78 allowed
        private const int MAX_ROOM_HT = 6;   // Based on screen height of 25, 24 allowed
        private const int MIN_ROOM_WT = 4;
        private const int MIN_ROOM_HT = 4;
        private const int ROOM_SKIP_PCT = 10;
        private const int ROOM_EXIT_PCT = 85;

        private MapSpace[,] levelMap = new MapSpace[80, 25];   // Array to hold map definition.
        private List<MapSpace> deadEnds = new List<MapSpace>();  // Holds hallway endings during map generation.

        public MapSpace[,] LevelMap
        {
            // Read-only - Only class should be able to edit map.
            get { return levelMap; }
        
        }
        public MapLevel()
        {
            // Generate a new map for this level.
            MapGeneration();

        }


        private void MapGeneration()
        {
            var rand = new Random();
            int roomWidth = 0, roomHeight = 0, roomAnchorX = 0, roomAnchorY = 0;

            // Primary generation procedure
            // Screen is divided into nine cell regions and a room is randomly generated in each.
            // Room exterior must be at least five spaces in each direction but not more than the
            // size of its cell region, minus one space, to allow for hallways between rooms.

            // Clear map
            levelMap = new MapSpace[80, 25];

            // Use the for statements to count cells L to R and top to bottom.
            // Increment the count based on a third of the way in each direction.
            // First row and first column of array are skipped so everything is 1 based.
            for (int y = 1; y < 18; y += REGION_HT)
            {
                for (int x = 1; x < 54; x += REGION_WD)
                {
                    if (rand.Next(101) > ROOM_SKIP_PCT)        //10% chance of not creating room
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

            // Fill in the blanks for the remaining cells.
            for (int y = 0; y < 25; y++)
            {
                for (int x = 0; x < 80; x++)
                {
                    if (levelMap[x,y] is null)
                        levelMap[x, y] = new MapSpace(' ', false, false, x, y);
                }
            }


            // Create hallways
            HallwayGeneration();

        }

        private void RoomGeneration(int westWallX, int northWallY, int roomWidth, int roomHeight)
        {
            // Create room on map based on inputs

            int eastWallX = westWallX + roomWidth;
            int southWallY = northWallY + roomHeight;
            int regionNumber = GetRegionNumber(westWallX, northWallY);
            int doorway = 0; 
            Dictionary<Direction, MapSpace> doorWays = new Dictionary<Direction, MapSpace>();
            var rand = new Random(DateTime.Now.Millisecond);


            for (int y = northWallY; y < (southWallY + 1); y++)
            {
                for (int x = westWallX; x < (eastWallX + 1); x++)
                {
                    if (y == northWallY || y == southWallY)
                    {
                        levelMap[x, y] = new MapSpace(HORIZONTAL, false, false, x, y);
                    }
                    
                    if (x == westWallX || x == eastWallX)
                    {
                        levelMap[x, y] = new MapSpace(VERTICAL, false, false, x, y);
                    }
                }
            }

            // Fill in any blanks.  Null spaces cause issues.
            for (int y = northWallY; y < (southWallY + 1); y++)
            {
                for (int x = westWallX; x < (eastWallX + 1); x++)
                {
                    if (levelMap[x, y] == null)
                        levelMap[x, y] = new MapSpace(ROOM_INT, false, false, x, y);
                }
            }

            // Add doorways
            if (regionNumber >= 4)  // North doorways
            {
                if (rand.Next(100) <= ROOM_EXIT_PCT)
                {
                    doorway = rand.Next(westWallX + 1, eastWallX - 1);
                    levelMap[doorway, northWallY] = new MapSpace(ROOM_DOOR, false, false, doorway, northWallY);
                    doorWays.Add(Direction.North, levelMap[doorway, northWallY]);
                }
            }

            if (regionNumber <= 6)  // South doorways
            {
                if (rand.Next(100) <= ROOM_EXIT_PCT)
                {
                    doorway = rand.Next(westWallX + 1, eastWallX - 1);
                    levelMap[doorway, southWallY] = new MapSpace(ROOM_DOOR, false, false, doorway, southWallY);
                    doorWays.Add(Direction.South, levelMap[doorway, southWallY]);
                }
            }

            if ("147258".Contains(regionNumber.ToString()))  // East doorways
            {
                if (rand.Next(100) <= ROOM_EXIT_PCT)
                {
                    doorway = rand.Next(northWallY + 1, southWallY - 1);
                    levelMap[eastWallX, doorway] = new MapSpace(ROOM_DOOR, false, false, eastWallX, doorway);
                    doorWays.Add(Direction.East, levelMap[eastWallX, doorway]);
                }
            }

            if ("258369".Contains(regionNumber.ToString()))  // West doorways
            {
                if (rand.Next(100) <= ROOM_EXIT_PCT)
                {
                    doorway = rand.Next(northWallY + 1, southWallY - 1);
                    levelMap[westWallX, doorway] = new MapSpace(ROOM_DOOR, false, false, westWallX, doorway);
                    doorWays.Add(Direction.West, levelMap[westWallX, doorway]);
                }
            }         


            // Add a hallway character or two for every door and add the characters to the deadEnds list for further generation.

            foreach(KeyValuePair<Direction, MapSpace> entry in doorWays)
            {
                switch (entry.Key)
                {
                    case Direction.North:     // North
                        levelMap[entry.Value.X, entry.Value.Y - 1] = new MapSpace(HALLWAY, false, false, entry.Value.X, entry.Value.Y - 1);
                        deadEnds.Add(levelMap[entry.Value.X, entry.Value.Y - 1]);
                        break;
                    case Direction.South:     // South
                        levelMap[entry.Value.X, entry.Value.Y + 1] = new MapSpace(HALLWAY, false, false, entry.Value.X, entry.Value.Y + 1);
                        deadEnds.Add(levelMap[entry.Value.X, entry.Value.Y + 1]);
                        break;
                    case Direction.East:     // East
                        levelMap[entry.Value.X + 1, entry.Value.Y] = new MapSpace(HALLWAY, false, false, entry.Value.X + 1, entry.Value.Y);
                        deadEnds.Add(levelMap[entry.Value.X + 1, entry.Value.Y]);
                        break;
                    case Direction.West:     // West
                        levelMap[entry.Value.X - 1, entry.Value.Y] = new MapSpace(HALLWAY, false, false, entry.Value.X - 1, entry.Value.Y);
                        deadEnds.Add(levelMap[entry.Value.X - 1, entry.Value.Y]);                        
                        break;
                    default:

                break;
                }
            }    

            // Set the room corners.
     
            levelMap[westWallX, northWallY] = new MapSpace(CORNER_NW, false, false, westWallX, northWallY);
            levelMap[eastWallX, northWallY] = new MapSpace(CORNER_NE, false, false, eastWallX, northWallY);
            levelMap[westWallX, southWallY] = new MapSpace(CORNER_SW, false, false, westWallX, southWallY);
            levelMap[eastWallX, southWallY] = new MapSpace(CORNER_SE, false, false, eastWallX, southWallY);
        }


        private void HallwayGeneration()
        {
            // After all rooms are generated with exits and initial hallway characters, scan for any possible disconnected
            // rooms and look for other rooms to connect to.
            int roomRegion = 0;
            Direction hallDirection = Direction.None;
            MapSpace hallwaySpace;
            Dictionary<Direction, MapSpace> adjacentChars = new Dictionary<Direction, MapSpace>();
            Dictionary<Direction, MapSpace> surroundingChars = new Dictionary<Direction, MapSpace>();

            for (int i = deadEnds.Count - 1; i >= 0; i--)
            {
                hallwaySpace = deadEnds[i];
                // Get region number and hallway direction to know which direction to move in.
                roomRegion = GetRegionNumber(hallwaySpace.X, hallwaySpace.Y);
                adjacentChars = SearchAdjacent("" + ROOM_DOOR, hallwaySpace.X, hallwaySpace.Y);

                // If there are doors on more than one side, the hallway is already connected.
                // Otherwise, get its direction.
                    
                if (adjacentChars.Count == 1)
                    hallDirection = (Direction)((int)adjacentChars.ElementAt(0).Key * -1);


                while (hallDirection != Direction.None)
                {
                    // Search in four directions to decide where to move.
                    surroundingChars = SearchAllDirections(hallwaySpace.X, hallwaySpace.Y);
                    // Remove opposite direction of current hallway. We're not doubling back.
                    //surroundingChars.Remove((Direction)((int)hallDirection * -1)); 

                    switch(hallDirection)
                    {
                        case Direction.North:
                            if (surroundingChars[Direction.North].MapCharacter == HALLWAY)
                                DrawHallway(hallwaySpace, surroundingChars[Direction.North]);
                            break;
                        case Direction.South:
                            if (surroundingChars[Direction.South].MapCharacter == HALLWAY)
                                DrawHallway(hallwaySpace, surroundingChars[Direction.South]);
                            break;
                        case Direction.West:
                            if (surroundingChars[Direction.West].MapCharacter == HALLWAY)
                                DrawHallway(hallwaySpace, surroundingChars[Direction.West]);
                            break;
                        case Direction.East:
                            if (surroundingChars[Direction.East].MapCharacter == HALLWAY)
                                DrawHallway(hallwaySpace, surroundingChars[Direction.East]);
                            break;
                    }

                    hallDirection = Direction.None;
                    deadEnds.RemoveAt(i);
                }



            }
            
        }

        private void DrawHallway(MapSpace start, MapSpace end)
        {

            if(start.X == end.X)
            {
                if(start.Y < end.Y)
                {
                    for (int y = start.Y; y < end.Y; y++)
                    levelMap[start.X, y] = new MapSpace(HALLWAY, start.X, y);
                }
                else if (start.Y > end.Y)
                {
                    for (int y = start.Y; y > end.Y; y--)
                    levelMap[start.X, y] = new MapSpace(HALLWAY, start.X, y);
                }
            }
            else if(start.Y == end.Y)
            {
                if (start.X < end.X)
                {
                    for (int x = start.X; x < end.X; x++)
                    levelMap[x, start.Y] = new MapSpace(HALLWAY, x, start.Y);
                }
                else if (start.X > end.X)
                { 
                    for (int x = start.X; x > end.X; x--)
                    levelMap[x, start.Y] = new MapSpace(HALLWAY, x, start.Y);
                }
            }
        }

        private Dictionary<Direction, MapSpace> SearchAdjacent(string characters, int x, int y)
        {

            // Search for specific character in four directions around point.
            // Return list of directions and characters found.

            Dictionary<Direction, MapSpace> retValue = new Dictionary<Direction, MapSpace>(); 

            foreach (char c in characters)
            {
                if (y - 1 >= 0 && levelMap[x, y - 1].MapCharacter == c)  // North
                    retValue.Add(Direction.North, levelMap[x, y - 1]);

                if (x + 1 <= MAP_WD && levelMap[x + 1, y].MapCharacter == c) // East
                    retValue.Add(Direction.East, levelMap[x + 1, y]);

                if (y + 1 <= MAP_HT && levelMap[x, y + 1].MapCharacter == c)  // South
                    retValue.Add(Direction.South, levelMap[x, y + 1]);

                if ((x - 1) >= 0 && levelMap[x - 1, y].MapCharacter == c)  // West
                    retValue.Add(Direction.West, levelMap[x - 1, y]);
            }
            

            return retValue;
        }

        private Dictionary<Direction, MapSpace> SearchAllDirections(int currentX, int currentY)
        {
            // Look in all directions and return a Dictionary of the first non-space characters found.
            Dictionary<Direction, MapSpace> retValue = new Dictionary<Direction, MapSpace>();

            retValue.Add(Direction.North, SearchToNextObject(Direction.North, currentX, currentY - 1));
            retValue.Add(Direction.South, SearchToNextObject(Direction.South, currentX, currentY + 1));
            retValue.Add(Direction.East, SearchToNextObject(Direction.East, currentX + 1, currentY));
            retValue.Add(Direction.West, SearchToNextObject(Direction.West, currentX - 1, currentY));

            return retValue;

        }

        private MapSpace SearchToNextObject(Direction direction, int startX, int startY)
        {
            // Get the next non-space object found in a given direction.
            MapSpace retValue = levelMap[startX, startY];
            bool edge = false;
            int currentX = startX, currentY = startY;

            switch (direction)
            {
                case Direction.North:
                    while(retValue.MapCharacter == ' ' && !edge)
                    {
                        retValue = levelMap[currentX, currentY];
                        if (currentY -1 > 0)
                            currentY--;
                        else edge= true;
                    }
                    break;
                case Direction.East:
                    while (retValue.MapCharacter == ' ' && !edge)
                    {
                        retValue = levelMap[currentX, currentY];
                        if (currentX + 1 <= MAP_WD)
                            currentX++;
                        else edge = true;
                    }
                    break;
                case Direction.South:
                    while (retValue.MapCharacter == ' ' && !edge)
                    {
                        retValue = levelMap[currentX, currentY];
                        if (currentY + 1 <= MAP_HT)
                            currentY++;
                        else edge = true;
                    }
                    break;
                case Direction.West:
                    while (retValue.MapCharacter == ' ' && !edge)
                    {
                        retValue = levelMap[currentX, currentY];
                        if (currentX - 1 <= 0)
                            currentX--;
                        else edge = true;
                    }
                    break;
            }

            return retValue;
        }

        private bool SearchToNextObject(char item, Direction direction, int x, int y, int spaces)
        {

            // Search for specific character over a given number of spaces.  Return true if found before any other non-space character.
            bool retValue = true;

            for (int i = 1; i <= spaces; i++)
            {
                switch (direction)
                {
                    case Direction.North:
                        retValue = (y - i >= 0 && (levelMap[x, y - i].MapCharacter == item || levelMap[x, y - i].MapCharacter == ' '));
                        break;
                    case Direction.East:
                        retValue = (x + i <= 24 && (levelMap[x + i, y].MapCharacter == item || levelMap[x + i, y].MapCharacter == ' '));
                        break;
                    case Direction.South:
                        retValue = (y + i <= 24 && (levelMap[x, y + i].MapCharacter == item || levelMap[x, y + i].MapCharacter == ' '));
                        break;
                    case Direction.West:
                        retValue = ((x - i) >= 0 && (levelMap[x - i, y].MapCharacter == item || levelMap[x - i, y].MapCharacter == ' '));
                        break;
                }
                if(!retValue)  // Something else has been found.
                    break;
            }

            return retValue;
        }        

        private MapSpace SearchByDistance(char item, Direction direction, int x, int y, int spaces)
        {

            // Search for specific character a given number of spaces, stopping at edge if necessary.
            MapSpace retValue = new MapSpace();

            for (int i = 1; i <= spaces; i++)
            {
                switch (direction)
                {
                    case Direction.North:
                        if (y - i >= 0 && levelMap[x, y - i].MapCharacter == item)
                            retValue = levelMap[x, y - i];
                        break;
                    case Direction.East:
                        if (x + i <= 24 && levelMap[x + i, y].MapCharacter == item)
                            retValue = levelMap[x + i, y];
                        break;
                    case Direction.South:
                        if (y + i <= 24 && levelMap[x, y + i].MapCharacter == item)
                            retValue = levelMap[x, y + i];
                        break;
                    case Direction.West:
                        if ((x - i) >= 0 && levelMap[x - i, y].MapCharacter == item)
                            retValue = levelMap[x - i, y];
                        break;
                }
            }

            return retValue;
        }

        private int GetRegionNumber(int RoomAnchorX, int RoomAnchorY)
        {
            // The map is divided into a 3 x 3 grid of 9 equal regions.
            // This function returns 1 to 9 to indicate where the region is on the map.

            int returnVal;

            int regionX = ((int)RoomAnchorX / 26) + 1;
            int regionY = ((int)RoomAnchorY / 8) + 1;

            returnVal = (regionX) + ((regionY - 1) * 3);

            return returnVal;
        }

        public string MapRow(int rowNumber)
        {
            string retValue = "";

            // Return the specified row from the array in string form for display.
            for (int x = 0; x < 80; x++)
            {
                retValue += levelMap[x, rowNumber].DisplayCharacter;
            }
            // Add new line character.
            retValue += "\n";

            return retValue;
        }


        internal class MapSpace{
            private char mapChar;           // Actual character on map.
            private char displayChar;       // Displayed character - space for hidden object.
            private bool searchToDisp;      // Does the player need to search to reveal?
            private int xc;                   // X coordinate
            private int yc;                  // Y coordinate
            
            public char MapCharacter
            {
                get { return mapChar; }
                set { mapChar = value; }
            }

            public char DisplayCharacter
            {
                get { return displayChar; }
                set { displayChar = value; }
            }
            public bool SearchRequired
            {
                get { return searchToDisp; }
            }

            public int X
            {
                get { return xc; }
                set { xc = value; }
            }

            public int Y
            {
                get { return yc; }
                set { yc = value; }
            }

            public MapSpace()
            {
                // Create blank space for map
                mapChar = ' ';
                displayChar = ' ';
                searchToDisp = false;
                xc = 0;
                yc = 0;
            }

            public MapSpace(char mapChar, int X, int Y)
            {
                // Create visible character
                this.mapChar = mapChar;
                this.displayChar = false ? ' ' : mapChar;
                this.searchToDisp = false;
                this.xc = X;
                this.yc = Y;
            }

            public MapSpace(char mapChar, Boolean hidden, Boolean search, int X, int Y)
            {
                this.mapChar = mapChar;
                this.displayChar = hidden ? ' ' : mapChar;
                this.searchToDisp = search;
                this.xc = X;
                this.yc = Y; 
            }
         }
    }
}
