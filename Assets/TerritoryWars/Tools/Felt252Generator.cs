using System.Numerics;
using System.Security.Cryptography;

namespace TerritoryWars.Tools
{
    public static class Felt252Generator
    {
        private static readonly BigInteger MaxFelt = BigInteger.One << 252;

        public static BigInteger GenerateFelt252()
        {
            byte[] bytes = new byte[32]; // 256 біт = 32 байти
            RandomNumberGenerator.Fill(bytes);

            // Обнулити старші біти, щоб не перевищити 252 біти
            bytes[31] &= 0x0F; // Залишаємо тільки 4 молодших біти останнього байта

            return new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
        }
    }
}