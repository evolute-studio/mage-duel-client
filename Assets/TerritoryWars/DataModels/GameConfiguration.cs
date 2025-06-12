using System;
using TerritoryWars.DataModels.Events;
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
        public static Vector2Int ServerBoardSize = new Vector2Int(8, 8);
        
        public static ushort GameCreationDuration = 65; // seconds
        public static ushort RevealDuration = 65; // seconds
        public static ushort RequestDuration = 65; // seconds
        
        public static ushort TurnDuration = 60; // seconds
        public static ushort PassingTurnDuration = 5; // TurnDuration includ

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
            int height = ServerBoardSize.x;
            int x = index / height;
            int y = index % height;
            return new Vector2Int(x, y) + ClientBoardSizeOffset;
        }

        public static Vector2Int GetPositionByRoot(byte root)
        {
            int height = ServerBoardSize.x;
            int tile = root / 4;
            int x = tile / height;
            int y = tile % height;
            return new Vector2Int(x, y) + ClientBoardSizeOffset;
        }

        public static (Vector2Int, Side) GetPositionAndSide(byte root)
        {
            Vector2Int position = GetPositionByRoot(root);
            Side side = (Side)((root + ClientRotationOffset) % 4);
            return (position, side);
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

        public static int GetTileTypeIndex(string type)
        {
            for (int i = 0; i < TileTypes.Length; i++)
            {
                if (TileTypes[i] == type)
                {
                    return i;
                }
            }
            return -1;
        }

        public static char[] GetInitialEdgeState(byte[] initialEdgeState)
        {
            char[] charArray = Array.ConvertAll(initialEdgeState, c => EdgeTypes[(int)c]);
            return charArray;
        }

        public static int GetPhaseDuration(SessionPhase phase)
        {
            switch (phase)
            {
                case SessionPhase.Creating:
                    return GameCreationDuration;
                case SessionPhase.Reveal:
                    return RevealDuration;
                case SessionPhase.Request:
                    return RequestDuration;
                case SessionPhase.Move:
                    return TurnDuration;
                case SessionPhase.Finished:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }
        }
    }
}
