using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Dojo.Starknet;
using TerritoryWars.Tools;

namespace TerritoryWars.DataModels
{
    [Serializable]
    public struct CommitmentsData
    {
        public bool IsNull => Permutations == null || Nonce == null || Hashes == null;
        
        public byte[] Permutations;
        public FieldElement[] Nonce;
        public List<uint[]> Hashes;
        public List<int> ProcessedIndexes;

        private SHA256 sha256;
         

        public CommitmentsData(int lenght)
        {
            Permutations = new byte[lenght];
            Nonce = new FieldElement[lenght];
            Hashes = new List<uint[]>(lenght);
                
            sha256 = SHA256.Create();
            ProcessedIndexes = new List<int>();
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
            BigInteger nonceBigInt = new BigInteger(nonce.Inner.data, isUnsigned: true, isBigEndian: true);
            byte c = Permutations[tileIndex];
                
            string data = tileIndex.ToString() + nonceBigInt + c;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
                
            byte[] hash = sha256.ComputeHash(bytes);
            uint[] result = new uint[8];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 3; j >= 0; j--)
                {
                    result[i] += (uint)hash[i * 4 + j] << ((3 - j) * 8);
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
            uint[] allHashes = Hashes.SelectMany(hash => hash).ToArray();
            CustomLogger.LogObject(allHashes, "All Hashes");
            return allHashes;
        }

        public byte GetIndex(byte c)
        {
            for (byte i = 0; i < Permutations.Length; i++)
            {
                if (Permutations[i] == c)
                {
                    if (!ProcessedIndexes.Contains(i))
                    {
                        ProcessedIndexes.Add(i);
                        return i; // Return the first unprocessed index with the given permutation
                    }
                }
            }
            return 255; // Not found
        }
    }
}