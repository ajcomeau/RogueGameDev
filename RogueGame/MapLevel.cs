using System;
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
        private const char CORNER_TL = '╔';
        private const char CORNER_BR = '╝';
        private const char CORNER_TR = '╗';
        private const char CORNER_BL = '╚';
        private const char ROOM_INT = '.';
        private const char ROOM_DOOR = '╬';
        private const int REGION_WT = 26;
        private const int REGION_HT = 8;
        private const int MAX_ROOM_WT = 24;  // Based on screen width of 80, 78 allowed
        private const int MAX_ROOM_HT = 6;   // Based on screen height of 24, 24 allowed
        private const int MIN_ROOM_WT = 4;
        private const int MIN_ROOM_HT = 4;
        private const int ROOM_SKIP_PCT = 20;

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
                for (int x = 1; x < 54; x += REGION_WT)
                {
                    if (rand.Next(101) > ROOM_SKIP_PCT)        //10% chance of not creating room
                    {
                        // Room size
                        roomHeight = rand.Next(MIN_ROOM_HT, MAX_ROOM_HT + 1);
                        roomWidth = rand.Next(MIN_ROOM_WT, MAX_ROOM_WT + 1);

                        // Center room in region
                        roomAnchorY = (int)((REGION_HT - roomHeight) / 2) + y;
                        roomAnchorX = (int)((REGION_WT - roomWidth) / 2) + x;

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

        private void RoomGeneration(int roomAnchorX, int roomAnchorY, int roomWidth, int roomHeight)
        {
            // Create room on map based on inputs

            // Draw the boundaries of the room
            Debug.WriteLine($"Creating room at {roomAnchorX}, {roomAnchorY} to {roomAnchorX + roomWidth}, {roomAnchorY + roomHeight}");

            for (int y = roomAnchorY; y < (roomAnchorY + roomHeight + 1); y++)
            {
                for (int x = roomAnchorX; x < (roomAnchorX + roomWidth + 1); x++)
                {
                    if (y == roomAnchorY || y == roomAnchorY + roomHeight)
                    {
                        dLevel[x, y] = new MapSpace(HORIZONTAL, false, false);
                    }
                    
                    if (x == roomAnchorX || x == roomAnchorX + roomWidth)
                    {
                        dLevel[x, y] = new MapSpace(VERTICAL, false, false);
                    }
                }
            }

            // Set the corners
            dLevel[roomAnchorX, roomAnchorY] = new MapSpace(CORNER_TL, false, false);
            dLevel[roomAnchorX + roomWidth, roomAnchorY] = new MapSpace(CORNER_TR, false, false);
            dLevel[roomAnchorX, roomAnchorY + roomHeight] = new MapSpace(CORNER_BL, false, false);
            dLevel[roomAnchorX + roomWidth, roomAnchorY + roomHeight] = new MapSpace(CORNER_BR, false, false);

            // Fill in any blanks.
            for (int y = roomAnchorY; y < (roomAnchorY + roomHeight + 1); y++)
            {
                for (int x = roomAnchorX; x < (roomAnchorX + roomWidth + 1); x++)
                {
                    if (dLevel[x, y] == null)
                        dLevel[x, y] = new MapSpace('.', false, false);
                }
            }

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
