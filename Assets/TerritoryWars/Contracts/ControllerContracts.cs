using Dojo.Starknet;

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
            return new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "create_game",
                calldata = new string[] { }
            }.ToString();
        }

        public static string create_game_from_snapshot(FieldElement snapshotId)
        {
            return new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "create_game_from_snapshot",
                calldata = new string[] { snapshotId.Hex() }
            }.ToString();
        }

        public static string create_snapshot(FieldElement boardId, byte moveNumber)
        {
            return new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "create_snapshot",
                calldata = new string[] { boardId.Hex(), moveNumber.ToString() }
            }.ToString();
        }

        public static string finish_game(FieldElement boardId)
        {
            return new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "finish_game",
                calldata = new string[] { boardId.Hex() }
            }.ToString();
        }

        public static string join_game(FieldElement hostPlayer)
        {
            return new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "join_game",
                calldata = new string[] { hostPlayer.Hex() }
            }.ToString();
        }

        public static string make_move(Option<byte> jokerTile, byte rotation, byte col, byte row)
        {
            return new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "make_move",
                calldata = new string[] { jokerTile.Unwrap().ToString(), rotation.ToString(), col.ToString(), row.ToString() }
            }.ToString();
        }

        public static string skip_move()
        {
            return new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "skip_move",
                calldata = new string[] { }
            }.ToString();
        }

        public static string cancel_game()
        {
            return new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "cancel_game",
                calldata = new string[] { }
            }.ToString();
        }

        public static string active_skin()
        {
            return new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "active_skin",
                calldata = new string[] { }
            }.ToString();
        }

        public static string balance()
        {
            return new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "balance",
                calldata = new string[] { }
            }.ToString();
        }

        public static string become_bot()
        {
            return new Transaction
            {
                contractAddress = EVOLUTE_DUEL_GAME_ADDRESS,
                entrypoint = "become_bot",
                calldata = new string[] { }
            }.ToString();
        }

        public static string change_skin(int skinId)
        {
            return new Transaction
            {
                contractAddress = EVOLUTE_DUEL_PLAYER_PROFILE_ACTIONS_ADDRESS,
                entrypoint = "change_skin",
                calldata = new string[] { skinId.ToString() }
            }.ToString();
        }

        public static string change_username(FieldElement newUsername)
        {
            return new Transaction
            {
                contractAddress = EVOLUTE_DUEL_PLAYER_PROFILE_ACTIONS_ADDRESS,
                entrypoint = "change_username",
                calldata = new string[] { newUsername.GetString() }
            }.ToString();
        }

        public static string username()
        {
            return new Transaction
            {
                contractAddress = EVOLUTE_DUEL_PLAYER_PROFILE_ACTIONS_ADDRESS,
                entrypoint = "username",
                calldata = new string[] { }
            }.ToString();


        }
    }
}