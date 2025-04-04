using UnityEngine.Events;

namespace TerritoryWars.UI.Popups
{
    public class PopupConfig
    {
        public string Text;
        public string FirstOptionText;
        public string SecondOptionText;
        public UnityAction FirstOptionAction;
        public UnityAction SecondOptionAction;
    }
}