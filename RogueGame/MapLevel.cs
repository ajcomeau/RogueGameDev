﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RogueGame{

    internal class MapLevel
    {
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
        private const int REGION_WD = 26;
        private const int REGION_HT = 8;
        private const int MAX_ROOM_WT = 24;  // Based on screen width of 80, 78 allowed
        private const int MAX_ROOM_HT = 6;   // Based on screen height of 25, 24 allowed
        private const int MIN_ROOM_WT = 4;
        private const int MIN_ROOM_HT = 4;
        private const int ROOM_SKIP_PCT = 10;
        private const int ROOM_EXIT_PCT = 85;

        private MapSpace[,] dLevel = new MapSpace[80, 25];   // Array to hold map definition.

        public MapSpace[,] LevelMap
        {
            // Read-only - Only class should be able to edit map.
            get { return dLevel; }
        
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
            dLevel = new MapSpace[80, 25];

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
                    if (dLevel[x,y] is null)
                        dLevel[x, y] = new MapSpace(' ', false, false);
                }
            }

        }

        private void RoomGeneration(int westWallX, int northWallY, int roomWidth, int roomHeight)
        {
            // Create room on map based on inputs

            int eastWallX = westWallX + roomWidth;
            int southWallY = northWallY + roomHeight;
            int regionNumber = GetRegionNumber(westWallX, northWallY);
            int doorway = 0; int[,] doors = new int[2,4];
            var rand = new Random(DateTime.Now.Millisecond);


            for (int y = northWallY; y < (southWallY + 1); y++)
            {
                for (int x = westWallX; x < (eastWallX + 1); x++)
                {
                    if (y == northWallY || y == southWallY)
                    {
                        dLevel[x, y] = new MapSpace(HORIZONTAL, false, false);
                    }
                    
                    if (x == westWallX || x == eastWallX)
                    {
                        dLevel[x, y] = new MapSpace(VERTICAL, false, false);
                    }
                }
            }

            // Fill in any blanks.
            for (int y = northWallY; y < (southWallY + 1); y++)
            {
                for (int x = westWallX; x < (eastWallX + 1); x++)
                {
                    if (dLevel[x, y] == null)
                        dLevel[x, y] = new MapSpace(ROOM_INT, false, false);
                }
            }

            // Add doorways
            if (regionNumber >= 4)  // North doorways
            {
                if (rand.Next(100) <= ROOM_EXIT_PCT)
                {
                    doorway = rand.Next(westWallX + 1, eastWallX - 1);
                    dLevel[doorway, northWallY] = new MapSpace(ROOM_DOOR, false, false);
                    doors[0, 0] = doorway;
                    doors[1, 0] = northWallY;
                }
            }

            if (regionNumber <= 6)  // South doorways
            {
                if (rand.Next(100) <= ROOM_EXIT_PCT)
                {
                    doorway = rand.Next(westWallX + 1, eastWallX - 1);
                    dLevel[doorway, southWallY] = new MapSpace(ROOM_DOOR, false, false);
                    doors[0, 1] = doorway;
                    doors[1, 1] = southWallY;
                }
            }

            if ("147258".Contains(regionNumber.ToString()))  // East doorways
            {
                if (rand.Next(100) <= ROOM_EXIT_PCT)
                {
                    doorway = rand.Next(northWallY + 1, southWallY - 1);
                    dLevel[eastWallX, doorway] = new MapSpace(ROOM_DOOR, false, false);
                    doors[0, 2] = eastWallX;
                    doors[1, 2] = doorway;
                }
            }

            if ("258369".Contains(regionNumber.ToString()))  // West doorways
            {
                if (rand.Next(100) <= ROOM_EXIT_PCT)
                {
                    doorway = rand.Next(northWallY + 1, southWallY - 1);
                    dLevel[westWallX, doorway] = new MapSpace(ROOM_DOOR, false, false);
                    doors[0, 3] = westWallX;
                    doors[1, 3] = doorway;
                }
            }


            // Create the hallways

            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0:     // North
                        if (doors[0,0] > 0)
                            dLevel[doors[0, 0], doors[1, 0] - 1] = new MapSpace(HALLWAY, false, false);
                        break;
                    case 1:     // South
                        if (doors[0, 1] > 0)
                            dLevel[doors[0, 1], doors[1, 1] + 1] = new MapSpace(HALLWAY, false, false);
                        break;
                    case 2:     // East
                        if (doors[0, 2] > 0)
                            dLevel[doors[0, 2] + 1, doors[1, 2]] = new MapSpace(HALLWAY, false, false);
                        break;
                    case 3:     // West
                        if (doors[0, 3] > 0)
                            dLevel[doors[0, 3] - 1, doors[1, 3]] = new MapSpace(HALLWAY, false, false);
                        break;
                    default:

                        break;
                }
            }
    

            // Set the corners.
     
            dLevel[westWallX, northWallY] = new MapSpace(CORNER_NW, false, false);
            dLevel[eastWallX, northWallY] = new MapSpace(CORNER_NE, false, false);
            dLevel[westWallX, southWallY] = new MapSpace(CORNER_SW, false, false);
            dLevel[eastWallX, southWallY] = new MapSpace(CORNER_SE, false, false);



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
                retValue += dLevel[x, rowNumber].DisplayCharacter;
            }
            // Add new line character.
            retValue += "\n";

            return retValue;
        }

        
        
        internal class MapSpace{
            private char mapChar;           // Actual character on map.
            private char displayChar;       // Displayed character - space for hidden object.
            private bool searchToDisp;      // Does the player need to search to reveal?            
            
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
                set { searchToDisp = value; }
            }

            public MapSpace()
            {
                // Create blank space for map
                mapChar = ' ';
                displayChar = ' ';
                searchToDisp = false;
            }

            public MapSpace(char mapChar, Boolean hidden, Boolean search)
            {
                this.mapChar = mapChar;
                this.displayChar = hidden ? ' ' : mapChar;
                this.searchToDisp = search;
            }
         }
    }
}