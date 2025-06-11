using System.Collections.Generic;
using System.Linq;
using Dojo.Starknet;
using UnityEngine;

namespace TerritoryWars.Contracts
{
    public static class ControllerContracts
    {
        public static string EVOLUTE_DUEL_GAME_ADDRESS = "0x0";
        public static string EVOLUTE_DUEL_PLAYER_PROFILE_ACTIONS_ADDRESS = "0x0";

        public struct Transaction
        {
            public string contractAddress;
            public string entrypoint;
            public string[] calldata;
        }

        public static string GetTransaction(string contractAddress, string entryPoint, string calldata)
        {
            return $"{{" +
                   $"\"contractAddress\":\"{contractAddress}\"," +
                   $"\"entrypoint\":\"{entryPoint}\",\"calldata\":[{calldata}]}}";
        }

        public static string create_game()
        {
            Transaction tx = new Transaction()
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "create_game",
                calldata = new string[] { }
            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            return json;
        }

        public static string create_game_from_snapshot(FieldElement snapshotId)
        {
            Transaction tx = new Transaction()
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "create_game_from_snapshot",
                calldata = new string[] { snapshotId.Hex() }
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            return json;
        }

        public static string create_snapshot(FieldElement boardId, byte moveNumber)
        {
            Transaction tx = new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "create_snapshot",
                calldata = new string[] { boardId.Hex(), moveNumber.ToString() }
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            return json;
        }

        public static string finish_game(FieldElement boardId)
        {
            Transaction tx = new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "finish_game",
                calldata = new string[] { boardId.Hex() }
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            return json;
        }

        public static string join_game(FieldElement hostPlayer)
        {
            Transaction tx = new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "join_game",
                calldata = new string[] { hostPlayer.Hex() }
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            return json;
        }

        public static string commit_tiles(uint[] commitments)
        {
            List<string> calldata = new List<string>();
            calldata.Add(commitments.Length.ToString());
            //calldata.AddRange(commitments.SelectMany(commitmentsItem => new[] { new FieldElement(commitmentsItem).Inner }));
            Transaction tx = new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "commit_tiles",
                calldata = commitments.Select(c => c.ToString()).ToArray()
            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            return json;
        }
        
        public static string reveal_tile(byte tileIndex, FieldElement nonce, byte c)
        {
            Transaction tx = new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "reveal_tile",
                calldata = new string[] { tileIndex.ToString(), nonce.Hex(), c.ToString() }
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            return json;
        }
        
        public static string request_next_tile(byte tileIndex, FieldElement nonce, byte c)
        {
            Transaction tx = new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "request_next_tile",
                calldata = new string[] { tileIndex.ToString(), nonce.Hex(), c.ToString() }
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            return json;
        }

        public static string make_move(Option<byte> jokerTile, byte rotation, byte col, byte row)
        {
            List<string> calldata = new List<string>();
            if(jokerTile is Option<byte>.Some)
                calldata.Add("0x0");
            else
            {
                calldata.Add("0x1");
            }
            if (jokerTile is Option<byte>.Some) calldata.Add(jokerTile.UnwrapByte().ToString());
            calldata.Add(rotation.ToString());
            calldata.Add(col.ToString());
            calldata.Add(row.ToString());
            
            Transaction tx = new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "make_move",
                calldata = calldata.ToArray(),
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            return json;
        }

        public static string skip_move()
        {
            Transaction tx = new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "skip_move",
                calldata = new string[] { }
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            return json;
        }

        public static string cancel_game()
        {
            Transaction tx = new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "cancel_game",
                calldata = new string[] { }
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            return json;
        }

        public static string active_skin()
        {
            Transaction tx = new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "active_skin",
                calldata = new string[] { }
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            return json;
        }

        public static string balance()
        {
            Transaction tx = new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "balance",
                calldata = new string[] { }
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            return json;
        }

        public static string become_bot()
        {
            Transaction tx = new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "become_bot",
                calldata = new string[] { }
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            return json;
        }

        public static string change_skin(int skinId)
        {
            Transaction tx = new Transaction
            {
                contractAddress = EVOLUTE_DUEL_PLAYER_PROFILE_ACTIONS_ADDRESS,
                entrypoint = "change_skin",
                calldata = new string[] { skinId.ToString() }
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            return json;
        }

        public static string change_username(FieldElement newUsername)
        {
            Transaction tx = new Transaction
            {
                contractAddress = EVOLUTE_DUEL_PLAYER_PROFILE_ACTIONS_ADDRESS,
                entrypoint = "change_username",
                calldata = new string[] { newUsername.GetString() }
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            Debug.Log($"Change username transaction: {json}");
            return json;
        }

        public static string username()
        {
            Transaction tx = new Transaction
            {
                contractAddress = EVOLUTE_DUEL_PLAYER_PROFILE_ACTIONS_ADDRESS,
                entrypoint = "username",
                calldata = new string[] { }
            };
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tx);
            return json;


        }
    }
}