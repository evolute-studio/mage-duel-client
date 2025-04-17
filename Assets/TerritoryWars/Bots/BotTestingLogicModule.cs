using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerritoryWars.Bots
{
    public class BotTestingLogicModule : BotLogicModule
    {
        public Dictionary<Action, float> ActionsPossibilities = new Dictionary<Action, float>();

        public BotTestingLogicModule(Bot bot) : base(bot)
        {
            ActionsPossibilities.Add(ReloadAction, 0.1f);
            ActionsPossibilities.Add(SkipAction, 0.25f);
            ActionsPossibilities.Add(MakeMove, 0.65f);
        }

        public override void ExecuteLogic()
        {
            if (Bot.IsDebug)
            {
                Bot.DebugModule.Recalculate();
                return;
            }
            
            Action action = GetRandomAction();
            if (action != null)
            {
                action.Invoke();
            }
        }

        public void ReloadAction()
        {
            #if !UNITY_EDITOR && UNITY_WEBGL
            Application.OpenURL(Application.absoluteURL);
            #endif
        }

        public void SkipAction()
        {
            SkipMove();
        }

        public Action GetRandomAction()
        {
            float randomValue = UnityEngine.Random.Range(0f, 1f);
            float cumulativeProbability = 0f;

            foreach (var action in ActionsPossibilities)
            {
                cumulativeProbability += action.Value;
                if (randomValue < cumulativeProbability)
                {
                    return action.Key;
                }
            }

            return MakeMove;
        }
    }
}