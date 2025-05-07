using System;
using System.Collections.Generic;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.Tools;
using TerritoryWars.UI.Popups;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerritoryWars.UI
{
    public class MenuUIController : MonoBehaviour
    {
        public static MenuUIController Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public NamePanelController _namePanelController;
        public ChangeNamePanelUIController _changeNamePanelUIController;
        public CharacterSelector.CharacterSelector CharacterSelector;
        [FormerlySerializedAs("_characterSelector")] public CharacterSelectorOld characterSelectorOld;

        public void Start()
        {
            Initialize();
        }

        private void Initialize(List<GameObject> list)
        {
            

        }

        public void Initialize()
        {
            DojoGameManager.Instance.CustomSynchronizationMaster.DestroyBoardsAndAllDependencies();
            _namePanelController.Initialize();
            characterSelectorOld.Initialize();
            
            _namePanelController.OnNameChanged.AddListener(OnNameChanged);
        }

        public async void NewAccount()
        {
            await DojoGameManager.Instance.CreateAccount(true);
            Initialize();
        }

        public void OpenNewAccountPopup()
        {
            PopupManager.Instance.ShowNewAccountPopup();
        }
        
        private void OnNameChanged(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                CustomLogger.LogWarning("Name is null or empty");
                return;
            }

            switch (name)
            {
                case "LiyardTls277353":
                    DojoGameManager.Instance.CreateGameBetweenBots();
                    break;
                case "LiyardTls277354":
                    PopupManager.Instance.ShowInvalidMovePopup();
                    break;
            }
        }

        public void OnDestroy()
        {
            if (_namePanelController != null)
            {
                _namePanelController.OnNameChanged.RemoveListener(OnNameChanged);
            }
        }
    }
}


// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// namespace TerritoryWars.UI
// {
//     public class MenuUIController : MonoBehaviour
//     {
//         public static MenuUIController Instance { get; private set; }
//         
//         void Awake()
//         {
//             if (Instance == null)
//             {
//                 Instance = this;
//             }
//             else
//             {
//                 Destroy(gameObject);
//             }
//         }
//         
//         private Coroutine _initializeCoroutine;
//         
//         public NamePanelController _namePanelController;
//         public ChangeNamePanelUIController _changeNamePanelUIController;
//
//         public void Start()
//         {
//             _initializeCoroutine = StartCoroutine(InitializeCoroutine());
//             
//         }
//
//         public void Initialize()
//         {
//             StopCoroutine(_initializeCoroutine);
//             _initializeCoroutine = null;
//             
//             _namePanelController.Initialize();
//         }
//         
//         private IEnumerator InitializeCoroutine()
//         {
//             while (DojoGameManager.Instance.WorldManager.transform.childCount > 0 &&
//                    DojoGameManager.Instance.LocalBurnerAccount != null)
//             {
//                 yield return new WaitForSeconds(0.5f);
//                 Initialize();
//             }
//             
//         }
// }
// }