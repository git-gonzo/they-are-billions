using Photon.Deterministic;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Quantum
{
    public class GameConfig : AssetObject
    {
        public int initialWorkers = 2;
        public int initialTroops;
        public AssetRef<EntityPrototype> workerPrototype;
        public FP workerStopDistance = 2;
        public List<ResourceAmount> initialResources;
        public List<BuildingConfig> allBuildings;
        [Header("UNITS")]
        public TroopsConfig troopsConfig;
        [Header("COSTS")]
        public ResourceAmount workerCost;
    }

    [Serializable]
    public class TroopsConfig
    {
        public AssetRef<EntityPrototype> basicTroopPrototype;
    }

}