using System.Threading.Tasks;
using Dojo.Starknet;
using TerritoryWars.ConnectorLayers.Dojo;
using TerritoryWars.DataModels;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.General
{
    public class RefactoringTester : MonoBehaviour
    {
        [Header("Getting Game model")] 
        public string PlayerId;
        public GameModel gameModel;


        [ContextMenu("Get Game")]
        public async Task GetGame()
        {
            ApplicationState.CurrentState = ApplicationStates.Initializing;
            gameModel = await DojoModels.GetGameInProgress(PlayerId);
        }
    }
}