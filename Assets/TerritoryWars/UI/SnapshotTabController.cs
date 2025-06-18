using System;
using System.Collections.Generic;
using Dojo;
using Dojo.Starknet;
using TerritoryWars;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    public class SnapshotTabController : MonoBehaviour
    {
        public GameObject PanelGameObject;
        public GameObject SnapshotListItemPrefab;
        public Transform ListItemParent;
        public GameObject BackgroundPlaceholderGO;

        private List<SnapshotListItem> _listItems = new List<SnapshotListItem>();
        private List<ModelInstance> _models = new List<ModelInstance>();
        private List<GameObject> _snapshotObjects = new List<GameObject>();

        public void Start() => Initialize();

        public void Initialize()
        {

        }

        public SnapshotListItem CreateListItem()
        {
            GameObject listItem = Instantiate(SnapshotListItemPrefab, ListItemParent);
            SnapshotListItem matchListItem = new SnapshotListItem(listItem);
            _listItems.Add(matchListItem);
            return matchListItem;
        }

        private void ClearAllListItems()
        {
            foreach (var matchListItem in _listItems)
            {
                Destroy(matchListItem.ListItem);
            }

            _listItems.Clear();
        }

        public void SetBackgroundPlaceholder(bool isActive)
        {
            BackgroundPlaceholderGO.SetActive(isActive);
        }

        private void CreatedNewEntity(GameObject newEntity)
        {
            if (!newEntity.TryGetComponent(out evolute_duel_Snapshot snapshotModel)) return;
            FetchData();
        }

        private void ModelUpdated(ModelInstance modelInstance)
        {
            //if (!modelInstance.transform.TryGetComponent(out evolute_duel_Snapshot snapshotModel)) return;
            if (modelInstance is evolute_duel_Snapshot && !_models.Contains(modelInstance))
            {
                FetchData();
                _models.Add(modelInstance);
            }

        }

        private async void FetchData()
        {
            ClearAllListItems();
            await DojoGameManager.Instance.SyncLocalPlayerSnapshots();
            GameObject[] snapshots = DojoGameManager.Instance.GetSnapshots();
            //BackgroundPlaceholderGO.SetActive(games.Length == 0);


            foreach (var snapshot in snapshots)
            {
                if (_snapshotObjects.Contains(snapshot)) continue;
                if (!snapshot.TryGetComponent(out evolute_duel_Snapshot snapshotModel)) return;
                if (!snapshotModel.player.Hex().Equals(DojoGameManager.Instance.LocalAccount.Address.Hex())) continue;

                evolute_duel_Player player =
                    await DojoGameManager.Instance.CustomSynchronizationMaster
                        .WaitForModelByPredicate<evolute_duel_Player>(p =>
                            p.player_id.Hex() == snapshotModel.player.Hex()
                        );
                if (player == null)
                {
                    CustomLogger.LogWarning(
                        $"Snapshot: {snapshotModel.snapshot_id.Hex()} has no player model: {snapshotModel.player.Hex()}");
                    continue;
                }

                string playerName = CairoFieldsConverter.GetStringFromFieldElement(player.username);
                uint evoluteBalance = player.balance;
                int moveNumber = snapshotModel.move_number;
                SnapshotListItem snapshotListItem = CreateListItem();
                snapshotListItem.UpdateItem(playerName, evoluteBalance, moveNumber, () =>
                {
                    SetActivePanel(false);
                    DojoConnector.CreateGameFromSnapshot(DojoGameManager.Instance.LocalAccount,
                        snapshotModel.snapshot_id);
                });
                _snapshotObjects.Add(snapshot);

            }

            SetBackgroundPlaceholder(snapshots.Length == 0);
        }

        public async void SetActivePanel(bool isActive)
        {
            if (isActive && MenuUIController.Instance.NamePanelController.IsDefaultName())
            {
                MenuUIController.Instance.ChangeNamePanelUIController.SetNamePanelActive(true);
                return;
            }


            PanelGameObject.SetActive(isActive);
            if (isActive)
            {
                _models.Clear();
                _snapshotObjects.Clear();
                ApplicationState.SetState(ApplicationStates.SnapshotTab);
                FetchData();
                IncomingModelsFilter.OnModelPassed.AddListener(ModelUpdated);
            }
            else
            {
                ApplicationState.SetState(ApplicationStates.Menu);
                DojoGameManager.Instance.CustomSynchronizationMaster.DestroyAllSnapshots();
                IncomingModelsFilter.OnModelPassed.RemoveListener(ModelUpdated);
                ClearAllListItems();
                CursorManager.Instance.SetCursor("default");
            }
        }



    }

    public class SnapshotListItem
    {
        public GameObject ListItem;
        public string CreatorPlayerName;
        public uint CreatorPlayerEvoluteCount;

        private TextMeshProUGUI _creatorPlayerNameText;
        private TextMeshProUGUI _creatorPlayerEvoluteCountText;
        private TextMeshProUGUI _moveNumberText;
        private Button _seeMapButton;
        private Button _restoreButton;

        public SnapshotListItem(GameObject listItem)
        {
            ListItem = listItem;
            _creatorPlayerNameText =
                listItem.transform.Find("PlayerName/PlayerNameText").GetComponent<TextMeshProUGUI>();
            _creatorPlayerEvoluteCountText =
                listItem.transform.Find("EvoluteCount/Count").GetComponent<TextMeshProUGUI>();
            _moveNumberText = listItem.transform.Find("MoveNumber/MoveNumberText").GetComponent<TextMeshProUGUI>();
            _seeMapButton = listItem.transform.Find("Buttons/MapButton").GetComponent<Button>();
            _restoreButton = listItem.transform.Find("Buttons/RestoreButton").GetComponent<Button>();
        }

        public void UpdateItem(string creatorPlayerName, uint creatorPlayerEvoluteCount, int moveNumber,
            UnityAction onRestore = null)
        {
            CreatorPlayerName = creatorPlayerName;
            CreatorPlayerEvoluteCount = creatorPlayerEvoluteCount;

            _creatorPlayerNameText.text = creatorPlayerName;
            _creatorPlayerEvoluteCountText.text = " x " + creatorPlayerEvoluteCount.ToString();
            _moveNumberText.text = "Moves: \n" + moveNumber;

            _restoreButton.onClick.RemoveAllListeners();
            if (onRestore != null)
            {

                _restoreButton.onClick.AddListener(onRestore);
            }

        }

        public void SetActive(bool isActive)
        {
            ListItem.SetActive(isActive);
        }
    }
}
