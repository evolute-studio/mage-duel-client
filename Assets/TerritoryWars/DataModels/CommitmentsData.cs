using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Dojo.Starknet;

namespace TerritoryWars.DataModels
{
    [Serializable]
    public struct CommitmentsData
    {
        public bool IsNull => Permutations == null || Nonce == null || Hashes == null;
        
        public byte[] Permutations;
        public FieldElement[] Nonce;
        public List<uint[]> Hashes;

        private SHA256 sha256;

        public CommitmentsData(int lenght)
        {
            Permutations = new byte[lenght];
            Nonce = new FieldElement[lenght];
            Hashes = new List<uint[]>(lenght);
                
            sha256 = SHA256.Create();
        }
            
        public void GenerateHashes()
        {
            Hashes = Enumerable.Range(0, Permutations.Length)
                .Select(ComputeHash)
                .ToList();
        }

        public uint[] ComputeHash(int index)
        {
            byte tileIndex = (byte)index;
            FieldElement nonce = Nonce[tileIndex];
            byte c = Permutations[tileIndex];
                
            byte[] bytes = new byte[34];
            bytes[0] = tileIndex;
            for( int i = 1; i < 33; i++)
            {
                bytes[i] = nonce.Inner.data[i - 1];
            }
            bytes[33] = c;
                
            byte[] hash = sha256.ComputeHash(bytes);
            uint[] result = new uint[8];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 3; j >= 0; j--)
                {
                    result[i] += (uint)hash[i * 4 + j] << (j * 8);
                }
            }
                
            return result;
        }
        
        public uint[] GetAllHashes()
        {
            if (Hashes == null || Hashes.Count == 0)
            {
                return new uint[0];
            }
            return Hashes.SelectMany(hash => hash).ToArray();
        }
    }
}