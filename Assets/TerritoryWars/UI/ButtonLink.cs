using TerritoryWars.ExternalConnections;
using TerritoryWars.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonLink : MonoBehaviour
    {
        private Button _button;
        public string Link;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button == null)
            {
                CustomLogger.LogWarning("Button component not found on this GameObject.");
                return;
            }
            _button.onClick.AddListener(OpenURL);
        }

        private void OpenURL()
        {
            if (!string.IsNullOrEmpty(Link))
            {
                JSBridge.OpenURL(Link);
            }
        }
    }
}