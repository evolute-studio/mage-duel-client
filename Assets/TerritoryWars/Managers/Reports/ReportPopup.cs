using System.Collections.Generic;
using TerritoryWars.Tools;
using TerritoryWars.UI.Popups;
using TMPro;
using UnityEngine;

namespace TerritoryWars.Managers.Reports
{
    public class ReportPopup : MessagePopupBase
    {
        public ReportType ReportType = ReportType.Feedback;
        public List<CustomButton> ReportButtons;
        public TMP_InputField MessageInput;

        public void SetFeedbackMode()
        {
            ReportType = ReportType.Feedback;
            int activeButtonIndex = 0;
            for (int i = 0; i < ReportButtons.Count; i++)
            {
                ReportButtons[i].SetPressed(i == activeButtonIndex);
            }
        }

        public void SetBugMode()
        {
            ReportType = ReportType.Bug;
            int activeButtonIndex = 1;
            for (int i = 0; i < ReportButtons.Count; i++)
            {
                ReportButtons[i].SetPressed(i == activeButtonIndex);
            }
        }

        public void SetCriticalIssueMode()
        {
            ReportType = ReportType.CriticalIssue;
            int activeButtonIndex = 2;
            for (int i = 0; i < ReportButtons.Count; i++)
            {
                ReportButtons[i].SetPressed(i == activeButtonIndex);
            }
        }

        public void SendReport()
        {
            ReportSystem.Instance.SendReport(ReportType, MessageInput.text);
        }
    }
}