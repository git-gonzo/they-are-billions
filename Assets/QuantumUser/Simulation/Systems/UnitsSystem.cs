namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class UnitsSystem : SystemMainThreadFilter<UnitsSystem.Filter>,
        ISignalCreateUnit,
        ISignalOnEntityPrototypeMaterialized,
        ISignalOnEntityDie
    {
        public void CreateUnit(Frame f, EntityRef playerEntity)
        {
            var config = f.SimulationConfig.GameConfig;
            var unit = f.Create(config.workerPrototype);
            var unitComponent = f.Unsafe.GetPointer<UnitComponent>(unit);
            unitComponent->playerOwner = playerEntity;
            f.FindAsset(unitComponent->unitAsset).Init(f, unit);
            f.Unsafe.GetPointer<Transform3D>(unit)->Position = SpawnPointHelper.GetSpawnPoint<WorkerSpawnPointComponent>(f);
            f.Events.UpdateWorkers(playerEntity);
        }

        public void OnEntityPrototypeMaterialized(Frame f, EntityRef entity, EntityPrototypeRef prototypeRef)
        {
            if(f.TryGet<UnitComponent>(entity, out var unit)) 
            {
                var pathfinder = f.Unsafe.GetPointer<NavMeshSteeringAgent>(entity);
                pathfinder->MaxSpeed = unit.Speed;
            }
        }

        public struct Filter
        {
            public EntityRef Entity;
            public UnitComponent* UnitComponent;
            public Transform3D* Transform;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            var config = f.SimulationConfig.GameConfig;
            var unit = filter.UnitComponent;

            f.FindAsset(unit->unitAsset).UpdateUnit(f, filter.Entity);
        }

        public void OnEntityDie(Frame f, EntityRef entity)
        {
            f.Destroy(entity);
        }
    }
}
