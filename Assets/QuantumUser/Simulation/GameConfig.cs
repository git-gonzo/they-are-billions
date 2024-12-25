using Photon.Deterministic;

namespace Quantum
{
    public class GameConfig : AssetObject
    {
        public int initialWorkers = 2;
        public AssetRef<EntityPrototype> workerPrototype;
        public FP workerStopDistance = 2;
        public int workerCostAmount = 5;
        public ResourceType workerCostResource = ResourceType.Wood;
    }
}