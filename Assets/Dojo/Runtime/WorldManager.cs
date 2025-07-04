using System.Linq;
using dojo_bindings;
using UnityEngine;
using Dojo.Torii;
using System;
using System.Collections.Generic;
using Dojo.Starknet;
using System.Threading.Tasks;

namespace Dojo
{
    public class WorldManager : MonoBehaviour
    {
        public SynchronizationMaster synchronizationMaster;
#if UNITY_WEBGL && !UNITY_EDITOR
        public ToriiWasmClient wasmClient;
#else
        public ToriiClient toriiClient;
#endif
        [SerializeField] public WorldManagerData dojoConfig;

        public async void Initialize(string rpcUrl = null, string toriiUrl = null, string worldAddress = null)
        {
            string rpc = rpcUrl ?? dojoConfig.rpcUrl;
            string torii = toriiUrl ?? dojoConfig.toriiUrl;
            FieldElement world = worldAddress != null ? new FieldElement(worldAddress) : dojoConfig.worldAddress;
            Debug.Log($"[WorldManager] Initializing with rpcUrl: {rpc} and toriiUrl: {torii} and worldAddress: {world.Hex()}");
            
#if UNITY_WEBGL && !UNITY_EDITOR
            wasmClient = new ToriiWasmClient(torii, dojoConfig.relayWebrtcUrl, world);
            await wasmClient.CreateClient();
#else
            toriiClient = new ToriiClient(torii, dojoConfig.relayUrl, world);
#endif

            /*  fetch entities from the world
                TODO: maybe do in the start function of the SynchronizationMaster?
                problem is when to start the subscription service
            */
            //await synchronizationMaster.SynchronizeEntities();

            // listen for entity updates
            synchronizationMaster.RegisterEntityCallbacks();
            synchronizationMaster.RegisterEventMessageCallbacks();
        }

        // #if UNITY_WEBGL && !UNITY_EDITOR
        //         // internal callback to be called for when the client is created
        //         // on the wasm sdk. 
        //         public void OnClientCreated(float clientPtr)
        //         {
        //             toriiClient.wasmClientPtr = (IntPtr)clientPtr;
        //             // we dont start the subscription service
        //             // because wasm already does it.

        //             // fetch entities from the world
        //             // TODO: maybe do in the start function of the SynchronizationMaster?
        //             // problem is when to start the subscription service
        //             synchronizationMaster.SynchronizeEntities();

        //             // listen for entity updates
        //             synchronizationMaster.RegisterEntityCallbacks();
        //         }
        // #endif

        /*  Get a child entity from the WorldManager game object.
            Name is usually the hashed_keys of the entity as a hex string.
        */
        public GameObject Entity(string name)
        {
            var entity = transform.Find(name);
            if (entity == null)
            {
                Debug.LogError($"Entity {name} not found");
                return null;
            }

            return entity.gameObject;
        }

        // Return all children entities.
        public GameObject[] Entities()
        {
            return transform.Cast<Transform>().Select(t => t.gameObject).ToArray();
        }

        // Return all children entities.
        // That have the specified component.
        public GameObject[] Entities<T>() where T : Component
        {
            return transform.Cast<Transform>()
                .Select(t => t.gameObject)
                .Where(g => g.GetComponent<T>() != null)
                .ToArray();
        }
        
        public T[] EntityModels<T>() where T : ModelInstance
        {
            return transform.Cast<Transform>()
                .Select(t => t.gameObject)
                .Select(g => g.GetComponent<T>())
                .Where(c => c != null)
                .ToArray();
        }
        
        public T EntityModel<T>(string fieldName = null, object value = null) where T : ModelInstance
        {
            if (fieldName == null && value == null)
            {
                return transform.Cast<Transform>()
                    .Select(t => t.gameObject)
                    .Select(g => g.GetComponent<T>())
                    .FirstOrDefault();
            }
            return EntityModel<T>(new Dictionary<string, object>{ { fieldName, value } });
        }
        
        public T EntityModel<T>(Dictionary<string, object> filters) where T : ModelInstance
        {
            return transform.Cast<Transform>()
                .Select(t => t.gameObject)
                .Select(g => g.GetComponent<T>())
                .Where(c => c != null)
                .FirstOrDefault(c => 
                {
                    foreach (var filter in filters)
                    {
                        var field = c.GetType().GetField(filter.Key);
                        if (field == null)
                        {
                            Debug.LogWarning($"Field {filter.Key} not found in component {typeof(T).Name}");
                            return false;
                        }

                        var value = field.GetValue(c);
                        if (value == null)
                        {
                            Debug.LogWarning($"Field {filter.Key} has null value in component {typeof(T).Name}");
                            return false;
                        }

                        var filterValue = filter.Value;
                        if (value is FieldElement fieldElement)
                        {
                            value = fieldElement.Hex();
                            if (filterValue is FieldElement filterFieldElement)
                            {
                                filterValue = filterFieldElement.Hex();
                            }
                        }

                        if (!value.Equals(filterValue))
                        {
                            Debug.LogWarning($"Field {filter.Key} with value {value} does not match filter value {filterValue} in component {typeof(T).Name}");
                            return false;
                        }
                    }
                    return true;
                });
        }

        // Add a new entity game object as a child of the WorldManager game object.
        public GameObject AddEntity(string key)
        {
            // check if entity already exists
            var entity = transform.Find(key)?.gameObject;
            if (entity != null)
            {
                Debug.LogWarning($"Entity {key} already exists");
                return entity.gameObject;
            }

            entity = new GameObject(key);
            entity.transform.parent = transform;

            return entity;
        }

        // Remove an entity game object from the WorldManager game object.
        public void RemoveEntity(string key)
        {
            var entity = transform.Find(key);
            if (entity != null)
            {
                Destroy(entity.gameObject);
            }
        }

        public async Task<byte[]> Publish(TypedData typedData, FieldElement[] signature)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return await wasmClient.PublishMessage(typedData, signature);
#else
            return await Task.Run(() => toriiClient.PublishMessage(typedData, signature).ToArray());
#endif
        }
    }
}
