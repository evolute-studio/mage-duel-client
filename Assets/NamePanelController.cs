using System;
using System.Collections.Generic;
using TerritoryWars;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tools;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class NamePanelController : MonoBehaviour
{
    public ChangeNamePanelUIController ChangeNamePanelUIController;
    
    public uint EvoluteBalance;
    
    public GameObject NamePanel;
    public GameObject ChangeNamePanel;
    public TextMeshProUGUI PlayerNameText;
    public TextMeshProUGUI EvoluteCountText;
    public Button ChangeNameButton;
    public Image ControllerIcon;
    
    private bool _isInitialized = false;
    
    public UnityEvent<string> OnNameChanged;

    // private void Awake()
    // {
    //     DojoGameManager.Instance.WorldManager.synchronizationMaster.OnSynchronized.AddListener(Initialize);
    // }

    // private void Initialize(List<GameObject> list)
    // {
    //     CustomLogger.LogWarning("Initialize NamePanelController after Sync");
    //     Invoke(nameof(Initialize), 0.1f);
    // }
    
    public void Initialize()
    {
        evolute_duel_Player profile = DojoGameManager.Instance.GetLocalPlayerData();
        if(profile == null)
        {
            CreateModelForNewPlayer();
            return;
        }
        string name = CairoFieldsConverter.GetStringFromFieldElement(profile.username);
        CheckNickname(name);
        SetName(name);
        SetEvoluteBalance(profile.balance);
        ControllerIcon.gameObject.SetActive(ApplicationState.IsController);
    }

    private void CreateModelForNewPlayer()
    {
        CustomLogger.LogWarning("Player profile is null");
        string username;
        if (ApplicationState.IsController)
        {
            username = WrapperConnector.instance.username;
        }
        else
        {
            username = "Guest" + DojoGameManager.Instance.LocalAccount.Address.Hex().Substring(0, 9);
        }
        DojoConnector.ChangeUsername(
            DojoGameManager.Instance.LocalAccount,
            CairoFieldsConverter.GetFieldElementFromString(username));
        SetName(username);
        SetEvoluteBalance(0);
    }

    public bool IsDefaultName()
    {
        // default name starts with "0x"
        return PlayerNameText.text.StartsWith("Guest");
    }

    public void CheckNickname(string name)
    {
        if (String.IsNullOrEmpty(name))
        {
            CustomLogger.LogError("Name is null or empty");
            WrapperConnectorCalls.GetControllerUsername();
        }
    }

    public void SetName(string name)
    {
        PlayerNameText.text = name;
        OnNameChanged?.Invoke(name);
    }
    
    public void SetEvoluteBalance(uint value)
    {
        EvoluteBalance = value;
        EvoluteCountText.text = value.ToString();
        LayoutRebuilder.ForceRebuildLayoutImmediate(EvoluteCountText.rectTransform);
    }
}
