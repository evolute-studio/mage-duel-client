using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dojo.Starknet;
using Dojo.Torii;
using dojo_bindings;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

namespace Dojo
{
    public class SynchronizationMaster : MonoBehaviour
    {
        public WorldManager worldManager;
        // Handle entities that get synchronized
        private ModelInstance[] _models;
        // Returns all of the model definitions
        private ModelInstance[] models => _models ??= LoadModels();

        public UnityEvent<List<GameObject>> OnSynchronized;
        public UnityEvent<GameObject> OnEntitySpawned;
        public UnityEvent<ModelInstance> OnModelUpdated;
        public UnityEvent<ModelInstance> OnEventMessage;
        public UnityEvent<TokenBalance> OnTokenBalanceUpdated;
        public UnityEvent<Token> OnTokenUpdated;

        // Awake is called when the script instance is being loaded.
        void Awake()
        {
            // We don't want our model definitions to be active.
            // Only used as templates for the actual entities to use.
            foreach (var model in models)
            {
                model.enabled = false;
            }
        }

        // Fetch all entities from the dojo world and spawn them.
        public async Task<int> SynchronizeEntities(Query query = null)
        {
            if (query == null)
            {
                query = worldManager.dojoConfig.query;
            }

            // Fetch entities from the world
#if UNITY_WEBGL && !UNITY_EDITOR
            var entities = await worldManager.wasmClient.Entities(query);
#else
            var entities = await Task.Run(() => worldManager.toriiClient.Entities(query));
#endif

            var entityGameObjects = new List<GameObject>();
            foreach (var entity in entities.items)
            {
                entityGameObjects.Add(SpawnEntity(entity.HashedKeys, entity.Models.Values.ToArray()));
            }

            OnSynchronized?.Invoke(entityGameObjects);
            return entities.items.Length;
        }

        // Spawn an Entity game object from a dojo.Entity
        private GameObject SpawnEntity(FieldElement hashedKeys, Model[] entityModels)
        {
            // Add the entity to the world.
            var entityGameObject = worldManager.AddEntity(hashedKeys.Hex());
            // Initialize each one of the entity models
            foreach (var entityModel in entityModels)
            {
                string[] parts = entityModel.Name.Split('-');
                string @namespace = parts[0];
                string name = parts[1];

                // Check if we have a model definition for this entity model
                var model = models.FirstOrDefault(m => m.GetType().Name == $"{@namespace}_{name}");
                if (model == null)
                {
                    Debug.LogWarning($"Model {entityModel.Name} not found");
                    continue;
                }

                ModelInstance component = entityGameObject.GetComponent(model.GetType()) as ModelInstance;
                
                if (component == null)
                {
                    // we dont need to initialize the component
                    // because it'll get updated
                    // Add the model component to the entity
                    component = (ModelInstance)entityGameObject.AddComponent(model.GetType());
                    component.Initialize(entityModel);
                }
                component.OnUpdate(entityModel);
                OnModelUpdated?.Invoke(component);
            }

            OnEntitySpawned?.Invoke(entityGameObject);
            return entityGameObject;
        }

        // Handles spawning / updating entities as they are updated from the dojo world
        private void HandleEntityUpdate(FieldElement hashedKeys, Model[] entityModels)
        {
            // Get the entity game object
            var entity = GameObject.Find(hashedKeys.Hex());
            if (entity == null)
            {
                entity = SpawnEntity(hashedKeys, entityModels);
                // We don't need to update the entity models
                return;
            }

            // Update each one of the entity models
            foreach (var entityModel in entityModels)
            {
                string[] parts = entityModel.Name.Split('-');
                string @namespace = parts[0];
                string name = parts[1];
                var model = models.FirstOrDefault(m => m.GetType().Name == $"{@namespace}_{name}");
                if (model == null)
                {
                    Debug.LogWarning($"Model {entityModel.Name} not found");
                    continue;
                }

                var component = entity.GetComponent(model.GetType());
                if (component == null)
                {
                    // we dont need to initialize the component
                    // because it'll get updated
                    component = (ModelInstance)entity.AddComponent(model.GetType());
                }

                // update component with new model data
                ((ModelInstance)component).OnUpdate(entityModel);
                OnModelUpdated?.Invoke((ModelInstance)component);
            }
        }

        private void HandleEventMessage(FieldElement hashedKeys, Model[] entityModels)
        {
            foreach (var entityModel in entityModels)
            {
                string[] parts = entityModel.Name.Split('-');
                string @namespace = parts[0];
                string name = parts[1];
                var model = models.FirstOrDefault(m => m.GetType().Name == $"{@namespace}_{name}");
                if (model == null)
                {
                    Debug.LogWarning($"Model {entityModel.Name} not found");
                    continue;
                }

                model.OnUpdate(entityModel);
                OnEventMessage?.Invoke(model);
            }
        }

        // Register our entity callbacks
        public void RegisterEntityCallbacks()
        {
            ToriiEvents.Instance.OnEntityUpdated += HandleEntityUpdate;
        }

        // Register event message callbacks
        public void RegisterEventMessageCallbacks()
        {
            ToriiEvents.Instance.OnEventMessageUpdated += HandleEventMessage;
        }

        // Register token callbacks
        public void RegisterTokenCallbacks()
        {
            ToriiEvents.Instance.OnTokenUpdated += (token) =>
            {
                OnTokenUpdated?.Invoke(token);
            };
        }

        // Register token balance callbacks
        public void RegisterTokenBalanceCallbacks()
        {
            ToriiEvents.Instance.OnTokenBalanceUpdated += (tokenBalance) =>
            {
                OnTokenBalanceUpdated?.Invoke(tokenBalance);
            };
        }


        private ModelInstance[] LoadModels()
        {
            List<ModelInstance> models = new();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                var modelTypes = assembly.GetTypes()
                    .Where(t => typeof(ModelInstance).IsAssignableFrom(t) && !t.IsAbstract);

                foreach (Type modelType in modelTypes)
                {
                    models.Add((ModelInstance)gameObject.AddComponent(modelType));
                }
            }

            return models.ToArray();
        }
    }
}