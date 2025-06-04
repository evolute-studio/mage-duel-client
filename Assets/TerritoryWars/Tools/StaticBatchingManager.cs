using System.Collections.Generic;
using UnityEngine;

namespace TerritoryWars.Tools
{
    public static class StaticBatchingManager
    {
        private static Dictionary<Material, List<GameObject>> _staticBatchesDictionary = new Dictionary<Material, List<GameObject>>();
        
        public static void RegisterStaticBatch(GameObject gameObject, Material material)
        {
            if (gameObject == null || material == null)
            {
                return;
            }

            if (!_staticBatchesDictionary.ContainsKey(material))
            {
                _staticBatchesDictionary[material] = new List<GameObject>();
            }
            
            _staticBatchesDictionary[material].Add(gameObject);
            gameObject.isStatic = true;
            StaticBatchingUtility.Combine(_staticBatchesDictionary[material].ToArray(), _staticBatchesDictionary[material][0]);
        }
    }
}