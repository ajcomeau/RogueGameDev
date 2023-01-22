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
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace RogueGame{

    internal class MapLevel
    {

        private enum Direction {
            None = 0,
            North = 1,
            East = 2,
            South = -1,
            West = -2
        }

        // Box drawing constants and other symbols.
        private const char HORIZONTAL = '═';        // Unicode symbols can be copy-pasted from https://www.w3.org/TR/xml-entity-names/025.html.  
        private const char VERTICAL = '║';
        private const char CORNER_NW = '╔';
        private const char CORNER_SE = '╝';
        private const char CORNER_NE = '╗';
        private const char CORNER_SW = '╚';
        private const char ROOM_INT = '.';
        private const char ROOM_DOOR = '╬';
        private const char HALLWAY = '▓';
        private const char EMPTY = ' ';
        private const int REGION_WD = 26;           //  Width / height of region holding single room.
        private const int REGION_HT = 8;
        private const int MAP_WD = 78;              // Max width / height of map display.
        private const int MAP_HT = 24;
        private const int MAX_ROOM_WT = 24;         // Based on screen width of 80, 78 allowed
        private const int MAX_ROOM_HT = 6;          // Based on screen height of 25, 24 allowed
        private const int MIN_ROOM_WT = 4;          // Minimum width / height of single room.
        private const int MIN_ROOM_HT = 4;
        private const int ROOM_SKIP_PCT = 10;       // Probability that room creation will be skipped for one region.
        private const int ROOM_EXIT_PCT = 95;       // Probability that room wall will contain exit.

        // Array to hold map definition.
        private MapSpace[,] levelMap = new MapSpace[80, 25];
        // Dictionary to hold hallway endings during map generation.
        private Dictionary<MapSpace, Direction> deadEnds = new Dictionary<MapSpace, Direction>();  

        public MapSpace[,] LevelMap
        {
            // Read-only - The map is available to other classes but only
            // the class should be able to edit.
            get { return levelMap; }
        }

        public MapLevel()
        {
            // Constructor - generate a new map for this level.
            MapGeneration();

        }

        private void MapGeneration()
        {
            // Primary generation procedure
            // Screen is divided into nine cell regions and a room is randomly generated in each.
            // Room exterior must be at least four spaces in each direction but not more than the
            // size of its cell region, minus one space, to allow for hallways between rooms.
            
            var rand = new Random();
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
                    if (rand.Next(101) > ROOM_SKIP_PCT)
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

            // After the rooms are generated, fill in the blanks for the remaining cells.
            for (int y = 0; y < 25; y++)
            {
                for (int x = 0; x < 80; x++)
                {
                    if (levelMap[x, y] is null)
                        levelMap[x, y] = new MapSpace(' ', false, false, x, y);
                }
            }


            // Create hallways
            HallwayGeneration();

        }

        private void RoomGeneration(int westWallX, int northWallY, int roomWidth, int roomHeight)
        {
            // Create room on map based on inputs

            int eastWallX = westWallX + roomWidth;          // Calculate room width
            int southWallY = northWallY + roomHeight;       // Calculate room height

            // Regions are defined 1 to 9, L to R, top to bottom.
            int regionNumber = GetRegionNumber(westWallX, northWallY);
            int doorway = 0;
            
            // Remember the exits created.
            Dictionary<Direction, MapSpace> doorWays = new Dictionary<Direction, MapSpace>();
            var rand = new Random();

            // Create horizontal and vertical walls for room.
            for (int y = northWallY; y <= southWallY; y++)
            {
                for (int x = westWallX; x <= eastWallX; x++)
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

            // Fill in any blank spaces in array.
            for (int y = northWallY; y <= southWallY; y++)
            {
                for (int x = westWallX; x <= eastWallX; x++)
                {
                    if (levelMap[x, y] == null)
                        levelMap[x, y] = new MapSpace(ROOM_INT, false, false, x, y);
                }
            }

            // Add doorways on room. Room walls facing the edges of the map do not get exits
            // so the ROOM_EXIT_PCT constant needs to be high to ensure that every room gets
            // at least one.

            if (regionNumber >= 4)  // North doorways
            {
                if (rand.Next(101) <= ROOM_EXIT_PCT)
                {
                    doorway = rand.Next(westWallX + 1, eastWallX);
                    levelMap[doorway, northWallY] = new MapSpace(ROOM_DOOR, false, false, doorway, northWallY);
                    doorWays.Add(Direction.North, levelMap[doorway, northWallY]);
                }
            }

            if (regionNumber <= 6)  // South doorways
            {
                if (rand.Next(101) <= ROOM_EXIT_PCT)
                {
                    doorway = rand.Next(westWallX + 1, eastWallX);
                    levelMap[doorway, southWallY] = new MapSpace(ROOM_DOOR, false, false, doorway, southWallY);
                    doorWays.Add(Direction.South, levelMap[doorway, southWallY]);
                }
            }

            if ("147258".Contains(regionNumber.ToString()))  // East doorways
            {
                if (rand.Next(101) <= ROOM_EXIT_PCT)
                {
                    doorway = rand.Next(northWallY + 1, southWallY);
                    levelMap[eastWallX, doorway] = new MapSpace(ROOM_DOOR, false, false, eastWallX, doorway);
                    doorWays.Add(Direction.East, levelMap[eastWallX, doorway]);
                }
            }

            if ("258369".Contains(regionNumber.ToString()))  // West doorways
            {
                if (rand.Next(101) <= ROOM_EXIT_PCT)
                {
                    doorway = rand.Next(northWallY + 1, southWallY);
                    levelMap[westWallX, doorway] = new MapSpace(ROOM_DOOR, false, false, westWallX, doorway);
                    doorWays.Add(Direction.West, levelMap[westWallX, doorway]);
                }
            }


            // Add a hallway character for every door and add the characters to the deadEnds list for further generation.

            foreach (KeyValuePair<Direction, MapSpace> entry in doorWays)
            {
                switch (entry.Key)
                {
                    case Direction.North:     // North
                        levelMap[entry.Value.X, entry.Value.Y - 1] = new MapSpace(HALLWAY, false, false, entry.Value.X, entry.Value.Y - 1);
                        deadEnds.Add(levelMap[entry.Value.X, entry.Value.Y - 1], entry.Key);
                        break;
                    case Direction.South:     // South
                        levelMap[entry.Value.X, entry.Value.Y + 1] = new MapSpace(HALLWAY, false, false, entry.Value.X, entry.Value.Y + 1);
                        deadEnds.Add(levelMap[entry.Value.X, entry.Value.Y + 1], entry.Key);
                        break;
                    case Direction.East:     // East
                        levelMap[entry.Value.X + 1, entry.Value.Y] = new MapSpace(HALLWAY, false, false, entry.Value.X + 1, entry.Value.Y);
                        deadEnds.Add(levelMap[entry.Value.X + 1, entry.Value.Y], entry.Key);
                        break;
                    case Direction.West:     // West
                        levelMap[entry.Value.X - 1, entry.Value.Y] = new MapSpace(HALLWAY, false, false, entry.Value.X - 1, entry.Value.Y);
                        deadEnds.Add(levelMap[entry.Value.X - 1, entry.Value.Y], entry.Key);
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
            Direction hallDirection = Direction.None; Direction direction90; Direction direction270;
            MapSpace hallwaySpace, newSpace;
            Dictionary<Direction, MapSpace> adjacentChars = new Dictionary<Direction, MapSpace>();
            Dictionary<Direction, MapSpace> surroundingChars = new Dictionary<Direction, MapSpace>();
            var rand = new Random();

            // Iterate through the list of hallway endings until all are resolved one way or another.

            for (int i = deadEnds.Count - 1; i >= 0; i--)  // Count backwards so we can remove processed items.
            {

                // If there are doors on more than one side, the hallway is already connected.
                // Otherwise, get its direction and a 90 degree turn for reference.
                hallwaySpace = deadEnds.ElementAt(i).Key;

                if (SearchAdjacent("" + ROOM_DOOR, hallwaySpace.X, hallwaySpace.Y).Count > 1)
                    deadEnds.Remove(hallwaySpace);
            }

            while (deadEnds.Count > 0)
            {
                for (int i = deadEnds.Count - 1; i >= 0; i--)  // Count backwards so we can remove processed items.
                {
                    // If there's a neighboring hallway space, this one is already connected.
                    hallwaySpace = deadEnds.ElementAt(i).Key;

                    if (SearchAdjacent("" + HALLWAY, hallwaySpace.X, hallwaySpace.Y).Count > 1)
                        deadEnds.Remove(hallwaySpace);
                }

                for (int i = deadEnds.Count - 1; i >= 0; i--)  // Count backwards so we can remove processed items.
                {
                    hallwaySpace = deadEnds.ElementAt(i).Key;
                    hallDirection = deadEnds.ElementAt(i).Value;
                    direction90 = (Math.Abs((int)hallDirection) == 1) ? (Direction)2 : (Direction)1;
                    direction270 = (Direction)((int)direction90 * -1);
                    
                    // Look for distant hallways in all directions.
                    if (hallDirection != Direction.None)
                    {
                        surroundingChars = SearchAllDirections(hallwaySpace.X, hallwaySpace.Y);

                        switch (true)
                        {
                            case true when (surroundingChars[hallDirection].MapCharacter == HALLWAY):
                                Debug.Print($"Drawing hallway from {hallwaySpace.X}, {hallwaySpace.Y} to " +
                                    $"{surroundingChars[hallDirection].X}, {surroundingChars[hallDirection].Y}.");
                                DrawHallway(hallwaySpace, surroundingChars[hallDirection]);
                                deadEnds.Remove(hallwaySpace);
                            break;
                            case true when (surroundingChars[direction90].MapCharacter == HALLWAY):
                                Debug.Print($"Drawing hallway from {hallwaySpace.X}, {hallwaySpace.Y} to " +
                                    $"{surroundingChars[direction90].X}, {surroundingChars[direction90].Y}.");
                                DrawHallway(hallwaySpace, surroundingChars[direction90]);
                                deadEnds.Remove(hallwaySpace);
                            break;
                            case true when (surroundingChars[direction270].MapCharacter == HALLWAY):
                                Debug.Print($"Drawing hallway from {hallwaySpace.X}, {hallwaySpace.Y} to " +
                                    $"{surroundingChars[direction270].X}, {surroundingChars[direction270].Y}.");
                                DrawHallway(hallwaySpace, surroundingChars[direction270]);
                                deadEnds.Remove(hallwaySpace);
                            break;
                            default:
                                // If there's no hallway to connect to, just add another space for the next iteration to pick up on.
                                adjacentChars = SearchAdjacent("" + EMPTY, hallwaySpace.X, hallwaySpace.Y);
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
                    Console.Write(MapText());
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
            MapSpace retValue;
            int currentX = startX, currentY = startY;

            currentY = (currentY > MAP_HT) ? MAP_HT : currentY; 
            currentY = (currentY < 0) ? 0 : currentY;
            currentX = (currentX > MAP_WD) ? MAP_WD : currentX; 
            currentX = (currentX < 0) ? 0 : currentX;

            retValue = levelMap[currentX, currentY];

            switch (direction)
            {
                case Direction.North:
                    while(retValue.MapCharacter == ' ' && currentY > 0)
                    {
                        retValue = levelMap[currentX, currentY];
                        currentY--;
                    }
                    break;
                case Direction.East:
                    while (retValue.MapCharacter == ' ' && currentX <= MAP_WD)
                    {
                        retValue = levelMap[currentX, currentY];
                        currentX++;
                    }
                    break;
                case Direction.South:
                    while (retValue.MapCharacter == ' ' && currentY <= MAP_HT)
                    {
                        retValue = levelMap[currentX, currentY];
                        currentY++;
                    }
                    break;
                case Direction.West:
                    while (retValue.MapCharacter == ' ' && currentX > 0)
                    {
                        retValue = levelMap[currentX, currentY];
                        currentX--;
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

        private MapSpace SearchByDistance(Direction direction, int x, int y, int spaces)
        {
            MapSpace retValue = new MapSpace();

            // Search for specific character a given number of spaces, stopping at edge if necessary.
            // If the search goes off the grid, return a blank mapspace.
            y = (y > MAP_HT) ? MAP_HT : y;
            y = (y < 0) ? 0 : y;
            x = (x > MAP_WD) ? MAP_WD : x;
            x = (x < 0) ? 0 : x;

            for (int i = 1; i <= spaces; i++)
            {
                switch (direction)
                {
                    case Direction.North:
                        if ((y - i) >= 0)
                            retValue = levelMap[x, y - i];

                        break;
                    case Direction.East:
                        if ((x + i) >= MAP_WD)
                            retValue = levelMap[x + i, y];
    
                        break;
                    case Direction.South:
                        if ((y + i) <= MAP_HT)
                            retValue = levelMap[x, y + i];
    
                        break;
                    case Direction.West:
                        if ((x - i) >= 0)
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

        public string MapText()
        {
            // Output the array to text for display.
            string lineValue = "", retValue = "";

            for (int y = 0; y <= MAP_HT; y++)
            {
                for (int x = 0; x <= MAP_WD; x++)
                    lineValue += levelMap[x, y].DisplayCharacter;

                lineValue += "\n";

                // Add new line character.
                retValue += lineValue;
                lineValue = "";
            }
            Debug.Write(retValue);
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

            public MapSpace(char mapChar, MapSpace oldSpace)
            {
                this.mapChar = mapChar;
                this.displayChar = mapChar;
                this.searchToDisp= oldSpace.searchToDisp;
                this.xc= oldSpace.X; this.yc= oldSpace.Y;    
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
