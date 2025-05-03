using System;
using TerritoryWars;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChangeNamePanelUIController : MonoBehaviour
{
    [SerializeField] private GameObject NamePanel;
    [SerializeField] private TMP_InputField NameInputField;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;
    
    private string _name;

    private void Start()
    {
        Initialization();
    }

    private void Initialization()
    {
        MenuUIController.Instance._namePanelController.OnNameChanged.AddListener(NameChanged);
        _confirmButton.onClick.AddListener(GetNameFromInputField);
        _cancelButton.onClick.AddListener(OnCancelButtonClick);
    }
    
    public void SetNamePanelActive(bool active)
    {
        if(active) SetNamePanelControlActive(true);
        NameInputField.text = MenuUIController.Instance._namePanelController.PlayerNameText.text;
        NamePanel.SetActive(active);
    }
    
    public void SetNamePanelControlActive(bool active)
    {
        NameInputField.interactable = active;
        _confirmButton.interactable = active;
        _cancelButton.interactable = active;
    }

    public void NameChanged(string name)
    {
        SetNamePanelControlActive(true);
        SetNamePanelActive(false);
    }

    private void GetNameFromInputField()
    {
        _name = NameInputField.text;
        if (IsNameValid())
        {
            //SetNamePanelActive(false);
            DojoConnector.ChangeUsername(
                DojoGameManager.Instance.LocalAccount,
                CairoFieldsConverter.GetFieldElementFromString(_name));
            SetNamePanelControlActive(false);
            evolute_duel_Player profile = DojoGameManager.Instance.GetLocalPlayerData();
            if (profile == null)
            {
                CustomLogger.LogWarning("profile is null");
                return;
            }
            
            //MenuUIController.Instance._namePanelController.SetName(CairoFieldsConverter.GetStringFromFieldElement(profile.username));
            MenuUIController.Instance._namePanelController.SetEvoluteBalance(profile.balance);
        }
        else
        {
            Debug.LogError("Name is not valid");
        }
    }
    
    private bool IsNameValid()
    {
        if(_name.Length < 3 || _name.Length > 31)
        {
            return false;
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(_name, @"^[a-zA-Z0-9]+$"))
        {
            return false;
        }
        return true;
    }

    private void OnCancelButtonClick()
    {
        SetNamePanelActive(false);
    }
}
