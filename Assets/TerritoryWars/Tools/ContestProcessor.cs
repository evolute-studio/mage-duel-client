﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dojo;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.Tools
{
    public class ContestProcessor
    {
        private List<ContestInformation> _contestActions = new List<ContestInformation>();
        private bool _isProcessing = false;
        public bool IsGameFinished = false;

        public async Task CheckFinishGame()
        {
            if (_isProcessing) return;

            _isProcessing = true;

            try
            {
                await CoroutineAsync(() => { }, 2f);

                if (IsGameFinished)
                {
                    HandleGameFinished();
                    return;
                }

                if (!IsGameFinished)
                {
                    List<ContestInformation> actionsToExecute = new List<ContestInformation>(_contestActions);
                    _contestActions.Clear();

                    foreach (var contestAction in actionsToExecute)
                    {
                        contestAction.ContestAction?.Invoke();
                    }
                }
            }
            
            finally
            {
                _isProcessing = false;
            }
        }

        public void SetGameFinished(bool isGameFinished)
        {
            IsGameFinished = isGameFinished;
        }

        private void HandleGameFinished()
        {
            FinishGameContests finishGameContests = new FinishGameContests(_contestActions);
        }

        public async void AddModel(ContestInformation modelContestInformation)
        {
            if (modelContestInformation != null)
            {
                _contestActions.Add(modelContestInformation);
            }

            await CheckFinishGame();
        }

        private async Task CoroutineAsync(Action action, float delay = 0f)
        {
            var tcs = new TaskCompletionSource<bool>();
            Coroutines.StartRoutine(WaitForCoroutine(tcs, action, delay));
            await tcs.Task;
        }

        private IEnumerator WaitForCoroutine(TaskCompletionSource<bool> tcs, Action action, float delay = 0f)
        {
            yield return new WaitForSeconds(delay);
            action();
            tcs.TrySetResult(true);
        }
    }

}