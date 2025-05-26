using System;
using UnityEngine;

namespace TerritoryWars.DataModels
{
    public class GameConfiguration
    {
        // on client board has size 10x10 (included borders)
        // on server board has size 8x8 (without borders)
        // so we need to offset client board size by 1 tile in each direction
        public static Vector2Int ClientBoardSizeOffset = new Vector2Int(1, 1);
        // to client it will be 3, to server it will be 1 (4 - 3)
        public static byte ClientRotationOffset = 3;
        public static Vector2Int ClientBoardSize = new Vector2Int(10, 10);
        
        public static string[] TileTypes =
        {
            "CCCC", "FFFF", "RRRR", "CCCF", "CCCR", "CCRR", "CFFF", "FFFR", "CRRR", "FRRR",
            "CCFF", "CFCF", "CRCR", "FFRR", "FRFR", "CCFR", "CCRF", "CFCR", "CFFR", "CFRF",
            "CRFF", "CRRF", "CRFR", "CFRR"
        };
        
        public static char[] EdgeTypes = { 'C', 'R', 'M', 'F' };
        
        public static Vector2Int GetClientPosition(byte col, byte row)
        {
            return new Vector2Int(col, row) + ClientBoardSizeOffset;
        }
        
        public static Vector2Int GetClientPosition(int index)
        {
            int x = index / 8;
            int y = index % 8;
            return new Vector2Int(x, y) + ClientBoardSizeOffset;
        }
        
        public static byte GetClientRotation(byte rotation)
        {
            return (byte)((rotation + 3) % 4);
        }
        
        public static string GetTileType(int index)
        {
            if (index < 0 || index >= TileTypes.Length)
            {
                return String.Empty;
            }
            return TileTypes[index];
        }
        
        public static char[] GetInitialEdgeState(byte[] initialEdgeState)
        {
            char[] charArray = Array.ConvertAll(initialEdgeState, c => EdgeTypes[(int)c]);
            return charArray;
        }
    }
}