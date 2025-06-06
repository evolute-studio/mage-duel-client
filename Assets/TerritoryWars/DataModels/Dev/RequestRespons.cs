using System;
using System.Collections.Generic;
using System.Linq;
using TerritoryWars.General;
using UnityEngine;

namespace TerritoryWars.DataModels.Dev
{
    [Serializable]
    public struct RequestResponse
    {
        public string Name;
        public ulong RequestTimestamp;
        public ulong TransactionHashTimestamp;
        public ulong ResponseTimestamp;
        private Delegate _eventHandler;

        private bool _isEventHandled;
        
        public static List<ulong> TotalDurations = new List<ulong>();
        public static List<ulong> TxTotalDurations = new List<ulong>();
        
        
        public static float AvgTotalDuration =>
            TotalDurations.Count > 0 ? (float)TotalDurations.Select(x => (double)x).Average() : 0;
        public static float MinTotalDuration =>
            TotalDurations.Count > 0 ? (float)TotalDurations.Min() : 0;
        public static float MaxTotalDuration =>
            TotalDurations.Count > 0 ? (float)TotalDurations.Max() : 0;
        
        public static float AvgTxTotalDuration =>
            TxTotalDurations.Count > 0 ? (float)TxTotalDurations.Select(x => (double)x).Average() : 0;
        public static float MinTxTotalDuration =>
            TxTotalDurations.Count > 0 ? (float)TxTotalDurations.Min() : 0;
        public static float MaxTxTotalDuration =>
            TxTotalDurations.Count > 0 ? (float)TxTotalDurations.Max() : 0;
        
        
        
        public RequestResponse Initialize<T>()
        {
            Name = typeof(T).Name;
            RequestTimestamp = (ulong)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            ResponseTimestamp = 0;
            TransactionHashTimestamp = 0;

            var response = this;
            _eventHandler = (Action<T>)((T evt) => response.OnEvent(evt));
            EventBus.Subscribe<T>((Action<T>)_eventHandler);
            return this;
        }

        private void OnEvent<T>(T evt)
        {
            if (_isEventHandled) return;
            ResponseTimestamp = (ulong)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            DebugLog();
            EventBus.Unsubscribe<T>((Action<T>)_eventHandler);
            _isEventHandled = true;
        }

        private void DebugLog()
        {
            if (_isEventHandled) return;
            TotalDurations.Add(ResponseTimestamp - RequestTimestamp);
            TxTotalDurations.Add(TransactionHashTimestamp - TransactionHashTimestamp);
            Debug.Log(
                $"RR: [{Name}], Tot Dur: {ResponseTimestamp - RequestTimestamp} ms. ");
            DebugGeneralLog();
        }

        public static void DebugGeneralLog()
        {
            Debug.Log($"RR: AvgTDur: {AvgTotalDuration/1000f} s, Min: {MinTotalDuration/1000f} s, Max: {MaxTotalDuration/1000f} s");
        }
    }
}