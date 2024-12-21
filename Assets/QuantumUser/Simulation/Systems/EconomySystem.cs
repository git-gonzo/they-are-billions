namespace Quantum
{
    using Photon.Deterministic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class EconomySystem : SystemSignalsOnly, ISignalOnResourceAdded
    {
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
        }
    }
}
