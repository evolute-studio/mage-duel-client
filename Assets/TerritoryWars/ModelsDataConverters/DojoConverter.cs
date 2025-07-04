using TerritoryWars.Tile;
using UnityEngine;

namespace TerritoryWars.ModelsDataConverters
{
    public static class DojoConverter
    {
        public static (Option<byte> joker_tile, byte rotation, byte col, byte row) MoveClientToServer(TileData data, int x, int y, bool isJoker)
        {
            var tileConfig = OnChainBoardDataConverter.GetTypeAndRotation(data.RotatedConfig);
            Option<byte> jokerTile = isJoker ? new Option<byte>.Some(tileConfig.Item1) : new Option<byte>.None();
            byte rotation = (byte)((tileConfig.Item2 + 1) % 4);
            byte col = (byte) (x - 1);
            byte row = (byte) (y - 1);
            return (jokerTile, rotation, col, row);
        }
    }
}