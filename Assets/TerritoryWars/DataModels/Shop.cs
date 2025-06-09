using System;

namespace TerritoryWars.DataModels
{
    [Serializable]
    public struct Shop
    {
        public string ShopId;
        public uint[] SkinPrices;
        
        public Shop SetData(evolute_duel_Shop shop)
        {
            return this;
        }
    }
}