namespace Quantum
{
    public partial class SimulationConfig : AssetObject
    {
        public int initialWorkers;
        public AssetRef<EntityPrototype> workerPrototype;
    }
}