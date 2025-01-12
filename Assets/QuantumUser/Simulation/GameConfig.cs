using Photon.Deterministic;
using System.Collections.Generic;
using UnityEngine;

namespace Quantum
{
    public class GameConfig : AssetObject
    {
        public int initialWorkers = 2;
        public AssetRef<EntityPrototype> workerPrototype;
        public FP workerStopDistance = 2;
        public List<ResourceAmount> initialResources;
        [Header("COSTS")]
        public ResourceAmount workerCost;
    }
}