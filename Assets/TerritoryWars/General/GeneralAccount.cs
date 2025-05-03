using Dojo.Starknet;

namespace TerritoryWars.General
{
    public class GeneralAccount
    {
        public bool IsController => _controllerAddress != null;
        private FieldElement _controllerAddress;

        public FieldElement Address
        {
            get
            {
                if( _controllerAddress != null ) 
                {
                    return _controllerAddress;
                }
                if (Account != null)
                {
                    return Account.Address;
                }

                return null;
            }
        }

        public Account Account { get; private set; }
        
        public GeneralAccount(FieldElement controllerAddress)
        {
            _controllerAddress = controllerAddress;
        }
        
        public GeneralAccount(Account account)
        {
            Account = account;
        }
    }
}