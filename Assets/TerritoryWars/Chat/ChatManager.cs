using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Dojo;
using Dojo.Starknet;
using Dojo.Torii;
using dojo_bindings;
using Newtonsoft.Json;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("ChatManager already exists. Deleting new instance.");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }
    
    public bool chatOpen = false;
    public FieldElement channel = new FieldElement(0);

    public DojoGameManager gameManager => DojoGameManager.Instance;
    public WorldManager worldManager => gameManager.WorldManager;

    [SerializeField] private GameObject messagePrefab;
    //[SerializeField] private GameObject chatPanel;
    [SerializeField] private Transform chatScrollView;
    [SerializeField] private Transform chatContenet;
    [SerializeField] private TMPro.TMP_InputField chatInput;
    
    private Coroutine _viewportCoroutine;
    
    // Start is called before the first frame update
    public void Initialize(FieldElement chanel = null)
    {
        worldManager.synchronizationMaster.OnEntitySpawned.RemoveListener(OnEntitySpawned);
        if (chanel == null)
        {
            chanel = new FieldElement(0);
        }
        
        channel = chanel;
        worldManager.synchronizationMaster.OnEntitySpawned.AddListener(OnEntitySpawned);
        // chatInput.gameObject.SetActive(false);
        // chatInput.text = "";
        // chatOpen = false;
    }
    
    private void OnEntitySpawned(GameObject obj)
    {
        if(obj == null) return;
        if (obj.TryGetComponent(out evolute_duel_Message message))
        {
            if (message.channel != channel) IncomingModelsFilter.DestroyModel(message);
            string username = CairoFieldsConverter.GetStringFromFieldElement(gameManager.GetPlayerData(message.identity.Hex()).username);
            string messageText = message.message;
            string result = $"<color=yellow>{username}</color>: {messageText}";
            // create a new message object
            var newMessage = Instantiate(messagePrefab, chatContenet);
            newMessage.GetComponentInChildren<TextMeshProUGUI>().text = result;

            // if (!chatOpen)
            // {
            //     SetActiveViewPort(true);
            //     _viewportCoroutine = StartCoroutine(ViewportCoroutine());
            // }
        }
    }
    
    private IEnumerator ViewportCoroutine()
    {
        yield return new WaitForSeconds(5f);
        chatScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
    }

    
    public void SetActiveToggle()
    {
        chatOpen = !chatOpen;
        SetActivePanel(chatOpen);
        // if(_viewportCoroutine != null)
        // {
        //     StopCoroutine(_viewportCoroutine);
        //     _viewportCoroutine = null;
        // }
    }

    public void SetActiveViewPort(bool value)
    {
        chatScrollView.gameObject.SetActive(value);
    }

    public void SetActiveInputField(bool value)
    {
        chatInput.gameObject.SetActive(value);
        if (value)
        {
            chatInput.text = "";
            chatInput.ActivateInputField();
        }
        else
        {
            chatInput.text = "";
            chatInput.gameObject.SetActive(false);
        }
    }
    
    public void SetActivePanel(bool value)
    {
        //SetActiveViewPort(value);
        SetActiveInputField(value);
    }

    // Update is called once per frame
    void Update()
    {
        // chat interactions below
        if (!chatOpen) return;
        // if we press enter, send message
        if (Input.GetKeyUp(KeyCode.Return))
        {
            SendMessage(chatInput.text);
            chatInput.gameObject.SetActive(false);
            chatInput.text = "";
            chatOpen = false;
        }

        // if press esc. close chat
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            SetActivePanel(false);
        }
    }

    async void SendMessage(string message)
    {
        var account = gameManager.LocalBurnerAccount;
        // random salt for the message
        var randomBytes = new byte[28];
        RandomNumberGenerator.Fill(randomBytes);

        // copy to a 32 byte array
        var salt = new byte[32];
        randomBytes.CopyTo(salt, 0);

        salt = salt.Reverse().ToArray();

        var username = gameManager.GetLocalPlayerData().username;

        var typed_data = TypedData.From(new evolute_duel_Message
        {
            identity = account.Address,
            message = message,
            channel = channel,
            salt = new FieldElement(salt),
        });

        Debug.Log(JsonConvert.SerializeObject(typed_data));

        FieldElement messageHash = typed_data.encode(account.Address);
        Signature signature = account.Signer.Sign(messageHash);

        await worldManager.Publish(typed_data, signature.ToFeltArray());
    }
}
