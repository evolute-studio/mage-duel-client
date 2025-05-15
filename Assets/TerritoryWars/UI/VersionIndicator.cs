using TMPro;
using UnityEngine;

namespace TerritoryWars.UI
{
    public class VersionIndicator : MonoBehaviour
    {
        private TextMeshProUGUI textMeshProUGUI;

        private void Awake()
        {
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            textMeshProUGUI.text = "v" + Application.version;
        }
    }
}