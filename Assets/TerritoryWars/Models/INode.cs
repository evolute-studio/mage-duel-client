using Dojo.Starknet;

namespace TerritoryWars.Models
{
    public interface INode
    {
        public byte GetPosition();
        public ushort GetBluePoints();
        public ushort GetRedPoints();
        public byte GetOpenEdges();
    }
}