using TerritoryWars.ModelsDataConverters;

namespace TerritoryWars.DataModels
{
    public struct PlayerProfile
    {
        public bool IsNull => string.IsNullOrEmpty(PlayerId);
        
        public string PlayerId;
        public string Username;
        public uint Balance;
        public int GamesPlayed;
        public byte ActiveSkin;
        public PlayerRole Role;

        public PlayerProfile SetData(evolute_duel_Player player)
        {
            PlayerId = player.player_id.Hex();
            Username = CairoFieldsConverter.GetStringFromFieldElement(player.username);
            Balance = player.balance;
            GamesPlayed = CairoFieldsConverter.GetIntFromHex(player.games_played.Hex());
            ActiveSkin = player.active_skin;
            Role = (PlayerRole)player.role;
            return this;
        }
    }
    
    public enum PlayerRole
    {
        Guest = 0,
        Controller = 1,
        Bot = 2
    }
}