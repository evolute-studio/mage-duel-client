using TerritoryWars.UI.Popups;
using UnityEngine;

namespace TerritoryWars.Managers.Reports
{
    public class ReportButton : MonoBehaviour
    {
        public void OpenReportPopup()
        {
            PopupManager.Instance.ShowReportPopup();
        }
    }
}