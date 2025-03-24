using System.Collections.Generic;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.Tools;
using TerritoryWars.UI.Popups;
using UnityEngine;

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

        public void Start()
        {
            
            Initialize();
            //DojoGameManager.Instance.SessionManager = null;
        }

        private void Initialize(List<GameObject> list)
        {
            

        }

        public void Initialize()
        {
            DojoGameManager.Instance.CustomSynchronizationMaster.DestroyBoardsAndAllDependencies();
            CustomLogger.LogWarning("MenuUIController.Initialize");
            _namePanelController.Initialize();
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