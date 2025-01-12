namespace Quantum
{
    using System;
    using UnityEngine;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class EconomySystem : SystemSignalsOnly,
        ISignalOnResourceAdded, 
        ISignalOnComponentAdded<PlayerEconomyComponent>,
        ISignalConsumeCost
    {
        public void OnAdded(Frame f, EntityRef entity, PlayerEconomyComponent* component)
        {
            //Add initial resources
            var config = f.SimulationConfig.GameConfig;
            component->resources = f.AllocateList<ResourceAmount>();
            var resources = f.ResolveList(component->resources);
            for (int i = 0; i < config.initialResources.Count; i++)
            {
                var resource = new ResourceAmount{ Resource = config.initialResources[i].Resource, Amount = config.initialResources[i].Amount };
                resources.Add(resource);
            }
            f.Events.UpdateResources(entity);
        }

        public void OnResourceAdded(Frame f, EntityRef playerEntity, ResourceAmount resource)
        {
            //TODO: Add player entity
            Debug.Log("Resource collected " + resource.Resource);
            var economy = f.Unsafe.GetPointer<PlayerEconomyComponent>(playerEntity);
            var resources = f.ResolveList(economy->resources);
            var found = false;
            for ( var i = 0; i < resources.Count; i++)
            {
                if (resources[i].Resource == resource.Resource)
                {
                    Debug.Log("Resource " + resource.Resource + " updated");
                    var r = resources[i];
                    r.Amount += resource.Amount;
                    resources[i] = r;
                    found = true;
                }
            }
            if (!found)
            {
                Debug.Log("New Resource added " + resource.Resource);
                resources.Add(resource);
            }
            f.Events.UpdateResources(playerEntity);
        }

        public void ConsumeCost(Frame f, EntityRef playerEntity, ResourceAmount resource)
        {
            Debug.Log("Consume Cost signal, " + resource.Resource + "-" + resource.Amount);
            var economy = f.Unsafe.GetPointer<PlayerEconomyComponent>(playerEntity);
            var resources = f.ResolveList(economy->resources);
            for (var i = 0; i < resources.Count; i++)
            {
                if (resources[i].Resource == resource.Resource)
                {
                    Debug.Log("Resource " + resource.Resource + " updated");
                    var r = resources[i];
                    r.Amount -= resource.Amount;
                    resources[i] = r;
                    f.Events.UpdateResources(playerEntity);
                    return;
                }
            }
        }
    }
}
