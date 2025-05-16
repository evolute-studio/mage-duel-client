using System;
using TerritoryWars;
using TerritoryWars.Contracts;
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
        MenuUIController.Instance.NamePanelController.OnNameChanged.AddListener(NameChanged);
        _confirmButton.onClick.AddListener(GetNameFromInputField);
        _cancelButton.onClick.AddListener(OnCancelButtonClick);
    }
    
    public void SetNamePanelActive(bool active)
    {
        if(active) SetNamePanelControlActive(true);
        NameInputField.text = "";
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
            //ControllerContracts.change_username(CairoFieldsConverter.GetFieldElementFromString(_name));
            SetNamePanelControlActive(false);
            evolute_duel_Player profile = DojoGameManager.Instance.GetLocalPlayerData();
            if (profile == null)
            {
                CustomLogger.LogWarning("profile is null");
                return;
            }
            
            //MenuUIController.Instance._namePanelController.SetName(CairoFieldsConverter.GetStringFromFieldElement(profile.username));
            MenuUIController.Instance.NamePanelController.SetEvoluteBalance(profile.balance);
        }
        else
        {
            Debug.LogError("Name is not valid");
        }
    }
    
    private bool IsNameValid()
    {
        TextMeshProUGUI placeholder = NameInputField.placeholder.GetComponent<TextMeshProUGUI>();
        if(_name.Length < 3 || _name.Length > 20)
        {
            placeholder.text = "3-20 characters";
            NameInputField.text = string.Empty;
            return false;
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(_name, @"^[a-zA-Z0-9]+$"))
        {
            placeholder.text = "Only latin letters and numbers";
            NameInputField.text = string.Empty;
            return false;
        }
        if (_name.StartsWith("Guest")){
            NameInputField.text = string.Empty;
            return false;
        }
        return true;
    }

    private void OnCancelButtonClick()
    {
        SetNamePanelActive(false);
    }
}
